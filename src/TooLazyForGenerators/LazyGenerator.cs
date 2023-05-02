using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using TooLazyForGenerators.Pipelines;

namespace TooLazyForGenerators;

/// <summary>
/// A standard implementation of <see cref="ILazyGenerator"/>. 
/// </summary>
public sealed class LazyGenerator : ILazyGenerator
{
    /// <summary>
    /// The project files the generator targets.
    /// </summary>
    public required IReadOnlyCollection<FileInfo> ProjectFiles { get; init; }
    
    /// <summary>
    /// The output types implementing <see cref="ISourceOutput"/> the generator will call.
    /// </summary>
    public required IReadOnlyCollection<Type> Outputs { get; init; }
    
    /// <summary>
    /// The steps of the generator pipeline.
    /// </summary>
    public required IReadOnlyList<PipelineStep> PipelineSteps { get; init; }

    /// <summary>
    /// The cancellation token for the generator.
    /// </summary>
    public required CancellationToken CancellationToken { get; init; }
    
    /// <summary>
    /// The services for the generator.
    /// </summary>
    public required IServiceProvider Services { get; init; }

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

        var runner = new GeneratorOutputRunner()
        {
            PipelineSteps = PipelineSteps,
            Files = files,
            Errors = errors,
            Project = project,
            CancellationToken = CancellationToken,
            Services = Services
        };
        
        foreach (var outputType in Outputs)
        {
            await runner.Run(outputType);
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
