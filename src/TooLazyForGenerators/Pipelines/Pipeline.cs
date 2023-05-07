using System.Collections.Concurrent;
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
public sealed class PipelineContext : ISourceAndErrors
{
    private readonly ConcurrentBag<SourceFile> files;
    private readonly ConcurrentBag<Error> errors;

    /// <summary>
    /// The target type of the pipeline.
    /// </summary>
    public Type TargetType { get; set; }
    
    /// <summary>
    /// The target project.
    /// </summary>
    public Project Project { get; set; }
    
    /// <summary>
    /// The cancellation token for the generator.
    /// </summary>
    public CancellationToken CancellationToken { get; }
    
    /// <summary>
    /// The services for the generator.
    /// </summary>
    public IServiceProvider Services { get; set; }

    public PipelineContext(
        Type targetType,
        Project project,
        CancellationToken cancellationToken,
        IServiceProvider services,
        ConcurrentBag<SourceFile> files,
        ConcurrentBag<Error> errors)
    {
        this.files = files;
        this.errors = errors;
        TargetType = targetType;
        Project = project;
        CancellationToken = cancellationToken;
        Services = services;
    }

    public void AddSource(SourceFile file) =>
        files.Add(file);

    public void AddError(Error error) =>
        errors.Add(error);
}
