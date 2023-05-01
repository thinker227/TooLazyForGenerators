namespace TooLazyForGenerators.Pipelines;

public interface IPipelineBuilder
{
    IPipelineBuilder Using(PipelineStep pipelineStep);
}
