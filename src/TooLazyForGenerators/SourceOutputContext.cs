using System.Collections.Concurrent;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace TooLazyForGenerators;

/// <summary>
/// A context for an <see cref="ISourceOutput"/>.
/// </summary>
public readonly struct SourceOutputContext : ISourceAndErrors
{
    private readonly ConcurrentBag<SourceFile> files;
    private readonly ConcurrentBag<Error> errors;
    
    /// <summary>
    /// The project which is being targeted.
    /// </summary>
    public Project Project { get; }
    
    /// <summary>
    /// The cancellation token for the operation.
    /// </summary>
    public CancellationToken CancellationToken { get; }

    /// <summary>
    /// Initializes a new <see cref="SourceOutputContext"/> instance.
    /// </summary>
    /// <param name="project">The project which is being targeted.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <param name="files">A collection of source files to add files to.</param>
    /// <param name="errors">A collection of errors to add errors to.</param>
    public SourceOutputContext(
        Project project,
        CancellationToken cancellationToken,
        ConcurrentBag<SourceFile> files,
        ConcurrentBag<Error> errors)
    {
        this.files = files;
        this.errors = errors;
        Project = project;
        CancellationToken = cancellationToken;
    }

    public void AddSource(SourceFile file) =>
        files.Add(file);

    public void AddError(Error error) =>
        errors.Add(error);
}

// TODO: Rename this to just SourceOutputContext once the old SourceOutputContext is removed.
public readonly struct NewSourceOutputContext
{
    private readonly AnalysisContext context;
    private readonly ConcurrentBag<SourceFile> files;
    private readonly ConcurrentBag<Error> errors;
    
    public NewSourceOutputContext(
        AnalysisContext context,
        ConcurrentBag<SourceFile> files,
        ConcurrentBag<Error> errors)
    {
        this.context = context;
        this.files = files;
        this.errors = errors;
    }

    public NewSourceOutputContext ForCompilation(Action<Compilation> action)
    {
        context.RegisterCompilationAction(ctx => action(ctx.Compilation));
        
        return this;
    }

    public NewSourceOutputContext ForSyntaxTree(Action<SyntaxTreeContext> action)
    {
        context.RegisterSyntaxTreeAction(ctx => action(new(
            ctx.Tree,
            ctx.IsGeneratedCode)));
            
        return this;
    }

    public NewSourceOutputContext ForSemanticModel(Action<SemanticModelContext> action)
    {
        context.RegisterSemanticModelAction(ctx => action(new(
            ctx.SemanticModel,
            ctx.IsGeneratedCode)));
            
        return this;
    }

    public NewSourceOutputContext ForSyntaxNode(Action<SyntaxNodeContext> action, ImmutableArray<SyntaxKind> syntaxKinds)
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

    public NewSourceOutputContext ForSymbol(Action<SymbolContext> action, ImmutableArray<SymbolKind> symbolKinds)
    {
        context.RegisterSymbolAction(ctx => action(new(
                ctx.Symbol,
                ctx.Compilation,
                ctx.IsGeneratedCode)),
            symbolKinds);
            
        return this;
    }

    public NewSourceOutputContext ForOperation(Action<OperationContext> action, ImmutableArray<OperationKind> operationKinds)
    {
        context.RegisterOperationAction(ctx => action(new(
                ctx.Operation,
                ctx.Compilation,
                ctx.ContainingSymbol,
                ctx.IsGeneratedCode)),
            operationKinds);
            
        return this;
    }

    public NewSourceOutputContext ForCodeBlock(Action<BlockContext> action)
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
