using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace CodeLess.Common
{
    public static class GeneratorUtils
    {
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
