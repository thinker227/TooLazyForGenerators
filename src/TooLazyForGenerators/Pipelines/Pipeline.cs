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

/// <summary>
/// A context for a pipeline step.
/// </summary>
public sealed class PipelineContext
{
    /// <summary>
    /// The target type of the pipeline.
    /// </summary>
    public required Type TargetType { get; set; }
    
    /// <summary>
    /// The function called to create an instance of <see cref="ISourceOutput"/> for the target type.
    /// </summary>
    public required Func<Type, IServiceProvider, ISourceOutput> CreateTarget { get; set; }
    
    /// <summary>
    /// The target project.
    /// </summary>
    public required Project Project { get; set; }
    
    /// <summary>
    /// The cancellation token for the generator.
    /// </summary>
    public required CancellationToken CancellationToken { get; init; }
    
    /// <summary>
    /// The services for the generator.
    /// </summary>
    public required IServiceProvider Services { get; set; }
}
