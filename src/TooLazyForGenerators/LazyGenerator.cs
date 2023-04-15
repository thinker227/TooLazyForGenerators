namespace TooLazyForGenerators;

internal sealed class LazyGenerator : ILazyGenerator
{
    public required IReadOnlyCollection<FileInfo> ProjectFiles { get; init; }
    
    public required IReadOnlyCollection<Type> Outputs { get; init; }

    public Task<GeneratorOutput> Run(CancellationToken cancellationToken = default) => throw new NotImplementedException();
}
