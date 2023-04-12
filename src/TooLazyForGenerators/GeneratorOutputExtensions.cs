namespace TooLazyForGenerators;

/// <summary>
/// Extensions for <see cref="GeneratorOutput"/>.
/// </summary>
public static class GeneratorOutputExtensions
{
    /// <summary>
    /// Writes the output of a <see cref="GeneratorOutput"/> to disk and returns
    /// a status code depending on whether the output was successful or not.
    /// </summary>
    /// <param name="output">The generator output.</param>
    public static int WriteAndReturn(this GeneratorOutput output)
    {
        output.Write();
        return output.Return();
    }

    /// <summary>
    /// Writes the output of a <see cref="GeneratorOutput"/> to disk.
    /// </summary>
    /// <param name="output">The generator output.</param>
    public static void Write(this GeneratorOutput output) => throw new NotImplementedException();

    /// <summary>
    /// Returns a status code depending on whether the output was successful or not.
    /// </summary>
    /// <param name="output">The generator output.</param>
    public static int Return(this GeneratorOutput output) => throw new NotImplementedException();
}
