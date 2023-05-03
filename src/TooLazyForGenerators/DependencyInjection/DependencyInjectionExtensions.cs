using Microsoft.Extensions.DependencyInjection;
using TooLazyForGenerators.Pipelines;

namespace TooLazyForGenerators.DependencyInjection;

public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Adds dependency injection to the generator pipeline.
    /// </summary>
    /// <param name="builder">The source builder.</param>
    public static TBuilder UsingDependencyInjection<TBuilder>(
        this TBuilder builder)
        where TBuilder : IPipelineBuilder
    {
        builder.Using((ctx, next) =>
        {
            ctx.CreateTarget = creationCtx =>
                (ISourceOutput)ActivatorUtilities.CreateInstance(
                    creationCtx.Services,
                    creationCtx.TargetType);
            
            return next(ctx);
        });
        
        return builder;
    }
}
