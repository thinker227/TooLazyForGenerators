using TooLazyForGenerators;

namespace PipelineGenerator;

public sealed class OutputA : ISourceOutput
{
    public Task GetSource(ISourceOutputContext ctx)
    {
        Console.WriteLine("Output A");
        return Task.CompletedTask;
    }
}
