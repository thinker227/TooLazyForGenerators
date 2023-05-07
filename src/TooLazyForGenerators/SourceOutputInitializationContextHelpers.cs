using Microsoft.CodeAnalysis;

namespace TooLazyForGenerators;

public readonly partial struct SourceOutputInitalizationContext
{
    /// <summary>
    /// Registers an action for an <see cref="INamedTypeSymbol"/>,
    /// i.e. a declared type.
    /// </summary>
    /// <param name="action">The action to execute for the type.</param>
    /// <param name="filter">A function which determines whether to run the action for a given type.</param>
    /// <param name="kinds">The kinds of types to run the action for.</param>
    public SourceOutputInitalizationContext ForTypes(
        Action<INamedTypeSymbol> action,
        Func<INamedTypeSymbol, bool>? filter = null,
        params TypeKind[] kinds)
    {
        context.RegisterSymbolAction(ctx =>
        {
            var type = (INamedTypeSymbol)ctx.Symbol;
            
            if (filter is not null && !filter(type)) return;
            if (kinds.Length > 0 && !kinds.Contains(type.TypeKind)) return;

            action(type);
        }, SymbolKind.NamedType);
        
        return this;
    }

    /// <summary>
    /// Registers an action for an <see cref="INamedTypeSymbol"/>,
    /// i.e. a declared type.
    /// </summary>
    /// <param name="action">The action to execute for the type.</param>
    /// <param name="kinds">The kinds of types to run the action for.</param>
    public SourceOutputInitalizationContext ForTypes(
        Action<INamedTypeSymbol> action,
        params TypeKind[] kinds) =>
        ForTypes(action, null, kinds);

    private SourceOutputInitalizationContext ForMembers<TMember>(
        Action<TMember> action,
        SymbolKind memberKind,
        Func<TMember, bool>? memberFilter = null,
        Func<INamedTypeSymbol, bool>? typeFilter = null)
        where TMember : ISymbol
    {
        context.RegisterSymbolAction(ctx =>
        {
            var member = (TMember)ctx.Symbol;

            if (typeFilter is not null && !typeFilter(member.ContainingType)) return;
            if (memberFilter is not null && !memberFilter(member)) return;

            action(member);
        }, memberKind);

        return this;
    }

    /// <summary>
    /// Registers an action for an <see cref="IMethodSymbol"/>,
    /// i.e. a method, local function, property body, etc.
    /// </summary>
    /// <param name="action">The action to execute for the method.</param>
    /// <param name="methodFilter">A function which determines whether to run the action for a given method.</param>
    /// <param name="typeFilter">A function which determines whether to run the action for a given method
    /// based on the containing type of the method.</param>
    /// <param name="kinds">The kinds of methods to run the action for.</param>
    public SourceOutputInitalizationContext ForMethods(
        Action<IMethodSymbol> action,
        Func<IMethodSymbol, bool>? methodFilter = null,
        Func<INamedTypeSymbol, bool>? typeFilter = null,
        params MethodKind[] kinds) =>
        ForMembers(method =>
        {
            if (kinds.Length > 0 && !kinds.Contains(method.MethodKind)) return;
            
            action(method);
        }, SymbolKind.Method, methodFilter, typeFilter);

    /// <summary>
    /// Registers an action for an <see cref="IMethodSymbol"/>,
    /// i.e. a method, local function, property body, etc.
    /// </summary>
    /// <param name="action">The action to execute for the method.</param>
    /// <param name="kinds">The kinds of methods to run the action for.</param>
    public SourceOutputInitalizationContext ForMethods(
        Action<IMethodSymbol> action,
        params MethodKind[] kinds) =>
        ForMethods(action, null, null, kinds);

    /// <summary>
    /// Registers an action for an <see cref="IPropertySymbol"/>,
    /// i.e. a property.
    /// </summary>
    /// <param name="action">The action to execute for the property.</param>
    /// <param name="propertyFilter">A function which determines whether to run the action for a given property.</param>
    /// <param name="typeFilter">A function which determines whether to run the action for a given property
    /// based on the containing type of the property.</param>
    public SourceOutputInitalizationContext ForProperties(
        Action<IPropertySymbol> action,
        Func<IPropertySymbol, bool>? propertyFilter = null,
        Func<INamedTypeSymbol, bool>? typeFilter = null) =>
        ForMembers(action, SymbolKind.Property, propertyFilter, typeFilter);

    /// <summary>
    /// Registers an action for an <see cref="IFieldSymbol"/>,
    /// i.e. a field.
    /// </summary>
    /// <param name="action">The action to execute for the property.</param>
    /// <param name="fieldFilter">A function which determines whether to run the action for a given field.</param>
    /// <param name="typeFilter">A function which determines whether to run the action for a given field
    /// based on the containing type of the field.</param>
    public SourceOutputInitalizationContext ForFields(
        Action<IFieldSymbol> action,
        Func<IFieldSymbol, bool>? fieldFilter = null,
        Func<INamedTypeSymbol, bool>? typeFilter = null) =>
        ForMembers(action, SymbolKind.Field, fieldFilter, typeFilter);
}
