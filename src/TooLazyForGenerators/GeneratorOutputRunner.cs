using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using TooLazyForGenerators.Pipelines;

namespace TooLazyForGenerators;

internal sealed class GeneratorOutputRunner
{
    public required IReadOnlyList<PipelineStep> PipelineSteps { get; init; }
    
    public required ConcurrentBag<SourceFile> Files { get; init; }
    
    public required ConcurrentBag<Error> Errors { get; init; }
    
    public required Project Project { get; init; }
    
    public required CancellationToken CancellationToken { get; init; }
    
    public required IServiceScope ServiceScope { get; init; }
    
    public Task Run(Type outputType)
    {
        var ctx = new PipelineContext()
        {
            TargetType = outputType,
            CreateTarget = CreateSourceOutput,
            Project = Project,
            CancellationToken = CancellationToken,
            Services = ServiceScope.ServiceProvider
        };

        return CallPipelineStep(ctx, 0);
    }

    private Task CallPipelineStep(PipelineContext pipelineContext, int stepIndex)
    {
        if (stepIndex < PipelineSteps.Count)
            return PipelineSteps[stepIndex](pipelineContext, newCtx =>
                CallPipelineStep(newCtx, stepIndex + 1));

        var creationContext = new TargetCreationContext(pipelineContext.TargetType, pipelineContext.Services);
        var instance = pipelineContext.CreateTarget(creationContext);
        var outputContext = new SourceOutputContext()
        {
            Files = Files,
            Errors = Errors,
            Project = pipelineContext.Project,
            CancellationToken = CancellationToken
        };
        return instance.GetSource(outputContext);
    }
    
    private static ISourceOutput CreateSourceOutput(TargetCreationContext ctx)
    {
        var type = ctx.TargetType;
        
        var ctor = type.GetConstructor(
            BindingFlags.Instance | BindingFlags.Public,
            Array.Empty<Type>());

        if (ctor is null)
            throw new InvalidOperationException($"{type.FullName} has no public parameterless constructor.");

        var instance = ctor.Invoke(null);
        return (ISourceOutput)instance;
    }
    
    
    
    private readonly struct SourceOutputContext : ISourceOutputContext
    {
        public required Project Project { get; init; }
    
        public required CancellationToken CancellationToken { get; init; }
        
        public required ConcurrentBag<SourceFile> Files { get; init; }
        
        public required ConcurrentBag<Error> Errors { get; init; }

        public void AddSource(SourceFile file) => Files.Add(file);

        public void AddError(Error error) => Errors.Add(error);
    }
}
