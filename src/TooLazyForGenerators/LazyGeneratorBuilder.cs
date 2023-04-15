namespace TooLazyForGenerators;

/// <summary>
/// A builder for an <see cref="ILazyGenerator"/>.
/// </summary>
public sealed class LazyGeneratorBuilder
{
    private readonly List<FileInfo> projectFiles = new();
    private readonly List<Type> outputs = new();
    
    public CancellationToken CancellationToken { get; }

    public LazyGeneratorBuilder(CancellationToken cancellationToken = default) =>
        CancellationToken = cancellationToken;

    /// <summary>
    /// Adds a project for the generator to target.
    /// </summary>
    /// <param name="projectFile">The project (<c>.csproj</c>) file of the project.</param>
    public LazyGeneratorBuilder TargetingProject(FileInfo projectFile)
    {
        projectFiles.Add(projectFile);
        return this;
    }

    /// <summary>
    /// Registers an output for the generator to use.
    /// </summary>
    /// <param name="outputType">The type of the output to register.
    /// The type has to implement <see cref="ISourceOutput{TSelf}"/>.</param>
    public LazyGeneratorBuilder WithOutput(Type outputType)
    {
        outputs.Add(outputType);
        return this;
    }

    /// <summary>
    /// Builds the generator.
    /// </summary>
    public ILazyGenerator Build() => new LazyGenerator()
    {
        ProjectFiles = projectFiles,
        Outputs = outputs
    };
}
