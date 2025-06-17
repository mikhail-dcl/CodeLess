using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeLess.Common
{
    public readonly struct GeneratorTypeInfo(INamedTypeSymbol symbol, TypeDeclarationSyntax syntax)
    {
        public readonly INamedTypeSymbol Symbol = symbol;
        public readonly TypeDeclarationSyntax Syntax = syntax;
    }
}
