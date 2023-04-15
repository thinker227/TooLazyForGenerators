using TooLazyForGenerators;

var builder = new LazyGeneratorBuilder();
await builder.TargetingProjectWithName("TooLazyForGenerators.Sample");
builder.WithOutputsFromAssembly();

var generator = builder.Build();

var output = await generator.Run();
return output.WriteAndReturn();
