using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace CodeLess.Common
{
    public static class AttributesUtils
    {
        public static bool TryGetAttribute(in SyntaxList<AttributeListSyntax> attributes, string attributeName, out AttributeSyntax attributeSyntax)
        {
            attributeSyntax = null!;

            foreach (var attributeList in attributes)
            {
                foreach (var attribute in attributeList.Attributes)
                {
                    var attributeStr = attribute.Name.ToString();

                    if (attributeStr == attributeName || attributeStr + nameof(Attribute) == attributeName)
                    {
                        attributeSyntax = attribute;
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool TryGetAttribute(this ISymbol symbol, string attributeName, out AttributeData attributeData)
        {
            attributeData = null!;

            foreach (var attribute in symbol.GetAttributes())
            {
                var attributeStr = attribute.AttributeClass?.Name;

                if (attributeStr == null)
                    continue;

                if (attributeStr == attributeName || attributeStr + nameof(Attribute) == attributeName)
                {
                    attributeData = attribute;
                    return true;
                }
            }

            return false;
        }
    }
}
