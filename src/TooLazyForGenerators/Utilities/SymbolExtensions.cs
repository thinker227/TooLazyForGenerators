using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using TooLazyForGenerators.Text;

namespace TooLazyForGenerators.Utilities;

public static class SymbolExtensions
{
    // Adapted from source:
    // https://github.com/CommunityToolkit/dotnet/blob/2b3e604cf66af5bf1b48fde0bf9efaf45c3195bf/src/CommunityToolkit.Mvvm.SourceGenerators/Extensions/ITypeSymbolExtensions.cs#L167
    /// <summary>
    /// Gets the fully qualified metadata name of a type symbol.
    /// This is mainly useful for checking whether a type is equal to another type without having
    /// to have an <see cref="ITypeSymbol"/> corresponding to the type to compare to.
    /// </summary>
    /// <param name="typeSymbol">The type symbol to get the metadata name of.</param>
    public static string GetFullyQualifiedMetadataName(this ITypeSymbol typeSymbol)
    {
        StringBuilder builder = new();
        BuildFrom(typeSymbol, builder);
        return builder.ToString();
        
        static void BuildFrom(ISymbol? symbol, in StringBuilder builder)
        {
            switch (symbol)
            {
            // Namespaces that are nested also append a leading '.'
            case INamespaceSymbol { ContainingNamespace.IsGlobalNamespace: false }:
                BuildFrom(symbol.ContainingNamespace, in builder);
                builder.Append('.');
                builder.Append(symbol.MetadataName.AsSpan());
                break;

            // Other namespaces (ie. the one right before global) skip the leading '.'
            case INamespaceSymbol { IsGlobalNamespace: false }:
                builder.Append(symbol.MetadataName.AsSpan());
                break;

            // Types with no namespace just have their metadata name directly written
            case ITypeSymbol { ContainingSymbol: INamespaceSymbol { IsGlobalNamespace: true } }:
                builder.Append(symbol.MetadataName.AsSpan());
                break;

            // Types with a containing non-global namespace also append a leading '.'
            case ITypeSymbol { ContainingSymbol: INamespaceSymbol namespaceSymbol }:
                BuildFrom(namespaceSymbol, in builder);
                builder.Append('.');
                builder.Append(symbol.MetadataName.AsSpan());
                break;

            // Nested types append a leading '+'
            case ITypeSymbol { ContainingSymbol: ITypeSymbol typeSymbol }:
                BuildFrom(typeSymbol, in builder);
                builder.Append('+');
                builder.Append(symbol.MetadataName.AsSpan());
                break;
            }
        }
    }

    private static ImmutableArray<AttributeData> GetAttributeDataForAttribute(
        this ISymbol symbol,
        Func<INamedTypeSymbol, bool> isTargetAttribute)
    {
        var attributes = symbol.GetAttributes();
        var builder = ImmutableArray.CreateBuilder<AttributeData>();

        for (var i = 0; i < attributes.Length; i++)
        {
            var data = attributes[i];
            
            if (data.AttributeClass is null) continue;

            var attributeClass = data.AttributeClass;
            if (attributeClass.IsGenericType) attributeClass = attributeClass.ConstructedFrom;
            if (!isTargetAttribute(attributeClass)) continue;
            
            builder.Add(data);
        }

        return builder.ToImmutable();
    }

    /// <summary>
    /// Gets a collection of <see cref="AttributeData"/> representing attributes of a given type present on a symbol.
    /// </summary>
    /// <param name="symbol">The symbol to get the attributes on.</param>
    /// <param name="attributeType">The type of the attribute to get.</param>
    /// <returns>A collection containing data about all applied attributes
    /// of type <paramref name="attributeType"/>.</returns>
    /// <remarks>
    /// If <paramref name="attributeType"/> is a generic type, then the <see cref="INamedTypeSymbol.ConstructedFrom"/>
    /// type will be used for equality comparisons,
    /// such that you can use this method to get generic attributes as well as non-generic ones.
    /// </remarks>
    public static ImmutableArray<AttributeData> GetAttributeDataForAttribute(
        this ISymbol symbol,
        INamedTypeSymbol attributeType) =>
        symbol.GetAttributeDataForAttribute(atr =>
            atr.Equals(attributeType, SymbolEqualityComparer.Default));

    /// <summary>
    /// Gets a collection of <see cref="AttributeData"/> representing attributes of a given type present on a symbol.
    /// </summary>
    /// <param name="symbol">The symbol to get the attributes on.</param>
    /// <param name="attributeMetadataName">The metadata name of the type of the attribute to get.</param>
    /// <returns>A collection containing data about all applied attributes
    /// which attribute type has the metadata name <paramref name="attributeMetadataName"/>.</returns>
    /// <remarks>
    /// If <paramref name="attributeMetadataName"/> is the metadata name of a generic type,
    /// then the <see cref="INamedTypeSymbol.ConstructedFrom"/> type will be used for equality comparisons,
    /// such that you can use this method to get generic attributes as well as non-generic ones.
    /// <br/>
    /// <br/>
    /// Note that this method calls <see cref="GetFullyQualifiedMetadataName"/> for each attribute
    /// present on the symbol, thus <see cref="GetAttributeDataForAttribute(Microsoft.CodeAnalysis.ISymbol,Microsoft.CodeAnalysis.INamedTypeSymbol)"/>
    /// may be more efficient than this method.
    /// </remarks>
    public static ImmutableArray<AttributeData> GetAttributeDataForAttribute(
        this ISymbol symbol,
        string attributeMetadataName) =>
        symbol.GetAttributeDataForAttribute(atr =>
            atr.GetFullyQualifiedMetadataName() == attributeMetadataName);

    private static bool HasAttribute(this ISymbol symbol, Func<INamedTypeSymbol, bool> isTargetAttribute)
    {
        var attributes = symbol.GetAttributes();

        for (var i = 0; i < attributes.Length; i++)
        {
            var data = attributes[i];
            
            if (data.AttributeClass is null) continue;

            var attributeClass = data.AttributeClass;
            if (attributeClass.IsGenericType) attributeClass = attributeClass.ConstructedFrom;
            if (isTargetAttribute(attributeClass)) return true;
        }

        return false;
    }

    /// <summary>
    /// Returns whether a symbol has an attribute of a given type.
    /// </summary>
    /// <param name="symbol">The symbol to check.</param>
    /// <param name="attributeType">The type of the attribute to check for.</param>
    /// <returns>Whether any of the attributes on <paramref name="symbol"/>
    /// is of type <paramref name="attributeType"/>.</returns>
    /// <remarks>
    /// If <paramref name="attributeType"/> is a generic type, then the <see cref="INamedTypeSymbol.ConstructedFrom"/>
    /// type will be used for equality comparisons,
    /// such that you can use this method to check for generic attributes as well as non-generic ones.
    /// </remarks>
    public static bool HasAttribute(this ISymbol symbol, INamedTypeSymbol attributeType) =>
        symbol.HasAttribute(atr =>
            atr.Equals(attributeType, SymbolEqualityComparer.Default));

    /// <summary>
    /// Returns whether a symbol has an attribute of a given type.
    /// </summary>
    /// <param name="symbol">The symbol to check.</param>
    /// <param name="attributeMetadataName">The metadata name of the type of the attribute to check for.</param>
    /// <returns>Whether the metadata name of any of the attributes on <paramref name="symbol"/>
    /// is equal to <paramref name="attributeMetadataName"/>.</returns>
    /// <remarks>
    /// If <paramref name="attributeMetadataName"/> is the metadata name of a generic type,
    /// then the <see cref="INamedTypeSymbol.ConstructedFrom"/> type will be used for equality comparisons,
    /// such that you can use this method to check for generic attributes as well as non-generic ones.
    /// <br/>
    /// <br/>
    /// Note that this method calls <see cref="GetFullyQualifiedMetadataName"/> for each attribute
    /// present on the symbol, thus <see cref="HasAttribute(Microsoft.CodeAnalysis.ISymbol, Microsoft.CodeAnalysis.INamedTypeSymbol)"/>
    /// may be more efficient than this method.
    /// </remarks>
    public static bool HasAttribute(this ISymbol symbol, string attributeMetadataName) =>
        symbol.HasAttribute(atr =>
            atr.GetFullyQualifiedMetadataName() == attributeMetadataName);
}
