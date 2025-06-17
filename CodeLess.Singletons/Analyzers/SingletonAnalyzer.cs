using CodeLess.Attributes;
using CodeLess.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace CodeLess.Singletons
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class SingletonAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(CodeLessDescriptors.CL0001_CLASS_MUST_BE_PARTIAL,
                                  CodeLessDescriptors.CL0002_CLASS_MUST_HAVE_PARAMETERLESS_CONSTRUCTOR,
                                  CodeLessDescriptors.CL0003_INTERNAL_MEMBER_MUST_BEGIN_WITH_LOWERCASE);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSymbolAction(AnalyzeNode, SymbolKind.NamedType);
            context.RegisterSyntaxNodeAction(AnalyzeClassDeclaration, SyntaxKind.ClassDeclaration);
        }

        // Symbol analyzer: only checks for partial class
        private static void AnalyzeNode(SymbolAnalysisContext context)
        {
            var type = (INamedTypeSymbol)context.Symbol;

            foreach (var declaringSyntaxReference in type.DeclaringSyntaxReferences)
            {
                if (declaringSyntaxReference.GetSyntax() is not ClassDeclarationSyntax classDeclaration
                    || !AttributesUtils.TryGetAttribute(classDeclaration.AttributeLists, Consts.ATTRIBUTE_NAME, out AttributeSyntax _)) continue;

                if (!classDeclaration.IsPartial())
                {
                    var partialError = Diagnostic.Create(
                        CodeLessDescriptors.CL0001_CLASS_MUST_BE_PARTIAL,
                        classDeclaration.Identifier.GetLocation(),
                        type.Name);

                    context.ReportDiagnostic(partialError);
                }
            }
        }

        // Syntax analyzer: handles semantic model usage for attribute argument evaluation
        private static void AnalyzeClassDeclaration(SyntaxNodeAnalysisContext context)
        {
            var classDeclaration = (ClassDeclarationSyntax)context.Node;

            if (!AttributesUtils.TryGetAttribute(classDeclaration.AttributeLists, Consts.ATTRIBUTE_NAME, out var attributeSyntax))
                return;

            var typeSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration);

            if (typeSymbol == null)
                return;

            var behaviorValue = SingletonGenerationBehavior.DEFAULT;

            if (attributeSyntax.ArgumentList is { Arguments.Count: > 0 })
            {
                foreach (var arg in attributeSyntax.ArgumentList.Arguments)
                {
                    if (arg.NameEquals == null || arg.NameEquals.Name.Identifier.Text == nameof(SingletonAttribute.Behavior))
                    {
                        var constant = context.SemanticModel.GetConstantValue(arg.Expression);

                        if (constant is { HasValue: true, Value: int intValue })
                        {
                            behaviorValue = (SingletonGenerationBehavior)intValue;
                            break;
                        }
                    }
                }
            }

            if ((behaviorValue & SingletonGenerationBehavior.ALLOW_IMPLICIT_CONSTRUCTION) != 0)
            {
                bool hasParameterlessCtor = typeSymbol.Constructors.Any(ctor => ctor.DeclaredAccessibility == Accessibility.Public &&
                                                                                ctor.Parameters.Length == 0);

                if (!hasParameterlessCtor)
                {
                    var ctorError = Diagnostic.Create(
                        CodeLessDescriptors.CL0002_CLASS_MUST_HAVE_PARAMETERLESS_CONSTRUCTOR,
                        classDeclaration.Identifier.GetLocation(),
                        typeSymbol.Name);

                    context.ReportDiagnostic(ctorError);
                }
            }
        }
    }
}
