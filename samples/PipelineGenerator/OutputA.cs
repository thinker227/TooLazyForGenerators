using TooLazyForGenerators;

namespace PipelineGenerator;

// TODO: Remove this if pipelines aren't supported anymore.
public sealed class OutputA : SourceOutput
{
    public override void Initialize(SourceOutputContext ctx) => throw new NotImplementedException();
}
