using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace TooLazyForGenerators;

/// <summary>
/// An initialization context for source outputs.
/// </summary>
/// <remarks>
/// Use the provided methods in this struct to register varying kinds of actions on different compilation entities.
/// See <seealso href="https://learn.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/compiler-api-model">
/// the compiler API model</seealso> for more information about compilation entities and types.
/// </remarks>
public readonly partial struct SourceOutputInitalizationContext
{
    private readonly AnalysisContext context;
    
    public SourceOutputInitalizationContext(AnalysisContext context) => this.context = context;

    /// <summary>
    /// Registers an action for a <see cref="Compilation"/>.
    /// </summary>
    /// <param name="action">The action to execute for the compilation.</param>
    public SourceOutputInitalizationContext ForCompilation(Action<Compilation> action)
    {
        context.RegisterCompilationAction(ctx => action(ctx.Compilation));
        
        return this;
    }

    /// <summary>
    /// Registers an action for a <see cref="SyntaxTree"/>.
    /// </summary>
    /// <param name="action">The action to execute for the syntax tree.</param>
    public SourceOutputInitalizationContext ForSyntaxTree(Action<SyntaxTreeContext> action)
    {
        context.RegisterSyntaxTreeAction(ctx => action(new(
            ctx.Tree,
            ctx.IsGeneratedCode)));
            
        return this;
    }

    /// <summary>
    /// Registers an action for a <see cref="SemanticModel"/>.
    /// </summary>
    /// <param name="action">The action to execute for the semantic model.</param>
    public SourceOutputInitalizationContext ForSemanticModel(Action<SemanticModelContext> action)
    {
        context.RegisterSemanticModelAction(ctx => action(new(
            ctx.SemanticModel,
            ctx.IsGeneratedCode)));
            
        return this;
    }

    /// <summary>
    /// Registers an action for a <see cref="SyntaxNode"/>.
    /// </summary>
    /// <param name="action">The action to execute for the syntax node.</param>
    /// <param name="syntaxKinds">The syntax kinds for which to execute the action.
    /// For each of the syntax kinds specified, it is guaranteed that the <paramref name="action"/>
    /// will only be executed for the corresponding syntax node types.</param>
    public SourceOutputInitalizationContext ForSyntaxNode(Action<SyntaxNodeContext> action, ImmutableArray<SyntaxKind> syntaxKinds)
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

    /// <summary>
    /// Registers an action for an <see cref="ISymbol"/>.
    /// </summary>
    /// <param name="action">The action to execute for the symbol.</param>
    /// <param name="symbolKinds">The symbol kinds for which to execute the action.
    /// For each of the symbol kinds specified, it is guaranteed that the <paramref name="action"/>
    /// will only be executed for the corresponding symbol types.</param>
    public SourceOutputInitalizationContext ForSymbol(Action<SymbolContext> action, ImmutableArray<SymbolKind> symbolKinds)
    {
        context.RegisterSymbolAction(ctx => action(new(
                ctx.Symbol,
                ctx.Compilation,
                ctx.IsGeneratedCode)),
            symbolKinds);
            
        return this;
    }

    /// <summary>
    /// Registers an action for an <see cref="IOperation"/>.
    /// </summary>
    /// <param name="action">The action to execute for the operation.</param>
    /// <param name="operationKinds">The operation kinds for which to execute the action.
    /// For each of the operation kinds specified, it is guaranteed that the <paramref name="action"/>
    /// will only be executed for the corresponding operation types.</param>
    public SourceOutputInitalizationContext ForOperation(Action<OperationContext> action, ImmutableArray<OperationKind> operationKinds)
    {
        context.RegisterOperationAction(ctx => action(new(
                ctx.Operation,
                ctx.Compilation,
                ctx.ContainingSymbol,
                ctx.IsGeneratedCode)),
            operationKinds);
            
        return this;
    }

    /// <summary>
    /// Registers an action for a code block (a method body or other expression outside a method).
    /// </summary>
    /// <param name="action">The action to execute for the code block.</param>
    public SourceOutputInitalizationContext ForCodeBlock(Action<BlockContext> action)
    {
        context.RegisterCodeBlockAction(ctx => action(new(
            ctx.CodeBlock,
            ctx.SemanticModel,
            ctx.OwningSymbol,
            ctx.IsGeneratedCode)));
            
        return this;
    }
}
