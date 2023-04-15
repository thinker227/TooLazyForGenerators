using Microsoft.CodeAnalysis;

namespace TooLazyForGenerators.Sample;

public sealed class SampleOutput : ISourceOutput
{
    public static SampleOutput Create() => throw new NotImplementedException();

    public Task<Output> GetSource(Project project) => throw new NotImplementedException();
}
