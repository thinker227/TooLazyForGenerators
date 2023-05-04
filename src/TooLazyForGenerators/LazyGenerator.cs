using System.Collections.Concurrent;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.Extensions.DependencyInjection;
using TooLazyForGenerators.Pipelines;

namespace TooLazyForGenerators;

internal sealed class LazyGenerator : ILazyGenerator
{
    public required IReadOnlyCollection<FileInfo> ProjectFiles { get; init; }
    
    public required IReadOnlyCollection<Type> Outputs { get; init; }
    
    public required IReadOnlyList<PipelineStep> PipelineSteps { get; init; }

    public required CancellationToken CancellationToken { get; init; }
    
    public required IServiceProvider Services { get; init; }

    public async Task<IGeneratorOutput> Run(CancellationToken cancellationToken = default)
    {
        var workspace = WorkspaceUtils.CreateWorkspace();

        var results = await Task.WhenAll(ProjectFiles
            .Select(file => HandleProject(workspace, file)));

        return new GeneratorOutput(
            results.SelectMany(r => r.Files).ToArray(),
            results.SelectMany(r => r.Errors).ToArray(),
            workspace);
    }

    private async Task<ProjectResult> HandleProject(MSBuildWorkspace workspace, FileInfo projectFile)
    {
        var project = await GetProject(workspace, projectFile);
        var files = new ConcurrentBag<SourceFile>();
        var errors = new ConcurrentBag<Error>();

        using var serviceScope = Services.CreateScope();
        
        var runner = new GeneratorOutputRunner()
        {
            PipelineSteps = PipelineSteps,
            Files = files,
            Errors = errors,
            Project = project,
            CancellationToken = CancellationToken,
            ServiceScope = serviceScope
        };

        await Task.WhenAll(Outputs
            .Select(type => runner.Run(type)));

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
