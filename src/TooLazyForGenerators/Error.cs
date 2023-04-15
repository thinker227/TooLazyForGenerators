using Microsoft.CodeAnalysis;

namespace TooLazyForGenerators;

/// <summary>
/// An error reported by a source output.
/// </summary>
/// <param name="Message">The message of the error.</param>
/// <param name="Location">The location of the error.</param>
public record struct Error(
    string Message,
    Location? Location);
