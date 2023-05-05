using System.Collections.Concurrent;
using Microsoft.CodeAnalysis;

namespace TooLazyForGenerators;

/// <summary>
/// A context for an <see cref="ISourceOutput"/>.
/// </summary>
public readonly struct SourceOutputContext
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

    /// <summary>
    /// Adds a source file.
    /// </summary>
    /// <param name="file">The file to add.</param>
    public void AddSource(SourceFile file) =>
        files.Add(file);
    
    /// <summary>
    /// Adds a source file.
    /// </summary>
    /// <param name="source">The literal source code contents of the file.</param>
    /// <param name="fileName">The name of the file.</param>
    public void AddSource(string source, string fileName) =>
        AddSource(new(source, fileName));

    /// <summary>
    /// Adds an error.
    /// </summary>
    /// <param name="error">The error to add.</param>
    public void AddError(Error error) =>
        errors.Add(error);

    /// <summary>
    /// Adds an error.
    /// </summary>
    /// <param name="message">The message of the error.</param>
    /// <param name="location">The location of the error.</param>
    public void AddError(string message, Location? location = null) =>
        AddError(new(message, location));
}
