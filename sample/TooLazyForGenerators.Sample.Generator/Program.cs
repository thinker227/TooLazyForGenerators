using TooLazyForGenerators;

var builder = new LazyGeneratorBuilder();
await builder.TargetingProjectWithName("TooLazyForGenerators.Sample");
builder.WithOutputsFromAssembly();

var output = await builder.Build().Run();
return output.WriteAndReturn();
