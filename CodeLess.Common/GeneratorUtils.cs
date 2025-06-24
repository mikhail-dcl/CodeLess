using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace CodeLess.Common
{
    public static class GeneratorUtils
    {
        private static readonly SymbolDisplayFormat FULLY_QUALIFIED_FORMAT = new (
            globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Omitted,
            typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
            genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
            memberOptions: SymbolDisplayMemberOptions.IncludeContainingType | SymbolDisplayMemberOptions.IncludeParameters,
            miscellaneousOptions: SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers | SymbolDisplayMiscellaneousOptions.UseSpecialTypes);

        public static string GetFullyQualifiedName(this ITypeSymbol typeSymbol) =>
            typeSymbol.ToDisplayString(FULLY_QUALIFIED_FORMAT);

        public static bool IsPartial(this TypeDeclarationSyntax classDeclaration) =>
            classDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword);

        public static string AccessibilityToString(Accessibility accessibility)
        {
            switch (accessibility)
            {
                case Accessibility.Private:
                    return "private";
                case Accessibility.Internal:
                    return "internal";
                case Accessibility.Protected:
                    return "protected";
                case Accessibility.ProtectedOrInternal:
                    return "protected internal";
                default:
                    return "public";
            }
        }

        public static NameSyntax? GetNamespace(this TypeDeclarationSyntax typeDeclarationSyntax) =>
            typeDeclarationSyntax.FirstAncestorOrSelf<NamespaceDeclarationSyntax>()?.Name
            ?? typeDeclarationSyntax.FirstAncestorOrSelf<FileScopedNamespaceDeclarationSyntax>()?.Name;

        public static bool TryGetSyntax<T>(this ISymbol symbol, out T result)
            where T : SyntaxNode
        {
            result = symbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() as T;
            return result != null;
        }

        public static string InheritDoc(this ISymbol source) =>
            $"/// <inheritdoc cref=\"{source.ToDisplayString().Replace('<', '{').Replace('>', '}')}\" />";
    }
}
