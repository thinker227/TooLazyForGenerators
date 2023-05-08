using Microsoft.CodeAnalysis;

namespace TooLazyForGenerators;

public interface ISourceAndErrors
{
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

public static class SourceAndErrorsExtensions
{
    /// <summary>
    /// Adds a source file.
    /// </summary>
    /// <param name="source">The literal source code contents of the file.</param>
    /// <param name="fileName">The name of the file.</param>
    public static void AddSource(
        this ISourceAndErrors sourceAndErrors,
        string source,
        string fileName) =>
        sourceAndErrors.AddSource(new(source, fileName));

    /// <summary>
    /// Adds an error.
    /// </summary>
    /// <param name="message">The message of the error.</param>
    /// <param name="location">The location of the error.</param>
    public static void AddError(
        this ISourceAndErrors sourceAndErrors,
        string message,
        Location? location = null) =>
        sourceAndErrors.AddError(new(message, location));
}
