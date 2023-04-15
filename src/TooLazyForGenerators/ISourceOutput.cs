using Microsoft.CodeAnalysis;

namespace TooLazyForGenerators;

/// <summary>
/// Represents a source output,
/// a type which acts as a transformer between a <see cref="Project"/> and a source output.  
/// </summary>
public interface ISourceOutput
{
    /// <summary>
    /// Gets a source from a <see cref="Project"/>.
    /// </summary>
    /// <param name="ctx">A context object which provides additional information
    /// as well as methods to register output.</param>
    Task GetSource(ISourceOutputContext ctx);
}
