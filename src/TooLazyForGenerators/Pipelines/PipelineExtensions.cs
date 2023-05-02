using Microsoft.Extensions.DependencyInjection;

namespace TooLazyForGenerators.Pipelines;

/// <summary>
/// Extensions relating to pipelines. 
/// </summary>
public static class PipelineExtensions
{
    /// <summary>
    /// Adds a pipeline step to the builder which filters for projects with a specific language.
    /// </summary>
    /// <param name="builder">The source builder.</param>
    /// <param name="languageName">The name of the language to filter for.
    /// It is recommended to use <see cref="Microsoft.CodeAnalysis.LanguageNames"/> for common language names.</param>
    public static TBuilder ForLanguage<TBuilder>(
        this TBuilder builder,
        string languageName)
        where TBuilder : IPipelineBuilder
    {
        builder.Using((ctx, next) =>
            ctx.Project.Language == languageName
                ? next(ctx)
                : Task.CompletedTask);
        
        return builder;
    }

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
