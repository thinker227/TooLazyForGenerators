namespace TooLazyForGenerators;

/// <summary>
/// A builder for an <see cref="ILazyGenerator"/>.
/// </summary>
public sealed class LazyGeneratorBuilder
{
    /// <summary>
    /// Adds a project for the generator to target.
    /// </summary>
    /// <param name="projectFile">The project (<c>.csproj</c>) file of the project.</param>
    public LazyGeneratorBuilder TargetingProject(FileInfo projectFile) => throw new NotImplementedException();

    /// <summary>
    /// Registers an output for the generator to use.
    /// </summary>
    /// <param name="outputType">The type of the output to register.
    /// The type has to implement <see cref="ISourceOutput{TSelf}"/>.</param>
    public LazyGeneratorBuilder WithOutput(Type outputType) => throw new NotImplementedException();

    /// <summary>
    /// Builds the generator.
    /// </summary>
    public ILazyGenerator Build() => throw new NotImplementedException();
}
