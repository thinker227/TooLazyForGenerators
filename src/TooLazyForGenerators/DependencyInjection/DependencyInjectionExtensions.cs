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
            ctx.CreateTarget = (type, services) =>
                (ISourceOutput)ActivatorUtilities.CreateInstance(services, type);
            
            return next(ctx);
        });
        
        return builder;
    }
}
