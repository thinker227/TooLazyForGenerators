using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

namespace TooLazyForGenerators;

internal sealed class LazyGenerator : ILazyGenerator
{
    public required IReadOnlyCollection<FileInfo> ProjectFiles { get; init; }
    
    public required IReadOnlyCollection<Type> Outputs { get; init; }
    
    public required CancellationToken CancellationToken { get; init; }

    public async Task<GeneratorOutput> Run(CancellationToken cancellationToken = default)
    {
        using var workspace = WorkspaceUtils.CreateWorkspace();

        foreach (var projectFile in ProjectFiles)
        {
            await HandleProject(projectFile, workspace);
        }

        throw new NotImplementedException();
    }

    private async Task HandleProject(FileInfo projectFile, MSBuildWorkspace workspace)
    {
        var project = await GetProject(workspace, projectFile);
        var files = new List<SourceFile>();
        
        SourceOutputContext ctx = new()
        {
            Project = project,
            CancellationToken = CancellationToken,
            Files = files
        };
        
        foreach (var output in GetOutputInstances())
        {
            await output.GetSource(ctx);
        }
    }
    
    private Task<Project> GetProject(MSBuildWorkspace workspace, FileInfo projectFile) =>
        workspace.OpenProjectAsync(
            projectFilePath: projectFile.FullName,
            cancellationToken: CancellationToken);

    private IEnumerable<ISourceOutput> GetOutputInstances() => Outputs.Select(type =>
    {
        var ctor = type.GetConstructor(
            BindingFlags.Instance | BindingFlags.Public,
            Array.Empty<Type>());

        if (ctor is null)
            throw new InvalidOperationException($"{type.FullName} has no public parameterless constructor.");

        var instance = ctor.Invoke(null);
        return (ISourceOutput)instance;
    });
    
    

    private readonly struct SourceOutputContext : ISourceOutputContext
    {
        public required Project Project { get; init; }
    
        public required CancellationToken CancellationToken { get; init; }
        
        public required ICollection<SourceFile> Files { get; init; }

        public void AddSource(SourceFile file) => Files.Add(file);
    }
}
