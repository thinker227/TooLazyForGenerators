namespace TooLazyForGenerators;

/// <summary>
/// A builder for an <see cref="ILazyGenerator"/>.
/// </summary>
public interface ILazyGeneratorBuilder
{
    /// <summary>
    /// The cancellation token for the builder.
    /// </summary>
    CancellationToken CancellationToken { get; }
    
    /// <summary>
    /// Adds a project for the generator to target.
    /// </summary>
    /// <param name="projectFile">The project (<c>.csproj</c>) file of the project.</param>
    ILazyGeneratorBuilder TargetingProject(FileInfo projectFile);

    /// <summary>
    /// Registers an output for the generator to use.
    /// </summary>
    /// <param name="outputType">The type of the output to register.
    /// The type has to implement <see cref="ISourceOutput"/>.</param>
    ILazyGeneratorBuilder WithOutput(Type outputType);

    /// <summary>
    /// Builds the generator.
    /// </summary>
    ILazyGenerator Build();
}
