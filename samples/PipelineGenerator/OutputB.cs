using TooLazyForGenerators;

namespace PipelineGenerator;

public sealed class OutputB : ISourceOutput
{
    public Task GetSource(SourceOutputContext ctx)
    {
        Console.WriteLine("Output B");
        return Task.CompletedTask;
    }
}
