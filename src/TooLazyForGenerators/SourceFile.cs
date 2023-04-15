namespace TooLazyForGenerators;

/// <summary>
/// An output file containing source code.
/// </summary>
/// <param name="Source">The literal source code contents of the file.</param>
/// <param name="FileName">The name of the file.</param>
public readonly record struct SourceFile(
    string Source,
    string FileName);
