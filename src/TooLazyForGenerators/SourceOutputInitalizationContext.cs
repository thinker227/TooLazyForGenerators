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
public readonly struct SourceOutputInitalizationContext
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

/// <summary>
/// A context for a <see cref="SyntaxTree"/>.
/// </summary>
/// <param name="Tree">The syntax tree.</param>
/// <param name="IsGenerated">Whether the syntax tree represents generated code or not.</param>
public readonly record struct SyntaxTreeContext(
    SyntaxTree Tree,
    bool IsGenerated);

/// <summary>
/// A context for a <see cref="SemanticModel"/>.
/// </summary>
/// <param name="SemanticModel">The semantic model.</param>
/// <param name="IsGenerated">Whether the semantic model represents generated code or not.</param>
public readonly record struct SemanticModelContext(
    SemanticModel SemanticModel,
    bool IsGenerated);

/// <summary>
/// A context for a <see cref="SyntaxNode"/>.
/// </summary>
/// <param name="Node">The syntax node.</param>
/// <param name="Compilation">The compilation which compiled the node.</param>
/// <param name="SemanticModel">The semantic model containing semantic information about the node.</param>
/// <param name="ContainingSymbol">The symbol the node is contained within,
/// for instance a method or type.</param>
/// <param name="IsGenerated">Whether the syntax node represents generated code or not.</param>
public readonly record struct SyntaxNodeContext(
    SyntaxNode Node,
    Compilation Compilation,
    SemanticModel SemanticModel,
    ISymbol? ContainingSymbol,
    bool IsGenerated);

/// <summary>
/// A context for an <see cref="ISymbol"/>.
/// </summary>
/// <param name="Symbol">The symbol.</param>
/// <param name="Compilation">The compilation which compiled the symbol.</param>
/// <param name="IsGenerated">Whether the symbol represents generated code or not.</param>
public readonly record struct SymbolContext(
    ISymbol Symbol,
    Compilation Compilation,
    bool IsGenerated);

/// <summary>
/// A context for an <see cref="IOperation"/>.
/// </summary>
/// <param name="Operation">The operation.</param>
/// <param name="Compilation">The compilation which compiled the operation.</param>
/// <param name="ContainingSymbol">The symbol the operation is contained within,
/// for instance a method or type.</param>
/// <param name="IsGenerated">Whether the operation represents generated code or not.</param>
public readonly record struct OperationContext(
    IOperation Operation,
    Compilation Compilation,
    ISymbol ContainingSymbol,
    bool IsGenerated);

/// <summary>
/// A context for a code block.
/// </summary>
/// <param name="CodeBlock">The syntax node representing the code block.</param>
/// <param name="SemanticModel">The semantic model containing semantic information about the code block.</param>
/// <param name="OwningSymbol">The symbol for which the code block provides a value or definition,
/// for instance a property or method.</param>
/// <param name="IsGenerated">Whether the code block represents generated code or not.</param>
public readonly record struct BlockContext(
    SyntaxNode CodeBlock,
    SemanticModel SemanticModel,
    ISymbol OwningSymbol,
    bool IsGenerated);
