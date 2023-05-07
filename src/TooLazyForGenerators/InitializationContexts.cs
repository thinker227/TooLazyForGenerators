using Microsoft.CodeAnalysis;

namespace TooLazyForGenerators;

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
