namespace TooLazyForGenerators.Pipelines;

/// <summary>
/// A delegate for a pipeline step.
/// </summary>
/// <param name="ctx">The output context passed through the pipeline.</param>
/// <param name="next">The next step in the pipeline.
/// Call this function to continue the pipeline, or do not to abort it.</param>
public delegate Task PipelineStep(ISourceOutputContext ctx, PipelineContinuation next);

/// <summary>
/// A delegate which continues a pipeline.
/// </summary>
/// <param name="ctx">The output context to pass to the next pipeline step.</param>
public delegate Task PipelineContinuation(ISourceOutputContext ctx);
