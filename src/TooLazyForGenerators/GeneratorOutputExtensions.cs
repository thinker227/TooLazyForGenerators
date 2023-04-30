using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

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
    /// <param name="getFolders">A function to get the folders for a file.
    /// If not specified then all files will be placed into a
    /// folder called .generated in the root of the project.</param>
    public static int WriteAndReturn(
        this GeneratorOutput output,
        Func<SourceFile, IEnumerable<string>>? getFolders = null)
    {
        output.Write(getFolders);
        return output.Return();
    }

    /// <summary>
    /// Writes the output of a <see cref="GeneratorOutput"/> to disk.
    /// </summary>
    /// <param name="output">The generator output.</param>
    /// <param name="getFolders">A function to get the folders for a file.
    /// If not specified then all files will be placed into a
    /// folder called .generated in the root of the project.</param>
    public static void Write(
        this GeneratorOutput output,
        Func<SourceFile, IEnumerable<string>>? getFolders = null)
    {
        getFolders ??= GetDefaultFolders;
        
        var solutions = GetSolutionsDictionary(output.Files);

        foreach (var (solution, projects) in solutions)
        {
            WriteToSolution(solution, projects, getFolders);
        }
    }

    private static readonly string[] defaultFolders = { ".generated" };

    private static IEnumerable<string> GetDefaultFolders(SourceFile _) =>
        defaultFolders;

    private static Dictionary<Solution, Dictionary<Project, List<SourceFile>>> GetSolutionsDictionary(
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

    private static void WriteToSolution(
        Solution solution,
        Dictionary<Project, List<SourceFile>> projects,
        Func<SourceFile, IEnumerable<string>> getFolders)
    {
        var currentSolution = solution;
        
        foreach (var (project, files) in projects)
        {
            var projectInCurrentSolution = currentSolution.GetProject(project.Id);
            if (projectInCurrentSolution is null) throw new InvalidOperationException(
                $"Project {project.Name} does not exist in the current solution.");
            
            var newProject = AddFilesToProject(projectInCurrentSolution, files, getFolders);
            currentSolution = newProject.Solution;
        }

        var success = currentSolution.Workspace.TryApplyChanges(currentSolution);
        if (!success) throw new InvalidOperationException(
            "Failed to apply changes to solution.");
    }

    private static Project AddFilesToProject(
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
    /// Returns a status code depending on whether the output was successful or not.
    /// </summary>
    /// <param name="output">The generator output.</param>
    public static int Return(this GeneratorOutput output) =>
        output.Errors.Count == 0
            ? 0
            : 1;
}
