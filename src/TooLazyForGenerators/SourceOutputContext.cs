using System.Collections.Concurrent;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace TooLazyForGenerators;

// TODO: Rename this to just SourceOutputContext once the old SourceOutputContext is removed.
public readonly struct SourceOutputContext
{
    private readonly AnalysisContext context;
    
    public SourceOutputContext(AnalysisContext context) => this.context = context;

    public SourceOutputContext ForCompilation(Action<Compilation> action)
    {
        context.RegisterCompilationAction(ctx => action(ctx.Compilation));
        
        return this;
    }

    public SourceOutputContext ForSyntaxTree(Action<SyntaxTreeContext> action)
    {
        context.RegisterSyntaxTreeAction(ctx => action(new(
            ctx.Tree,
            ctx.IsGeneratedCode)));
            
        return this;
    }

    public SourceOutputContext ForSemanticModel(Action<SemanticModelContext> action)
    {
        context.RegisterSemanticModelAction(ctx => action(new(
            ctx.SemanticModel,
            ctx.IsGeneratedCode)));
            
        return this;
    }

    public SourceOutputContext ForSyntaxNode(Action<SyntaxNodeContext> action, ImmutableArray<SyntaxKind> syntaxKinds)
    {
        context.RegisterSyntaxNodeAction(
            ctx => action(new(
                ctx.Node,
                ctx.Compilation,
                ctx.SemanticModel,
                ctx.ContainingSymbol,
                ctx.IsGeneratedCode)),
            syntaxKinds);
            
        return this;
    }

    public SourceOutputContext ForSymbol(Action<SymbolContext> action, ImmutableArray<SymbolKind> symbolKinds)
    {
        context.RegisterSymbolAction(ctx => action(new(
                ctx.Symbol,
                ctx.Compilation,
                ctx.IsGeneratedCode)),
            symbolKinds);
            
        return this;
    }

    public SourceOutputContext ForOperation(Action<OperationContext> action, ImmutableArray<OperationKind> operationKinds)
    {
        context.RegisterOperationAction(ctx => action(new(
                ctx.Operation,
                ctx.Compilation,
                ctx.ContainingSymbol,
                ctx.IsGeneratedCode)),
            operationKinds);
            
        return this;
    }

    public SourceOutputContext ForCodeBlock(Action<BlockContext> action)
    {
        context.RegisterCodeBlockAction(ctx => action(new(
            ctx.CodeBlock,
            ctx.SemanticModel,
            ctx.OwningSymbol,
            ctx.IsGeneratedCode)));
            
        return this;
    }
}

public readonly record struct SyntaxTreeContext(
    SyntaxTree Tree,
    bool IsGenerated);

public readonly record struct SemanticModelContext(
    SemanticModel SemanticModel,
    bool IsGenerated);

public readonly record struct SyntaxNodeContext(
    SyntaxNode Node,
    Compilation Compilation,
    SemanticModel SemanticModel,
    ISymbol? ContainingSymbol,
    bool IsGenerated);

public readonly record struct SymbolContext(
    ISymbol Symbol,
    Compilation Compilation,
    bool IsGenerated);

public readonly record struct OperationContext(
    IOperation Operation,
    Compilation Compilation,
    ISymbol ContainingSymbol,
    bool IsGenerated);

public readonly record struct BlockContext(
    SyntaxNode CodeBlock,
    SemanticModel SemanticModel,
    ISymbol OwningSymbol,
    bool IsGenerated);
