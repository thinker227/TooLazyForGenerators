namespace TooLazyForGenerators;

/// <summary>
/// A generator which takes one or multiple projects and adds source code to them.
/// </summary>
public sealed class LazyGenerator
{
    /// <summary>
    /// Creates a builder for a <see cref="LazyGenerator"/>.
    /// </summary>
    public static LazyGeneratorBuilder CreateBuilder() => new();

    /// <summary>
    /// Runs the generator.
    /// </summary>
    /// <returns>The output from the generator.</returns>
    public Task<GeneratorOutput> Run() => throw new NotImplementedException();
}
