using Microsoft.CodeAnalysis;

namespace TooLazyForGenerators;

/// <summary>
/// Extensions for <see cref="GeneratorOutput"/>.
/// </summary>
public static class GeneratorOutputExtensions
{
    /// <summary>
    /// Returns the files of a <see cref="GeneratorOutput"/> as a dictionary of projects and files.
    /// </summary>
    /// <param name="output">The source output.</param>
    public static IReadOnlyDictionary<Project, IReadOnlyCollection<SourceFile>> FilesAsDictionary(
        this GeneratorOutput output)
    {
        var result = new Dictionary<Project, IReadOnlyCollection<SourceFile>>();

        foreach (var file in output.Files)
        {
            if (!result.TryGetValue(file.Project, out var files))
            {
                files = new List<SourceFile>();
                result.Add(file.Project, files);
            }

            var list = (List<SourceFile>)files;
            list.Add(file.File);
        }

        return result;
    }
    
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
