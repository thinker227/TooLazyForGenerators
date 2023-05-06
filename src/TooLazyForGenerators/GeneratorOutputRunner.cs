using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using TooLazyForGenerators.Pipelines;

namespace TooLazyForGenerators;

internal sealed class GeneratorOutputRunner
{
    private readonly IReadOnlyList<PipelineStep> pipelineSteps;
    private readonly ConcurrentBag<SourceFile> files;
    private readonly ConcurrentBag<Error> errors;
    private readonly Project project;
    private readonly CancellationToken cancellationToken;
    private readonly IServiceScope serviceScope;

    public GeneratorOutputRunner(
        IReadOnlyList<PipelineStep> pipelineSteps,
        ConcurrentBag<SourceFile> files,
        ConcurrentBag<Error> errors,
        Project project,
        CancellationToken cancellationToken,
        IServiceScope serviceScope)
    {
        this.pipelineSteps = pipelineSteps;
        this.files = files;
        this.errors = errors;
        this.project = project;
        this.cancellationToken = cancellationToken;
        this.serviceScope = serviceScope;
    }

    public Task Run(Type outputType)
    {
        var ctx = new PipelineContext()
        {
            TargetType = outputType,
            CreateTarget = CreateSourceOutput,
            Project = project,
            CancellationToken = cancellationToken,
            Services = serviceScope.ServiceProvider
        };

        return CallPipelineStep(ctx, 0);
    }

    private Task CallPipelineStep(PipelineContext pipelineContext, int stepIndex)
    {
        if (stepIndex < pipelineSteps.Count)
            return pipelineSteps[stepIndex](pipelineContext, newCtx =>
                CallPipelineStep(newCtx, stepIndex + 1));

        var instance = pipelineContext.CreateTarget(pipelineContext.TargetType, pipelineContext.Services);
        var outputContext = new SourceOutputContext(pipelineContext.Project, cancellationToken, files, errors);
        return instance.GetSource(outputContext);
    }
    
    private static ISourceOutput CreateSourceOutput(Type type, IServiceProvider services)
    {
        var ctor = type.GetConstructor(
            BindingFlags.Instance | BindingFlags.Public,
            null,
            Array.Empty<Type>(),
            null);
        
        if (ctor is null)
            throw new InvalidOperationException($"{type.FullName} has no public parameterless constructor.");

        var instance = ctor.Invoke(null);
        return (ISourceOutput)instance;
    }
}
