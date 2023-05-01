namespace TooLazyForGenerators;

public interface ILazyGeneratorBuilder
{
    CancellationToken CancellationToken { get; }
    
    ILazyGeneratorBuilder TargetingProject(FileInfo projectFile);

    ILazyGeneratorBuilder WithOutput(Type outputType);

    ILazyGenerator Build();
}
