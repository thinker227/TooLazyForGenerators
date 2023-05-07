using System.Collections.Concurrent;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace TooLazyForGenerators;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class GeneratorAnalyzerWrapper : DiagnosticAnalyzer
{
    private readonly ISourceOutput sourceOutput;
    private readonly ExecutionOptions options;
    private readonly ConcurrentBag<SourceFile> files;
    private readonly ConcurrentBag<Error> errors;

    public GeneratorAnalyzerWrapper(
        ISourceOutput sourceOutput,
        ExecutionOptions options,
        ConcurrentBag<SourceFile> files,
        ConcurrentBag<Error> errors)
    {
        this.sourceOutput = sourceOutput;
        this.options = options;
        this.files = files;
        this.errors = errors;
    }

    // This diagnostic should never occur, but it is required for the analyzer to run. 
    private static readonly DiagnosticDescriptor negativeOneDollarBill = new(
        "TLFG0000",
        "$-1",
        "This diagnostic should never occur.",
        "Lazy",
        DiagnosticSeverity.Hidden,
        true);
    
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(negativeOneDollarBill);

    public override void Initialize(AnalysisContext context)
    {
        if (options.RunConcurrently) context.EnableConcurrentExecution();
        
        context.ConfigureGeneratedCodeAnalysis(
            options.RunForGeneratedCode 
                ? GeneratedCodeAnalysisFlags.Analyze
                : GeneratedCodeAnalysisFlags.None);

        var sourceOutputContext = new NewSourceOutputContext(context, files, errors);
        
        sourceOutput.Initialize(sourceOutputContext);
    }
}
