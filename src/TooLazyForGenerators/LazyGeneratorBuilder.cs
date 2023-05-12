using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace TooLazyForGenerators;

/// <summary>
/// A builder for a <see cref="LazyGenerator"/>.
/// </summary>
public sealed class LazyGeneratorBuilder
{
    private readonly List<Func<LazyGeneratorBuilder, Task>> asyncCalls;
    private readonly List<FileInfo> projectFiles;
    private readonly List<Type> outputs;
    private ExecutionOptions options;
    
    /// <summary>
    /// The cancellation token for the builder.
    /// </summary>
    public CancellationToken CancellationToken { get; }
    
    /// <summary>
    /// The services for the generator.
    /// </summary>
    public IServiceCollection Services { get; }

    /// <summary>
    /// Initializes a new <see cref="LazyGeneratorBuilder"/> instance.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token for the builder.</param>
    public LazyGeneratorBuilder(CancellationToken cancellationToken = default)
    {
        asyncCalls = new();
        projectFiles = new();
        outputs = new();
        options = new();
        CancellationToken = cancellationToken;
        Services = new ServiceCollection();
    }

    /// <summary>
    /// Creates a new <see cref="LazyGeneratorBuilder"/> using a <see cref="System.Threading.CancellationToken"/>
    /// which is cancelled when the <see cref="Console"/> receives a cancel input.
    /// </summary>
    public static LazyGeneratorBuilder CreateUsingConsoleCancelling()
    {
        CancellationTokenSource cts = new();
        
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };

        return new(cts.Token);
    }

    /// <summary>
    /// Adds a project for the generator to target.
    /// </summary>
    /// <param name="projectFile">The project (<c>.csproj</c>) file of the project.</param>
    public LazyGeneratorBuilder TargetingProject(FileInfo projectFile)
    {
        projectFiles.Add(projectFile);
        return this;
    }
    
    /// <summary>
    /// Adds a project with a specified file path for the generator to target.
    /// </summary>
    /// <param name="projectFilePath">The file path to the project (<c>.csproj</c>) file of the project.</param>
    public LazyGeneratorBuilder TargetingProject(
        string projectFilePath)
    {
        FileInfo file = new(projectFilePath);
        if (!file.Exists)
            throw new InvalidOperationException($"File '{projectFilePath}' does not exist.");

        TargetingProject(file);
        
        return this;
    }
    
    /// <summary>
    /// Adds a project with a specified name for the generator to target
    /// by recursively searching for a solution in any parent directory of the current directory.
    /// </summary>
    /// <param name="projectName">The name of the project to target.</param>
    /// <remarks>
    /// This method is incredibly slow and should not be used if speed is desired,
    /// for instance if running as a build step,
    /// in which case <see cref="TargetingProject(string)"/> with a hardcoded project file path
    /// or project file path supplied through command-line arguments would be more appropriate. 
    /// </remarks>
    public LazyGeneratorBuilder TargetingProjectWithName(
        string projectName)
    {
        asyncCalls.Add(Execute);
        return this;

        async Task Execute(LazyGeneratorBuilder builder)
        {
            DirectoryInfo currentDirectory = new(Directory.GetCurrentDirectory());
            var solutionPath = TryGetSolutionInParentDirectories(currentDirectory);

            if (solutionPath is null) return;

            using var workspace = WorkspaceUtils.CreateWorkspace();
            var solution = await workspace.OpenSolutionAsync(
                solutionFilePath: solutionPath,
                cancellationToken: builder.CancellationToken);

            var project = solution.Projects.FirstOrDefault(p => p.Name == projectName);
            if (project?.FilePath is null) return;

            builder.TargetingProject(project.FilePath);
        }
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
    /// Registers an output for the generator to use.
    /// </summary>
    /// <param name="outputType">The type of the output to register.
    /// The type has to implement <see cref="SourceOutput"/>.</param>
    public LazyGeneratorBuilder WithOutput(Type outputType)
    {
        outputs.Add(outputType);
        return this;
    }
    
    /// <summary>
    /// Registers an <see cref="SourceOutput"/> for the generator to use.
    /// </summary>
    /// <typeparam name="T">The type of the output to register.</typeparam>
    public LazyGeneratorBuilder WithOutput<T>()
        where T : SourceOutput, new() =>
        WithOutput(typeof(T));
    
    /// <summary>
    /// Registers all outputs from a specified assembly or the calling assembly for the generator to use. 
    /// </summary>
    /// <param name="assembly">The assembly to register all outputs from.
    /// If <see langword="null"/> then the calling assembly will be used.</param>
    public LazyGeneratorBuilder WithOutputsFromAssembly(
        Assembly? assembly = null)
    {
        assembly ??= Assembly.GetCallingAssembly();

        var types = assembly.GetTypes()
            .Where(InheritsSourceOutput)
            .ToArray();

        foreach (var type in types)
        {
            WithOutput(type);
        }

        return this;
    }

    private static bool InheritsSourceOutput(Type type)
    {
        var currentType = type;
        
        while (currentType is not null)
        {
            if (currentType == typeof(SourceOutput)) return true;
            currentType = currentType.BaseType;
        }

        return false;
    }

    /// <summary>
    /// Disables concurrent execution of generators.
    /// This can be useful for debugging purposes, but it is recommended to leave this enabled for live generators.
    /// </summary>
    public LazyGeneratorBuilder DisableConcurrentExecution()
    {
        options.RunConcurrently = false;
        return this;
    }

    /// <summary>
    /// Enables generation for generated code,
    /// including both code generated by Roslyn source generators and by generators using this library.
    /// </summary>
    /// <remarks>
    /// Generated code is considered any file with the extension <c>.g.cs</c>,
    /// beginning with a comment <c>// &lt;auto-generated/&gt;</c>,
    /// or fulfilling one of several other cases.
    /// See <see href="https://sourceroslyn.io/#Microsoft.CodeAnalysis/InternalUtilities/GeneratedCodeUtilities.cs,55">
    /// the Roslyn source</see> for the full logic behind determining whether code is generated.
    /// </remarks>
    public LazyGeneratorBuilder EnableExecutionForGeneratedCode()
    {
        options.RunForGeneratedCode = true;
        return this;
    }

    /// <summary>
    /// Builds the generator.
    /// </summary>
    public async Task<LazyGenerator> Build()
    {
        foreach (var call in asyncCalls)
        {
            await call(this);
        }
        
        return new(
            projectFiles,
            outputs,
            CancellationToken,
            Services.BuildServiceProvider(),
            options);
    }
}
