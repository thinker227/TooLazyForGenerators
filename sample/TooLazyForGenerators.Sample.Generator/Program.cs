using TooLazyForGenerators;

var builder = new LazyGeneratorBuilder();
await builder.TargetingProjectWithName("TooLazyForGenerators.Sample");
builder.WithOutputsFromAssembly();

using var output = await builder.Build().Run();
return output.WriteAndReturn();
