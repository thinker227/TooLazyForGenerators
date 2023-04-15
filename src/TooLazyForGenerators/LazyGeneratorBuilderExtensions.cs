using System.Reflection;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

namespace TooLazyForGenerators;

/// <summary>
/// Extensions for <see cref="LazyGeneratorBuilder"/>.
/// </summary>
public static class LazyGeneratorBuilderExtensions
{
    /// <summary>
    /// Adds a project with a specified file path for the generator to target,
    /// </summary>
    /// <param name="builder">The source builder-</param>
    /// <param name="projectFilePath">The file path to the project (<c>.csproj</c>) file of the project.</param>
    public static LazyGeneratorBuilder TargetingProject(
        this LazyGeneratorBuilder builder,
        string projectFilePath)
    {
        FileInfo file = new(projectFilePath);
        if (!file.Exists)
            throw new InvalidOperationException($"File '{projectFilePath}' does not exist.");

        builder.TargetingProject(file);
        
        return builder;
    }

    /// <summary>
    /// Adds a project with a specified name for the generator to target
    /// by recursively searching for a solution in any parent directory of the current directory.
    /// </summary>
    /// <param name="builder">The source builder.</param>
    /// <param name="projectName">The name of the project to target.</param>
    public static async Task<LazyGeneratorBuilder> TargetingProjectWithName(
        this LazyGeneratorBuilder builder,
        string projectName)
    {
        DirectoryInfo currentDirectory = new(Directory.GetCurrentDirectory());
        var solutionPath = TryGetSolutionInParentDirectories(currentDirectory);

        if (solutionPath is null) return builder;

        using var workspace = WorkspaceUtils.CreateWorkspace();
        var solution = await workspace.OpenSolutionAsync(
            solutionFilePath: solutionPath,
            cancellationToken: builder.CancellationToken);

        var project = solution.Projects.FirstOrDefault(p => p.Name == projectName);
        if (project?.FilePath is null) return builder;

        builder.TargetingProject(new(project.FilePath));
        
        return builder;
    }

    private static string? TryGetSolutionInParentDirectories(DirectoryInfo directory)
    {
        while (true)
        {
            var solutionFiles = directory.GetFiles("*.sln", SearchOption.TopDirectoryOnly);
            
            if (solutionFiles.Length == 1) return solutionFiles[0].FullName;
            
            if (directory.Parent is null) return null;
            directory = directory.Parent;
        }
    }

    /// <summary>
    /// Registers an <see cref="ISourceOutput"/> for the generator to use.
    /// </summary>
    /// <typeparam name="T">The type of the output to register.</typeparam>
    /// <param name="builder">The source builder.</param>
    public static LazyGeneratorBuilder WithOutput<T>(this LazyGeneratorBuilder builder)
        where T : ISourceOutput =>
        builder.WithOutput(typeof(T));

    /// <summary>
    /// Registers all outputs from a specified assembly or the calling assembly for the generator to use. 
    /// </summary>
    /// <param name="builder">The source builder.</param>
    /// <param name="assembly">The assembly to register all outputs from.
    /// If <see langword="null"/> then the calling assembly will be used.</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static LazyGeneratorBuilder WithOutputsFromAssembly(
        this LazyGeneratorBuilder builder,
        Assembly? assembly = null)
    {
        assembly ??= Assembly.GetCallingAssembly();

        var types = assembly.GetTypes()
            .Where(ImplementsISourceOutputT)
            .ToArray();

        foreach (var type in types)
        {
            builder.WithOutput(type);
        }

        return builder;
    }

    private static bool ImplementsISourceOutputT(Type type) => type.GetInterfaces()
        .Any(i => i == typeof(ISourceOutput));
}
