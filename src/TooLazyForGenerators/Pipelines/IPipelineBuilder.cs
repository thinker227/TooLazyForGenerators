namespace TooLazyForGenerators.Pipelines;

/// <summary>
/// A builder for a generator supporting pipelines.
/// </summary>
public interface IPipelineBuilder
{
    /// <summary>
    /// Adds a pipeline step the generator pipeline should use.
    /// </summary>
    /// <param name="pipelineStep">The step to add.</param>
    IPipelineBuilder Using(PipelineStep pipelineStep);
}
