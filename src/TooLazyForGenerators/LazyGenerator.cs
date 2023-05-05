using System.Collections.Concurrent;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.Extensions.DependencyInjection;
using TooLazyForGenerators.Pipelines;

namespace TooLazyForGenerators;

/// <summary>
/// A generator which takes one or multiple projects and adds source code to them.
/// </summary>
public sealed class LazyGenerator 
{
    private readonly IReadOnlyCollection<FileInfo> projectFiles;
    
    private readonly IReadOnlyCollection<Type> outputs;
    
    private readonly IReadOnlyList<PipelineStep> ripelineSteps;

    private readonly CancellationToken cancellationToken;
    
    private readonly IServiceProvider services;

    /// <summary>
    /// Initializes a new <see cref="LazyGenerator"/> instance.
    /// </summary>
    /// <param name="projectFiles">The project files the generator targets.</param>
    /// <param name="outputs">The output types implementing <see cref="ISourceOutput"/> the generator will call.</param>
    /// <param name="ripelineSteps">The steps of the generator pipeline.</param>
    /// <param name="cancellationToken">The cancellation token for the generator.</param>
    /// <param name="services">The services for the generator.</param>
    public LazyGenerator(IReadOnlyCollection<FileInfo> projectFiles, IReadOnlyCollection<Type> outputs, IReadOnlyList<PipelineStep> ripelineSteps, CancellationToken cancellationToken, IServiceProvider services)
    {
        this.projectFiles = projectFiles;
        this.outputs = outputs;
        this.ripelineSteps = ripelineSteps;
        this.cancellationToken = cancellationToken;
        this.services = services;
    }

    /// <summary>
    /// Runs the generator.
    /// </summary>
    /// <returns>The output from the generator.</returns>
    public async Task<IGeneratorOutput> Run()
    {
        var workspace = WorkspaceUtils.CreateWorkspace();

        var results = await Task.WhenAll(projectFiles
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

        using var serviceScope = services.CreateScope();
        
        var runner = new GeneratorOutputRunner()
        {
            PipelineSteps = ripelineSteps,
            Files = files,
            Errors = errors,
            Project = project,
            CancellationToken = cancellationToken,
            ServiceScope = serviceScope
        };

        await Task.WhenAll(outputs
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
            cancellationToken: cancellationToken);

    

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
