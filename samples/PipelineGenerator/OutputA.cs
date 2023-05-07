using TooLazyForGenerators;

namespace PipelineGenerator;

// TODO: Remove this if pipelines aren't supported anymore.
public sealed class OutputA : ISourceOutput
{
    public void Initialize(SourceOutputContext ctx) => throw new NotImplementedException();
}
