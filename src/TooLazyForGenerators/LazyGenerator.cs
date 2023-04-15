namespace TooLazyForGenerators;

internal sealed class LazyGenerator : ILazyGenerator
{
    public Task<GeneratorOutput> Run(CancellationToken cancellationToken = default) => throw new NotImplementedException();
}
