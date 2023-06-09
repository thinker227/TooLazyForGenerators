﻿using System.Collections.Concurrent;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace TooLazyForGenerators;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal sealed class GeneratorAnalyzerWrapper : DiagnosticAnalyzer
{
    private readonly SourceOutput sourceOutput;
    private readonly ExecutionOptions options;

    public GeneratorAnalyzerWrapper(
        SourceOutput sourceOutput,
        ExecutionOptions options)
    {
        this.sourceOutput = sourceOutput;
        this.options = options;
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

        var sourceOutputContext = new SourceOutputInitalizationContext(context);
        
        sourceOutput.Initialize(sourceOutputContext);
    }
}
