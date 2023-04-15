using Microsoft.CodeAnalysis;

namespace TooLazyForGenerators;

/// <summary>
/// The output of an <see cref="ILazyGenerator"/>.
/// </summary>
/// <param name="Files">The source files produced by the generator.</param>
/// <param name="Errors">The error produced by the generator.</param>
public readonly record struct GeneratorOutput(
    IReadOnlyCollection<ProjectSourceFile> Files,
    IReadOnlyCollection<Error> Errors);

/// <summary>
/// A a source file attached to a project. 
/// </summary>
/// <param name="Project">The project of the file.</param>
/// <param name="File">The source file.</param>
public readonly record struct ProjectSourceFile(
    Project Project,
    SourceFile File);
