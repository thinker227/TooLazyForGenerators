using Microsoft.CodeAnalysis;

namespace TooLazyForGenerators.Pipelines;

/// <summary>
/// A delegate for a pipeline step.
/// </summary>
/// <param name="ctx">The output context passed through the pipeline.</param>
/// <param name="next">The next step in the pipeline.
/// Call this function to continue the pipeline, or do not to abort it.</param>
public delegate Task PipelineStep(PipelineContext ctx, PipelineContinuation next);

/// <summary>
/// A delegate which continues a pipeline.
/// </summary>
/// <param name="ctx">The output context to pass to the next pipeline step.</param>
public delegate Task PipelineContinuation(PipelineContext ctx);

public sealed class PipelineContext
{
    public required Type TargetType { get; set; }
    
    public required Func<TargetCreationContext, ISourceOutput> CreateTarget { get; set; }
    
    public required Project Project { get; set; }
    
    public required CancellationToken CancellationToken { get; init; }
    
    public required IServiceProvider? Services { get; init; }
}

public readonly record struct TargetCreationContext(
    Type TargetType,
    IServiceProvider? Services);
