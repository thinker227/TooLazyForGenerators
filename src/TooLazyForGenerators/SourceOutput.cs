using System.Collections.Concurrent;
using Microsoft.CodeAnalysis;

namespace TooLazyForGenerators;

/// <summary>
/// The base class for source outputs generating source files and errors.
/// </summary>
/// <remarks>
/// Use the <see cref="AddSource(SourceFile)"/> and <see cref="AddError(Error)"/> methods
/// and their corresponding overloads to add source files and errors.
/// <br/>
/// <br/>
/// Constructor dependency injection is supported on types inheriting from <see cref="SourceOutput"/>.
/// If a constructor with one or more parameters is present, the services registered using
/// <see cref="LazyGeneratorBuilder.Services"/> will be used to call the constructor.
/// </remarks>
public abstract class SourceOutput
{
    internal ConcurrentBag<SourceFile>? Files { get; set; }
    
    internal ConcurrentBag<Error>? Errors { get; set; }

    /// <summary>
    /// Adds a source file to the generator output.
    /// </summary>
    /// <param name="file">The source file to add.</param>
    protected void AddSource(SourceFile file) =>
        Files?.Add(file);

    /// <summary>
    /// Adds a source file to the generator output.
    /// </summary>
    /// <param name="source">The literal source code contents of the file.</param>
    /// <param name="fileName">The name of the file.</param>
    protected void AddSource(string source, string fileName) =>
        AddSource(new(source, fileName));

    /// <summary>
    /// Adds an error to the generator output.
    /// </summary>
    /// <param name="error">The error to add.</param>
    protected void AddError(Error error) =>
        Errors?.Add(error);

    /// <summary>
    /// Adds an error to the generator output.
    /// </summary>
    /// <param name="message">The message of the error.</param>
    /// <param name="location">The location of the error.</param>
    protected void AddError(string message, Location? location = null) =>
        AddError(new(message, location));

    /// <summary>
    /// Adds an error to the generator output.
    /// </summary>
    /// <param name="message">The message of the error.</param>
    /// <param name="syntaxNode">The syntax node which location should be used as the location of the error.</param>
    protected void AddError(string message, SyntaxNode syntaxNode) =>
        AddError(message, syntaxNode.GetLocation());

    /// <summary>
    /// Adds an error to the generator output.
    /// </summary>
    /// <param name="message">The message of the error.</param>
    /// <param name="symbol">The symbol which location should be used as the location of the error.
    /// If the symbol has multiple locations, an error will be added for each location.</param>
    protected void AddError(string message, ISymbol symbol)
    {
        foreach (var location in symbol.Locations)
        {
            AddError(message, location);
        }
    }

    /// <summary>
    /// Called to initialize the generator.
    /// </summary>
    /// <param name="ctx">A context object which allows the generator to register actions
    /// for different kinds of compilation entities.</param>
    public abstract void Initialize(SourceOutputInitalizationContext ctx);
}
