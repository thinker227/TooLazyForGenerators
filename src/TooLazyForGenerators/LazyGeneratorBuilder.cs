namespace TooLazyForGenerators;

/// <summary>
/// A builder for a <see cref="LazyGenerator"/>.
/// </summary>
public sealed class LazyGeneratorBuilder
{
    /// <summary>
    /// Adds a project for the generator to target.
    /// </summary>
    /// <param name="projectFile">The project (<c>.csproj</c>) file of the project.</param>
    public LazyGeneratorBuilder TargetingProject(FileInfo projectFile) => throw new NotImplementedException();

    /// <summary>
    /// Registers an <see cref="ISourceOutput{T}"/> for the generator to use.
    /// </summary>
    /// <typeparam name="T">The type of the output to register.</typeparam>
    public LazyGeneratorBuilder WithOutput<T>()
        where T : ISourceOutput<T> => throw new NotImplementedException();

    /// <summary>
    /// Builds the generator.
    /// </summary>
    public LazyGenerator Build() => throw new NotImplementedException();
}
