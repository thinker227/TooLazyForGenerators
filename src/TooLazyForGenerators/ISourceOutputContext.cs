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
}
