using Microsoft.Extensions.DependencyInjection;

namespace TooLazyForGenerators.DependencyInjection;

public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Adds dependency injection to the generator pipeline.
    /// </summary>
    /// <param name="builder">The source builder.</param>
    public static LazyGeneratorBuilder UsingDependencyInjection(
        this LazyGeneratorBuilder builder)
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
