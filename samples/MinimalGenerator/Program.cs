using TooLazyForGenerators;

var builder = new LazyGeneratorBuilder();
// Target the project TooLazyForGenerators.Sample.
await builder.TargetingProjectWithName("ExampleApp");
// Use all outputs from the current assembly.
builder.WithOutputsFromAssembly();

// Build the generator and run it.
using var output = await builder.Build().Run();
// Write the output to disk and return a status code.
return output.WriteAndReturn();
