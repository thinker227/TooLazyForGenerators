using Microsoft.CodeAnalysis;

namespace TooLazyForGenerators;

/// <summary>
/// Represents a source output,
/// a type which acts as a transformer between a <see cref="Project"/> and a source output.  
/// </summary>
public interface ISourceOutput<out TSelf> where TSelf : ISourceOutput<TSelf>
{
    static abstract TSelf Create();
    
    /// <summary>
    /// Gets a source from a <see cref="Project"/>.
    /// </summary>
    /// <param name="project">The project to create the source from.</param>
    Task<Output> GetSource(Project project);
}
