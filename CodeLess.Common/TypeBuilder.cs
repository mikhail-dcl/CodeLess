using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeLess.Common
{
    public abstract class TypeBuilder
    {
        public StringBuilder Usings { get; } = new ();
        public StringBuilder NamespaceLeading { get; } = new ();
        public StringBuilder NamespaceTrailing { get; } = new ();
        public StringBuilder AccessModifier { get; } = new ();
        public StringBuilder ClassName { get; } = new ();
        public StringBuilder TypeGenericArguments { get; } = new ();

        internal static void AppendUsing(StringBuilder builder, in GeneratorTypeInfo typeInfo, HashSet<string>? usingsExpressionCollection = null)
        {
            usingsExpressionCollection ??= new HashSet<string>();

            var usings = (typeInfo.Syntax.FirstAncestorOrSelf<CompilationUnitSyntax>()
                                ?.DescendantNodesAndSelf()
                                 .OfType<UsingDirectiveSyntax>() ?? [])
                                 .Select(u => u.ToString().Trim());

            foreach (string @using in usings)
                usingsExpressionCollection.Add(@using);

            builder.Append(string.Join(Environment.NewLine, usingsExpressionCollection));
        }

        internal void SetUsings(in GeneratorTypeInfo typeInfo)
        {
            Usings.Clear();

            AppendUsing(Usings, typeInfo);
        }

        internal void SetNamespace(in GeneratorTypeInfo typeInfo)
        {
            string? @namespace = typeInfo.Syntax.GetNamespace()?.ToString();

            NamespaceLeading.Clear();
            NamespaceTrailing.Clear();
            if (@namespace is null) return;

            NamespaceLeading.Append("namespace ");
            NamespaceLeading.AppendLine(@namespace);
            NamespaceLeading.Append("{");
            NamespaceTrailing.Append("}");
        }

        internal void SetAccessModifier(in GeneratorTypeInfo typeInfo)
        {
            AccessModifier.Clear();
            AccessModifier.Append(GeneratorUtils.AccessibilityToString(typeInfo.Symbol.DeclaredAccessibility));
        }

        internal void SetClassName(in GeneratorTypeInfo typeInfo)
        {
            ClassName.Clear();
            ClassName.Append(typeInfo.Symbol.Name);
        }

        internal void SetTypeGenericArguments(in GeneratorTypeInfo typeInfo)
        {
            TypeGenericArguments.Clear();
            typeInfo.Symbol.GetGenericArguments(TypeGenericArguments);
        }

        internal virtual void Clear()
        {
            Usings.Clear();
            NamespaceLeading.Clear();
            NamespaceTrailing.Clear();
            AccessModifier.Clear();
            ClassName.Clear();
            TypeGenericArguments.Clear();
        }

        internal abstract string BuildSource();
    }
}
