using Microsoft.CodeAnalysis;

namespace TooLazyForGenerators;

/// <summary>
/// Represents a source output,
/// a type which acts as a transformer between a <see cref="Project"/> and a source output.  
/// </summary>
public interface ISourceOutput
{
    /// <summary>
    /// Called when initializing the source output.
    /// Use the provided <paramref name="ctx"/> to register actions which will be called
    /// on specific kinds of compilation entities, then use  
    /// </summary>
    /// <param name="ctx"></param>
    void Initialize(NewSourceOutputContext ctx);
}
