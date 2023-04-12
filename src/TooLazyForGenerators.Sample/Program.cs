using TooLazyForGenerators;

var builder = LazyGenerator.CreateBuilder();
await builder.TargetingProjectWithName("TooLazyForGenerators.Sample");
builder.WithOutputsFromAssembly();

var generator = builder.Build();

var output = await generator.Run();
return output.WriteAndReturn();
