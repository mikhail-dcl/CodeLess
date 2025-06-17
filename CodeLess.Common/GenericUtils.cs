using Microsoft.CodeAnalysis;
using System.Text;

namespace CodeLess.Common
{
    public static class GenericUtils
    {
        // Check if a symbol denotes a generic argument
        public static bool IsGenericArgument(ISymbol symbol)
        {
            if (symbol.ContainingSymbol is INamedTypeSymbol namedTypeSymbol)
            {
                foreach (ITypeSymbol typeArgument in namedTypeSymbol.TypeArguments)
                {
                    if (SymbolEqualityComparer.Default.Equals(typeArgument, symbol))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool IsGenericType(this ITypeSymbol typeSymbol) =>
            typeSymbol is INamedTypeSymbol { IsGenericType: true, TypeArguments.Length: > 0 };

        public static void GetGenericArguments(this ITypeSymbol typeSymbol, StringBuilder genericArgumentsBuilder)
        {
            if (!(typeSymbol is INamedTypeSymbol { IsGenericType: true } namedTypeSymbol) || namedTypeSymbol.TypeArguments.Length == 0)
                return;

            genericArgumentsBuilder.Append("<");

            for (int i = 0; i < namedTypeSymbol.TypeArguments.Length; i++)
            {
                ITypeSymbol typeArgument = namedTypeSymbol.TypeArguments[i];
                genericArgumentsBuilder.Append(typeArgument.Name);

                if (i < namedTypeSymbol.TypeArguments.Length - 1)
                {
                    genericArgumentsBuilder.Append(", ");
                }
            }

            genericArgumentsBuilder.Append(">");
        }
    }
}
