using Microsoft.CodeAnalysis;

namespace TooLazyForGenerators;

/// <summary>
/// A context for an <see cref="ISourceOutput"/>.
/// </summary>
public interface ISourceOutputContext
{
    /// <summary>
    /// The project which is being targeted.
    /// </summary>
    Project Project { get; }
    
    /// <summary>
    /// The cancellation token for the operation.
    /// </summary>
    CancellationToken CancellationToken { get; }

    /// <summary>
    /// Adds a source file.
    /// </summary>
    /// <param name="file">The file to add.</param>
    void AddSource(SourceFile file);

    /// <summary>
    /// Adds an error.
    /// </summary>
    /// <param name="error">The error to add.</param>
    void AddError(Error error);
}

public static class SourceOutputContextExtensions
{
    /// <summary>
    /// Adds a source file.
    /// </summary>
    /// <param name="ctx">The source context.</param>
    /// <param name="source">The literal source code contents of the file.</param>
    /// <param name="fileName">The name of the file.</param>
    public static void AddSource(this ISourceOutputContext ctx, string source, string fileName) =>
        ctx.AddSource(new(source, fileName));

    /// <summary>
    /// Adds an error.
    /// </summary>
    /// <param name="ctx">The source context.</param>
    /// <param name="message">The message of the error.</param>
    /// <param name="location">The location of the error.</param>
    public static void AddError(this ISourceOutputContext ctx, string message, Location? location = null) =>
        ctx.AddError(new(message, location));
}
