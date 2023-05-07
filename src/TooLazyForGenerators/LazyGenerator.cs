using System.Collections.Concurrent;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.Extensions.DependencyInjection;
using TooLazyForGenerators.Pipelines;

namespace TooLazyForGenerators;

/// <summary>
/// A generator which takes one or multiple projects and adds source code to them.
/// </summary>
public sealed class LazyGenerator
{
    private readonly IReadOnlyCollection<FileInfo> projectFiles;
    private readonly IReadOnlyCollection<Type> outputs;
    private readonly IReadOnlyList<PipelineStep> pipelineSteps;
    private readonly CancellationToken cancellationToken;
    private readonly IServiceProvider services;
    private readonly ExecutionOptions options;

    /// <summary>
    /// Initializes a new <see cref="LazyGenerator"/> instance.
    /// </summary>
    /// <param name="projectFiles">The project files the generator targets.</param>
    /// <param name="outputs">The output types implementing <see cref="ISourceOutput"/> the generator will call.</param>
    /// <param name="pipelineSteps">The steps of the generator pipeline.</param>
    /// <param name="cancellationToken">The cancellation token for the generator.</param>
    /// <param name="services">The services for the generator.</param>
    /// <param name="options">The execution options for the generator.</param>
    public LazyGenerator(
        IReadOnlyCollection<FileInfo> projectFiles,
        IReadOnlyCollection<Type> outputs,
        IReadOnlyList<PipelineStep> pipelineSteps,
        CancellationToken cancellationToken,
        IServiceProvider services,
        ExecutionOptions options)
    {
        this.projectFiles = projectFiles;
        this.outputs = outputs;
        this.pipelineSteps = pipelineSteps;
        this.cancellationToken = cancellationToken;
        this.services = services;
        this.options = options;
    }

    /// <summary>
    /// Runs the generator.
    /// </summary>
    /// <returns>The output from the generator.</returns>
    public async Task<GeneratorOutput> Run()
    {
        var workspace = WorkspaceUtils.CreateWorkspace();

        var results = await Task.WhenAll(projectFiles
            .Select(file => HandleProject(workspace, file)));

        return new(
            results.SelectMany(r => r.Files).ToArray(),
            results.SelectMany(r => r.Errors).ToArray(),
            workspace);
    }

    private async Task<ProjectResult> HandleProject(MSBuildWorkspace workspace, FileInfo projectFile)
    {
        var project = await GetProject(workspace, projectFile);
        if (!project.SupportsCompilation) throw new InvalidOperationException(
            $"Project {project.Name} does not support compilation.");
        
        var compilation = (await project.GetCompilationAsync(cancellationToken))!;

        using var serviceScope = services.CreateScope();

        var files = new ConcurrentBag<SourceFile>();
        var errors = new ConcurrentBag<Error>();

        // ISourceOutput instances don't have lifetimes longer than this method.
        // ReSharper disable once AccessToDisposedClosure
        ISourceOutput CreateSourceOutput(Type type) =>
            (ISourceOutput)ActivatorUtilities.CreateInstance(serviceScope.ServiceProvider, type);

        // The analyzer lifetime is no longer than the execution of this method.
        // ReSharper disable once AccessToDisposedClosure
        var analyzers = outputs
            .Select(type => (DiagnosticAnalyzer)new GeneratorAnalyzerWrapper(
                CreateSourceOutput(type),
                options,
                files,
                errors))
            .ToImmutableArray();

        // TODO: Supply options to WithAnalyzers.
        var compilationWithAnalyzers = compilation.WithAnalyzers(
            analyzers,
            cancellationToken: cancellationToken);
        
        // We don't actually care about the result here,
        // because the analyzers aren't supposed to produce any diagnostics.
        await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync(cancellationToken);
        
        return new(
            files
                .Select(file => new ProjectSourceFile(project, file))
                .ToArray(),
            errors);
    }
    
    private Task<Project> GetProject(MSBuildWorkspace workspace, FileInfo projectFile) =>
        workspace.OpenProjectAsync(
            projectFilePath: projectFile.FullName,
            cancellationToken: cancellationToken);

    private readonly record struct ProjectResult(
        IReadOnlyCollection<ProjectSourceFile> Files,
        IReadOnlyCollection<Error> Errors);
}
