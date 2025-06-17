using CodeLess.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace CodeLess.Interfaces.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AutoInterfaceAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(CodeLessDescriptors.CL011_INTERFACE_TYPE_CANNOT_BE_INFERRED);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSymbolAction(AnalyzeNode, SymbolKind.NamedType);
        }

        // Symbol analyzer: only checks for partial class
        private static void AnalyzeNode(SymbolAnalysisContext context)
        {
            var type = (INamedTypeSymbol)context.Symbol;

            foreach (var declaringSyntaxReference in type.DeclaringSyntaxReferences)
            {
                if (declaringSyntaxReference.GetSyntax() is not ClassDeclarationSyntax classDeclaration
                    || !AttributesUtils.TryGetAttribute(classDeclaration.AttributeLists, Consts.ATTRIBUTE_NAME, out AttributeSyntax _)) continue;

                if (!type.TryGetInterfaceForAutoGeneration(out _))
                {
                    var partialError = Diagnostic.Create(
                        CodeLessDescriptors.CL011_INTERFACE_TYPE_CANNOT_BE_INFERRED,
                        classDeclaration.Identifier.GetLocation(),
                        type.Name);

                    context.ReportDiagnostic(partialError);
                }
            }
        }
    }
}
