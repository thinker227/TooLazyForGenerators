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
    public static LazyGeneratorBuilder ForLanguage(this LazyGeneratorBuilder builder, string languageName) =>
        builder.Using((ctx, next) =>
            ctx.Project.Language == languageName
                ? next(ctx)
                : Task.CompletedTask);

    /// <summary>
    /// Adds exception handling to the generator pipeline.
    /// </summary>
    /// <param name="builder">The source builder.</param>
    /// <param name="createError">A function to turn an exception into an error.
    /// Will use a default function if not specified.</param>
    public static LazyGeneratorBuilder WithExceptionHandling(
        this LazyGeneratorBuilder builder,
        Func<Exception, ISourceOutputContext, Error>? createError = null)
    {
        createError ??= CreateDefaultError;
        
        return builder.Using(async (ctx, next) =>
        {
            try
            {
                await next(ctx);
            }
            catch (Exception e)
            {
                ctx.AddError(createError(e, ctx));
            }
        });
    }

    private static Error CreateDefaultError(Exception e, ISourceOutputContext _) => new(
        $"An exception of type {e.GetType().Name} occured in the generator pipeline.\n{e}", 
        null);
}
