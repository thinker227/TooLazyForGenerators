using Microsoft.CodeAnalysis;

namespace TooLazyForGenerators;

/// <summary>
/// Represents the output of a generator. 
/// </summary>
public interface IGeneratorOutput : IDisposable
{
    /// <summary>
    /// The files the produced by the generator.
    /// </summary>
    IReadOnlyCollection<ProjectSourceFile> Files { get; }
    
    /// <summary>
    /// The errors produced by the generator.
    /// </summary>
    IReadOnlyCollection<Error> Errors { get; }
    
    /// <summary>
    /// The <see cref="Microsoft.CodeAnalysis.Workspace"/> which 
    /// </summary>
    Workspace Workspace { get; }
}

/// <summary>
/// A a source file attached to a project. 
/// </summary>
/// <param name="Project">The project of the file.</param>
/// <param name="File">The source file.</param>
public readonly record struct ProjectSourceFile(
    Project Project,
    SourceFile File);
