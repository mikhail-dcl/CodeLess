using CodeLess.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeLess.Interfaces
{
    public static class AutoInterfaceUtils
    {
        public static bool TryGetInterfaceForAutoGeneration(this INamedTypeSymbol typeSymbol, out INamedTypeSymbol? @interface)
        {
            // The interface must be an error type symbol, which indicates that it is not defined
            // The interface must contain the same generic signature as the type symbol

            @interface = typeSymbol.Interfaces
                                   .Append(typeSymbol.BaseType)
                                   .OfType<IErrorTypeSymbol>()
                                   .FirstOrDefault(i => i.TypeArguments.SequenceEqual(typeSymbol.TypeArguments));

            // If the undefined interface is not found the second candidate is a partial interface
            if (@interface == null)
            {
                @interface = typeSymbol.Interfaces
                                       .FirstOrDefault(i => i.TryGetSyntax(out InterfaceDeclarationSyntax declarationSyntax)
                                                            && declarationSyntax.IsPartial()
                                                            && i.TypeArguments.SequenceEqual(typeSymbol.TypeArguments));
            }

            return @interface != null;
        }
    }
}
