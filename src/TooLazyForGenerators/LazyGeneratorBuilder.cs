using TooLazyForGenerators.Pipelines;

namespace TooLazyForGenerators;

public sealed class LazyGeneratorBuilder : ILazyGeneratorBuilder, IPipelineBuilder
{
    private readonly List<FileInfo> projectFiles = new();
    private readonly List<Type> outputs = new();
    private readonly List<PipelineStep> pipelineSteps = new();
    
    public CancellationToken CancellationToken { get; }

    public LazyGeneratorBuilder(CancellationToken cancellationToken = default) =>
        CancellationToken = cancellationToken;

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
        CancellationToken = CancellationToken
    };
}
