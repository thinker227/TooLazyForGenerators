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
        MSBuildLocator.RegisterDefaults();
        
        DirectoryInfo currentDirectory = new(Directory.GetCurrentDirectory());
        var result = await TryGetSolutionInParentDirectories(currentDirectory);

        if (result is not (Workspace workspace, Solution solution)) return builder;

        using (workspace)
        {
            var project = solution.Projects.FirstOrDefault(p => p.Name == projectName);
            if (project?.FilePath is null) return builder;

            builder.TargetingProject(new(project.FilePath));
        }
        
        return builder;
    }

    private static async Task<(Workspace, Solution)?> TryGetSolutionInParentDirectories(DirectoryInfo directory)
    {
        var solutionFiles = directory.GetFiles("*.sln", SearchOption.TopDirectoryOnly);
        if (solutionFiles.Length != 1)
        {
            if (directory.Parent is null) return null;
            return await TryGetSolutionInParentDirectories(directory.Parent);
        }
        
        var workspace = MSBuildWorkspace.Create();
        
        var solution = await workspace.OpenSolutionAsync(solutionFiles[0].FullName);
        return (workspace, solution);
    }

    /// <summary>
    /// Registers an <see cref="ISourceOutput{TSelf}"/> for the generator to use.
    /// </summary>
    /// <typeparam name="T">The type of the output to register.</typeparam>
    /// <param name="builder">The source builder.</param>
    public static LazyGeneratorBuilder WithOutput<T>(this LazyGeneratorBuilder builder)
        where T : ISourceOutput<T> =>
        builder.WithOutput(typeof(T));
        

    private static MethodInfo? withOutputMethod;
    
    private const string WithOutputMethodName = nameof(LazyGeneratorBuilder.WithOutput);

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
        withOutputMethod ??= typeof(LazyGeneratorBuilder).GetMethod(WithOutputMethodName)
            ?? throw new InvalidOperationException($"Method {WithOutputMethodName} could not be found.");
        
        assembly ??= Assembly.GetCallingAssembly();

        var types = assembly.GetTypes()
            .Where(ImplementsISourceOutputT)
            .ToArray();

        foreach (var type in types)
        {
            var method = withOutputMethod.MakeGenericMethod(type);
            method.Invoke(builder, null);
        }

        return builder;
    }

    private static bool ImplementsISourceOutputT(Type type) => type.GetInterfaces()
        .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ISourceOutput<>));
}
