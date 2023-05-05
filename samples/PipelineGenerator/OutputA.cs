using TooLazyForGenerators;

namespace PipelineGenerator;

public sealed class OutputA : ISourceOutput
{
    public Task GetSource(SourceOutputContext ctx)
    {
        Console.WriteLine("Output A");
        return Task.CompletedTask;
    }
}
