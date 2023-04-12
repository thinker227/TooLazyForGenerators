namespace TooLazyForGenerators;

/// <summary>
/// The output of a <see cref="LazyGenerator"/>.
/// </summary>
public abstract record GeneratorOutput
{
    /// <summary>
    /// An output which indicates a success.
    /// </summary>
    public sealed record Success : GeneratorOutput;

    /// <summary>
    /// An output which indicates some error.
    /// </summary>
    public sealed record Failure : GeneratorOutput;
}
