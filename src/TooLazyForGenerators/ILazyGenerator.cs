namespace TooLazyForGenerators;

/// <summary>
/// A generator which takes one or multiple projects and adds source code to them.
/// </summary>
public interface ILazyGenerator
{
    /// <summary>
    /// Runs the generator.
    /// </summary>
    /// <returns>The output from the generator.</returns>
    Task<GeneratorOutput> Run(CancellationToken cancellationToken = default);
}
