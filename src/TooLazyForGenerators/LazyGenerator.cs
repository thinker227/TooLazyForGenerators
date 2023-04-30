﻿using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using TooLazyForGenerators.Pipelines;

namespace TooLazyForGenerators;

internal sealed class LazyGenerator : ILazyGenerator
{
    public required IReadOnlyCollection<FileInfo> ProjectFiles { get; init; }
    
    public required IReadOnlyCollection<Type> Outputs { get; init; }
    
    public required IReadOnlyList<PipelineStep> PipelineSteps { get; init; }

    public required CancellationToken CancellationToken { get; init; }

    public async Task<IGeneratorOutput> Run(CancellationToken cancellationToken = default)
    {
        var workspace = WorkspaceUtils.CreateWorkspace();
        
        List<ProjectResult> results = new();
        foreach (var projectFile in ProjectFiles)
        {
            var result = await HandleProject(workspace, projectFile);
            results.Add(result);
        }

        return new GeneratorOutput(
            results.SelectMany(r => r.Files).ToArray(),
            results.SelectMany(r => r.Errors).ToArray(),
            workspace);
    }

    private async Task<ProjectResult> HandleProject(MSBuildWorkspace workspace, FileInfo projectFile)
    {
        var project = await GetProject(workspace, projectFile);
        var files = new List<SourceFile>();
        var errors = new List<Error>();
        
        SourceOutputContext ctx = new()
        {
            Project = project,
            CancellationToken = CancellationToken,
            Files = files,
            Errors = errors
        };
        
        foreach (var output in GetOutputInstances())
        {
            await CallPipeline(output, ctx, 0);
        }

        return new(
            files
                .Select(file => new ProjectSourceFile(project, file))
                .ToArray(),
            errors);
    }
    
    private Task<Project> GetProject(MSBuildWorkspace workspace, FileInfo projectFile) =>
        workspace.OpenProjectAsync(
            projectFilePath: projectFile.FullName,
            cancellationToken: CancellationToken);
    
    private IEnumerable<ISourceOutput> GetOutputInstances() => Outputs.Select(type =>
    {
        var ctor = type.GetConstructor(
            BindingFlags.Instance | BindingFlags.Public,
            Array.Empty<Type>());

        if (ctor is null)
            throw new InvalidOperationException($"{type.FullName} has no public parameterless constructor.");

        var instance = ctor.Invoke(null);
        return (ISourceOutput)instance;
    });

    private Task CallPipeline(ISourceOutput output, ISourceOutputContext ctx, int pipelineIndex) =>
        pipelineIndex >= PipelineSteps.Count
            ? output.GetSource(ctx)
            : PipelineSteps[pipelineIndex](ctx, newCtx =>
                CallPipeline(output, newCtx, pipelineIndex + 1));


    
    private readonly struct SourceOutputContext : ISourceOutputContext
    {
        public required Project Project { get; init; }
    
        public required CancellationToken CancellationToken { get; init; }
        
        public required ICollection<SourceFile> Files { get; init; }
        
        public required ICollection<Error> Errors { get; init; }

        public void AddSource(SourceFile file) => Files.Add(file);

        public void AddError(Error error) => Errors.Add(error);
    }

    private readonly record struct ProjectResult(
        IReadOnlyCollection<ProjectSourceFile> Files,
        IReadOnlyCollection<Error> Errors);

    private readonly record struct GeneratorOutput(
        IReadOnlyCollection<ProjectSourceFile> Files,
        IReadOnlyCollection<Error> Errors,
        MSBuildWorkspace Workspace) : IGeneratorOutput
    {
        Workspace IGeneratorOutput.Workspace => Workspace;

        public void Dispose() => Workspace.Dispose();
    }
}
