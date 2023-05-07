namespace TooLazyForGenerators;

public struct ExecutionOptions
{
    public bool RunConcurrently { get; set; } = true;

    public bool RunForGeneratedCode { get; set; } = false;
    
    public ExecutionOptions() { }
}
