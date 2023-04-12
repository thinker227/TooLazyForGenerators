namespace TooLazyForGenerators;

/// <summary>
/// A source output.
/// </summary>
public abstract record Output
{
    public sealed record Source : Output;

    public sealed record Error : Output;
}
