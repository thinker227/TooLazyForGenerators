using Microsoft.CodeAnalysis;
using TooLazyForGenerators;

var builder = new LazyGeneratorBuilder();
await builder.TargetingProjectWithName("ExampleApp");
builder.WithOutputsFromAssembly();

builder.Using((ctx, next) =>
{
    // Print a message before each call to an output.
    Console.WriteLine("Hewwo from the pipeline!");
    return next(ctx);
});

builder.Using(async (ctx, next) =>
{
    // Filter for only C# projects.
    if (ctx.Project.Language == LanguageNames.CSharp)
        await next(ctx);
});

using var output = await builder.Build().Run();
output.WriteAndReturn();
