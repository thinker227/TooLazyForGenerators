using TooLazyForGenerators;

namespace PipelineGenerator;

public sealed class OutputB : ISourceOutput
{
    public Task GetSource(ISourceOutputContext ctx)
    {
        Console.WriteLine("Output B");
        return Task.CompletedTask;
    }
}
