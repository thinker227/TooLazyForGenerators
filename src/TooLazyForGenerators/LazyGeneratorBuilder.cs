using TooLazyForGenerators.Pipelines;

namespace TooLazyForGenerators;

/// <summary>
/// A builder for an <see cref="ILazyGenerator"/>.
/// </summary>
public sealed class LazyGeneratorBuilder : ILazyGeneratorBuilder, IPipelineBuilder
{
    private readonly List<FileInfo> projectFiles = new();
    private readonly List<Type> outputs = new();
    private readonly List<PipelineStep> pipelineSteps = new();
    
    public CancellationToken CancellationToken { get; }

    public LazyGeneratorBuilder(CancellationToken cancellationToken = default) =>
        CancellationToken = cancellationToken;

    /// <summary>
    /// Adds a project for the generator to target.
    /// </summary>
    /// <param name="projectFile">The project (<c>.csproj</c>) file of the project.</param>
    public LazyGeneratorBuilder TargetingProject(FileInfo projectFile)
    {
        projectFiles.Add(projectFile);
        return this;
    }

    ILazyGeneratorBuilder ILazyGeneratorBuilder.TargetingProject(FileInfo projectFile) =>
        TargetingProject(projectFile);
    
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

    ILazyGeneratorBuilder ILazyGeneratorBuilder.WithOutput(Type outputType) =>
        WithOutput(outputType);
    
    /// <summary>
    /// Adds a pipeline step the generator pipeline should use.
    /// </summary>
    /// <param name="pipelineStep">The step to add.</param>
    public LazyGeneratorBuilder Using(PipelineStep pipelineStep)
    {
        pipelineSteps.Add(pipelineStep);
        return this;
    }

    IPipelineBuilder IPipelineBuilder.Using(PipelineStep pipelineStep) =>
        Using(pipelineStep);
    
    /// <summary>
    /// Builds the generator.
    /// </summary>
    public ILazyGenerator Build() => new LazyGenerator()
    {
        ProjectFiles = projectFiles,
        Outputs = outputs,
        PipelineSteps = pipelineSteps,
        CancellationToken = CancellationToken
    };
}
