using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace TooLazyForGenerators;

/// <summary>
/// The output of a generator
/// </summary>
/// <param name="Files">The files the produced by the generator.</param>
/// <param name="Errors">The errors produced by the generator.</param>
/// <param name="Workspace">The <see cref="Microsoft.CodeAnalysis.Workspace"/>
/// by which projects handled by the generator are managed.</param>
public readonly record struct GeneratorOutput(
    IReadOnlyCollection<ProjectSourceFile> Files,
    IReadOnlyCollection<Error> Errors,
    Workspace Workspace) : IDisposable
{
    public void Dispose() => Workspace.Dispose();
    
    /// <summary>
    /// Returns the files of the output as a dictionary of projects and files.
    /// </summary>
    public IReadOnlyDictionary<Project, IReadOnlyCollection<SourceFile>> FilesAsDictionary()
    {
        var result = new Dictionary<Project, IReadOnlyCollection<SourceFile>>();

        foreach (var file in Files)
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
    /// Writes the output to disk and returns a status code
    /// depending on whether the output was successful or not.
    /// </summary>
    /// <param name="getFolders">A function to get the folders for a file.
    /// If not specified then all files will be placed into a
    /// folder called .generated in the root of the project.</param>
    public int WriteAndReturn(
        Func<SourceFile, IEnumerable<string>>? getFolders = null)
    {
        Write(getFolders);
        return Return();
    }

    /// <summary>
    /// Writes the output to disk.
    /// </summary>
    /// <param name="getFolders">A function to get the folders for a file.
    /// If not specified then all files will be placed into a
    /// folder called .generated in the root of the project.</param>
    public void Write(
        Func<SourceFile, IEnumerable<string>>? getFolders = null)
    {
        getFolders ??= GetDefaultFolders;
        
        var solutions = GetSolutionsDictionary(Files);

        foreach (var (solution, projects) in solutions)
        {
            WriteToSolution(solution, projects, getFolders);
        }
    }

    private static readonly string[] defaultFolders = { ".generated" };

    private IEnumerable<string> GetDefaultFolders(SourceFile _) =>
        defaultFolders;

    private Dictionary<Solution, Dictionary<Project, List<SourceFile>>> GetSolutionsDictionary(
        IEnumerable<ProjectSourceFile> outputFiles)
    {
        var solutions = new Dictionary<Solution, Dictionary<Project, List<SourceFile>>>();
        
        foreach (var (project, file) in outputFiles)
        {
            var solution = project.Solution;
            
            if (!solutions.TryGetValue(solution, out var projects))
            {
                projects = new();
                solutions.Add(solution, projects);
            }

            if (!projects.TryGetValue(project, out var files))
            {
                files = new();
                projects.Add(project, files);
            }

            files.Add(file);
        }

        return solutions;
    }

    private void WriteToSolution(
        Solution solution,
        Dictionary<Project, List<SourceFile>> projects,
        Func<SourceFile, IEnumerable<string>> getFolders)
    {
        // Just hope and pray that the workspace is not some weird eldritch nonsense from nowhere
        // and that using it like this will actually work.
        var workspace = solution.Workspace;
        
        foreach (var (project, files) in projects)
        {
            var projectInCurrentSolution = workspace.CurrentSolution.GetProject(project.Id);
            if (projectInCurrentSolution is null) throw new InvalidOperationException(
                $"Project {project.Name} does not exist in the current solution.");
            
            var newProject = AddFilesToProject(projectInCurrentSolution, files, getFolders);

            var success = workspace.TryApplyChanges(newProject.Solution);
            // TODO: There is probably a better way to reporting a failure here than to throw an exception.
            if (!success) throw new InvalidOperationException(
                $"Failed to apply changes to project {project.Name}.");
        }
    }

    private Project AddFilesToProject(
        Project project,
        IEnumerable<SourceFile> files,
        Func<SourceFile, IEnumerable<string>> getFolders)
    {
        var currentProject = project;
        
        foreach (var file in files)
        {
            var sourceText = SourceText.From(file.Source);
            var folders = getFolders(file);
            var document = currentProject.AddDocument(file.FileName, sourceText, folders);
            currentProject = document.Project;
        }

        return currentProject;
    }

    /// <summary>
    /// Returns a status code depending on whether the output is successful or not.
    /// </summary>
    public int Return() =>
        Errors.Count == 0
            ? 0
            : 1;
}

/// <summary>
/// A a source file attached to a project. 
/// </summary>
/// <param name="Project">The project of the file.</param>
/// <param name="File">The source file.</param>
public readonly record struct ProjectSourceFile(
    Project Project,
    SourceFile File);
