namespace TooLazyForGenerators.Pipelines;

public delegate Task PipelineStep(ISourceOutputContext ctx, PipelineContinuation next);

public delegate Task PipelineContinuation(ISourceOutputContext ctx);
