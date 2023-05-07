using System.Collections.Concurrent;
using Microsoft.CodeAnalysis;

namespace TooLazyForGenerators;

/// <summary>
/// A context for an <see cref="ISourceOutput"/>.
/// </summary>
public readonly struct SourceOutputContext : ISourceAndErrors
{
    private readonly ConcurrentBag<SourceFile> files;
    private readonly ConcurrentBag<Error> errors;
    
    /// <summary>
    /// The project which is being targeted.
    /// </summary>
    public Project Project { get; }
    
    /// <summary>
    /// The cancellation token for the operation.
    /// </summary>
    public CancellationToken CancellationToken { get; }

    /// <summary>
    /// Initializes a new <see cref="SourceOutputContext"/> instance.
    /// </summary>
    /// <param name="project">The project which is being targeted.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <param name="files">A collection of source files to add files to.</param>
    /// <param name="errors">A collection of errors to add errors to.</param>
    public SourceOutputContext(
        Project project,
        CancellationToken cancellationToken,
        ConcurrentBag<SourceFile> files,
        ConcurrentBag<Error> errors)
    {
        this.files = files;
        this.errors = errors;
        Project = project;
        CancellationToken = cancellationToken;
    }

    public void AddSource(SourceFile file) =>
        files.Add(file);

    public void AddError(Error error) =>
        errors.Add(error);
}
