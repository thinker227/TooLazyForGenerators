using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using TooLazyForGenerators.Pipelines;

namespace TooLazyForGenerators;

/// <summary>
/// A builder for a <see cref="LazyGenerator"/>.
/// </summary>
public sealed class LazyGeneratorBuilder
{
    private readonly List<FileInfo> projectFiles = new();
    private readonly List<Type> outputs = new();
    private readonly List<PipelineStep> pipelineSteps = new();
    
    /// <summary>
    /// The cancellation token for the builder.
    /// </summary>
    public CancellationToken CancellationToken { get; }
    
    /// <summary>
    /// The services for the generator.
    /// </summary>
    public IServiceCollection Services { get; }

    /// <summary>
    /// Initializes a new <see cref="LazyGeneratorBuilder"/> instance.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token for the builder.</param>
    public LazyGeneratorBuilder(CancellationToken cancellationToken = default)
    {
        CancellationToken = cancellationToken;
        Services = new ServiceCollection();
    }

    /// <summary>
    /// Adds a project for the generator to target.
    /// </summary>
    /// <param name="projectFile">The project (<c>.csproj</c>) file of the project.</param>
    public LazyGeneratorBuilder TargetingProject(FileInfo projectFile)
    {
        projectFiles.Add(projectFile);
        return this;
    }
    
    /// <summary>
    /// Adds a project with a specified file path for the generator to target.
    /// </summary>
    /// <param name="projectFilePath">The file path to the project (<c>.csproj</c>) file of the project.</param>
    public LazyGeneratorBuilder TargetingProject(
        string projectFilePath)
    {
        FileInfo file = new(projectFilePath);
        if (!file.Exists)
            throw new InvalidOperationException($"File '{projectFilePath}' does not exist.");

        TargetingProject(file);
        
        return this;
    }
    
    /// <summary>
    /// Adds a project with a specified name for the generator to target
    /// by recursively searching for a solution in any parent directory of the current directory.
    /// </summary>
    /// <param name="projectName">The name of the project to target.</param>
    /// <remarks>
    /// This method is incredibly slow and should not be used if speed is desired,
    /// for instance if running as a build step,
    /// in which case <see cref="TargetingProject(string)"/> with a hardcoded project file path
    /// or project file path supplied through command-line arguments would be more appropriate. 
    /// </remarks>
    public async Task<LazyGeneratorBuilder> TargetingProjectWithName(
        string projectName)
    {
        DirectoryInfo currentDirectory = new(Directory.GetCurrentDirectory());
        var solutionPath = TryGetSolutionInParentDirectories(currentDirectory);

        if (solutionPath is null) return this;

        using var workspace = WorkspaceUtils.CreateWorkspace();
        var solution = await workspace.OpenSolutionAsync(
            solutionFilePath: solutionPath,
            cancellationToken: CancellationToken);

        var project = solution.Projects.FirstOrDefault(p => p.Name == projectName);
        if (project?.FilePath is null) return this;

        TargetingProject(project.FilePath);
        
        return this;
    }
    
    private static string? TryGetSolutionInParentDirectories(DirectoryInfo directory)
    {
        while (true)
        {
            var solutionFiles = directory.GetFiles("*.sln", SearchOption.TopDirectoryOnly);
            
            if (solutionFiles.Length == 1) return solutionFiles[0].FullName;
            
            if (directory.Parent is null) return null;
            directory = directory.Parent;
        }
    }
    
    /// <summary>
    /// Registers an output for the generator to use.
    /// </summary>
    /// <param name="outputType">The type of the output to register.
    /// The type has to implement <see cref="ISourceOutput"/>.</param>
    public LazyGeneratorBuilder WithOutput(Type outputType)
    {
        outputs.Add(outputType);
        return this;
    }
    
    /// <summary>
    /// Registers an <see cref="ISourceOutput"/> for the generator to use.
    /// </summary>
    /// <typeparam name="T">The type of the output to register.</typeparam>
    public LazyGeneratorBuilder WithOutput<T>()
        where T : ISourceOutput, new() =>
        WithOutput(typeof(T));
    
    /// <summary>
    /// Registers all outputs from a specified assembly or the calling assembly for the generator to use. 
    /// </summary>
    /// <param name="assembly">The assembly to register all outputs from.
    /// If <see langword="null"/> then the calling assembly will be used.</param>
    public LazyGeneratorBuilder WithOutputsFromAssembly(
        Assembly? assembly = null)
    {
        assembly ??= Assembly.GetCallingAssembly();

        var types = assembly.GetTypes()
            .Where(ImplementsISourceOutputT)
            .ToArray();

        foreach (var type in types)
        {
            WithOutput(type);
        }

        return this;
    }
    
    private static bool ImplementsISourceOutputT(Type type) => type.GetInterfaces()
        .Any(i => i == typeof(ISourceOutput));
    
    /// <summary>
    /// Adds a pipeline step the generator pipeline should use.
    /// </summary>
    /// <param name="pipelineStep">The step to add.</param>
    public LazyGeneratorBuilder Using(PipelineStep pipelineStep)
    {
        pipelineSteps.Add(pipelineStep);
        return this;
    }

    /// <summary>
    /// Builds the generator.
    /// </summary>
    public LazyGenerator Build() => new(
        projectFiles,
        outputs,
        pipelineSteps,
        CancellationToken,
        Services.BuildServiceProvider());
}
