using TooLazyForGenerators;

namespace PipelineGenerator;

// TODO: Remove this if pipelines aren't supported anymore.
public sealed class OutputB : SourceOutput
{
    public override void Initialize(SourceOutputContext ctx) => throw new NotImplementedException();
}
