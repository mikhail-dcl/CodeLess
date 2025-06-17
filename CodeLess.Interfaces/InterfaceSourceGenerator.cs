using CodeLess.Common;
using CodeLess.Interfaces.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Text;

namespace CodeLess.Interfaces
{
    [Generator]
    public class InterfaceSourceGenerator : IIncrementalGenerator
    {
        private static readonly ThreadLocal<InterfaceBuilder> TEMPLATE = new(() => new InterfaceBuilder());

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // Filter classes marked with the AutoInterface attribute

            var typeDeclarations = context.SyntaxProvider.ForAttributeWithMetadataName(
                Consts.ATTRIBUTE_FULLY_QUALIFIED_NAME,
                static (node, _) => node is ClassDeclarationSyntax,
                static (context, _) => new GeneratorTypeInfo((INamedTypeSymbol) context.TargetSymbol, (ClassDeclarationSyntax)context.TargetNode));

            var compilationAndTypes = context.CompilationProvider.Combine(typeDeclarations.Collect());

            context.RegisterSourceOutput(compilationAndTypes, Generate);
        }

        private void Generate(SourceProductionContext context, (Compilation Left, ImmutableArray<GeneratorTypeInfo> Right) arg)
        {
            (Compilation? compilation, ImmutableArray<GeneratorTypeInfo> tuples) = arg;

            foreach (GeneratorTypeInfo typeInfo in tuples)
            {
                // Ignore non-public and non-internal classes
                if (typeInfo.Symbol.DeclaredAccessibility is not (Accessibility.Public or Accessibility.Internal))
                    continue;

                if (!typeInfo.Symbol.TryGetAttribute(Consts.ATTRIBUTE_NAME, out _))
                    continue;

                if (!typeInfo.Symbol.TryGetInterfaceForAutoGeneration(out var interfaceErrorTypeSymbol))
                    continue;

                var builder = TEMPLATE.Value;

                builder.Clear();
                builder.SetOriginalTypeName(typeInfo);
                builder.SetClassTrivia(typeInfo);
                builder.SetInterfaceName(interfaceErrorTypeSymbol!);
                builder.SetUsings(typeInfo);
                builder.SetNamespace(typeInfo);
                builder.SetAccessModifier(typeInfo);

                // Get all members and try to add them to the interface if they are not marked by IgnoreAutoInterfaceMemberAttribute
                foreach (ISymbol member in typeInfo.Symbol.GetMembers())
                {
                    if (member.TryGetAttribute(Consts.IGNORE_MEMBER_ATTRIBUTE_NAME, out _))
                        continue;

                    try { builder.TryAddMember(member); }
                    catch (Exception ex)
                    {
                        var diagnostic = Diagnostic.Create(
                            CodeLessDescriptors.GENERATOR_ERROR,
                            Location.None,
                            member.ToDisplayString(),
                            compilation.AssemblyName,
                            ex
                        );
                        context.ReportDiagnostic(diagnostic);
                    }
                }

                var tree = CSharpSyntaxTree.ParseText(builder.BuildSource());
                var root = (CompilationUnitSyntax)tree.GetRoot();
                var formatted = root.NormalizeWhitespace(indentation: "    ", eol: Environment.NewLine);

                context.AddSource(builder.ClassName + ".g.cs", SourceText.From(formatted.ToFullString(), Encoding.UTF8));
            }
        }
    }
}
