using Microsoft.Extensions.DependencyInjection;
using TooLazyForGenerators.Pipelines;

namespace TooLazyForGenerators;

/// <summary>
/// A builder for a <see cref="LazyGenerator"/>.
/// </summary>
public sealed class LazyGeneratorBuilder : ILazyGeneratorBuilder, IPipelineBuilder
{
    private readonly List<FileInfo> projectFiles = new();
    private readonly List<Type> outputs = new();
    private readonly List<PipelineStep> pipelineSteps = new();
    
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

    public LazyGeneratorBuilder TargetingProject(FileInfo projectFile)
    {
        projectFiles.Add(projectFile);
        return this;
    }

    ILazyGeneratorBuilder ILazyGeneratorBuilder.TargetingProject(FileInfo projectFile) =>
        TargetingProject(projectFile);
    
    public LazyGeneratorBuilder WithOutput(Type outputType)
    {
        outputs.Add(outputType);
        return this;
    }

    ILazyGeneratorBuilder ILazyGeneratorBuilder.WithOutput(Type outputType) =>
        WithOutput(outputType);
    
    public LazyGeneratorBuilder Using(PipelineStep pipelineStep)
    {
        pipelineSteps.Add(pipelineStep);
        return this;
    }

    IPipelineBuilder IPipelineBuilder.Using(PipelineStep pipelineStep) =>
        Using(pipelineStep);
    
    public ILazyGenerator Build() => new LazyGenerator()
    {
        ProjectFiles = projectFiles,
        Outputs = outputs,
        PipelineSteps = pipelineSteps,
        CancellationToken = CancellationToken,
        Services = Services.BuildServiceProvider()
    };
}
