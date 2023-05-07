using System.Collections.Concurrent;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace TooLazyForGenerators;

// Analyzers created manually don't require the [DiagnosticAnalyzer] attribute.
#pragma warning disable RS1001

public sealed class GeneratorAnalyzerWrapper : DiagnosticAnalyzer
{
    private readonly Type sourceOutputType;
    private readonly IServiceProvider services;
    private readonly ExecutionOptions options;
    private readonly ConcurrentBag<SourceFile> files;
    private readonly ConcurrentBag<Error> errors;

    public GeneratorAnalyzerWrapper(
        Type sourceOutputType,
        IServiceProvider services,
        ExecutionOptions options,
        ConcurrentBag<SourceFile> files,
        ConcurrentBag<Error> errors)
    {
        this.sourceOutputType = sourceOutputType;
        this.services = services;
        this.options = options;
        this.files = files;
        this.errors = errors;
    }

    // The analyzer shouldn't actually produce any diagnostics.
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray<DiagnosticDescriptor>.Empty;

    public override void Initialize(AnalysisContext context)
    {
        if (options.RunConcurrently) context.EnableConcurrentExecution();
        
        context.ConfigureGeneratedCodeAnalysis(
            options.RunForGeneratedCode 
                ? GeneratedCodeAnalysisFlags.Analyze
                : GeneratedCodeAnalysisFlags.None);

        var sourceOutputContext = new NewSourceOutputContext(context, files, errors);
        var sourceOutput = CreateSourceOutput();
        
        sourceOutput.Initialize(sourceOutputContext);
    }

    private ISourceOutput CreateSourceOutput() =>
        (ISourceOutput)ActivatorUtilities.CreateInstance(services, sourceOutputType);
}
