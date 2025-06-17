using CodeLess.Attributes;
using CodeLess.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Immutable;
using System.Text;
using System.Threading;

namespace CodeLess.Singletons
{
    [Generator]
    public class SingletonSourceGenerator : IIncrementalGenerator
    {
        private static readonly ThreadLocal<SingletonTemplate> TEMPLATE = new(() => new SingletonTemplate());

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // Filter types marked with [Singleton]
            IncrementalValuesProvider<ClassDeclarationSyntax> typeDeclarations = context.SyntaxProvider
                                                                                        .CreateSyntaxProvider(
                                                                                             static (syntaxNode, _) => syntaxNode is ClassDeclarationSyntax typeDeclaration && AttributesUtils.TryGetAttribute(typeDeclaration.AttributeLists, Consts.ATTRIBUTE_NAME, out var _),
                                                                                             static (ctx, _) => (ClassDeclarationSyntax)ctx.Node
                                                                                         );

            // Combine methods with compilation
            var compilationAndTypes= context.CompilationProvider.Combine(typeDeclarations.Collect());

            context.RegisterSourceOutput(compilationAndTypes, Generate);
        }

        private void Generate(SourceProductionContext context, (Compilation compilation, ImmutableArray<ClassDeclarationSyntax> types) arg)
        {
            (Compilation? compilation, ImmutableArray<ClassDeclarationSyntax> classDeclarationSyntaxes) = arg;

            foreach (ClassDeclarationSyntax classDeclarationSyntax in classDeclarationSyntaxes)
            {
                INamedTypeSymbol? typeSymbol;
                try
                {
                    var semanticModel = compilation.GetSemanticModel(classDeclarationSyntax.SyntaxTree);
                    typeSymbol = (INamedTypeSymbol?) ModelExtensions.GetDeclaredSymbol(semanticModel, classDeclarationSyntax);

                    if (typeSymbol == null)
                        continue;

                    // Get behaviour value from attribute
                    if (!typeSymbol.TryGetAttribute(Consts.ATTRIBUTE_NAME, out var attributeData))
                        continue;

                    SingletonGenerationBehavior behavior = SingletonGenerationBehavior.DEFAULT;
                    if (attributeData is { ConstructorArguments.Length: > 0 })
                    {
                        var argValue = attributeData.ConstructorArguments[0].Value;

                        behavior = argValue switch
                                   {
                                       int intValue => (SingletonGenerationBehavior)intValue,
                                       SingletonGenerationBehavior enumValue => enumValue,
                                       _ => behavior
                                   };
                    }

                    var typeInfo = new GeneratorTypeInfo(typeSymbol, classDeclarationSyntax);
                    // Now you can use 'behavior' as needed

                    var template = TEMPLATE.Value;
                    template.Clear();

                    template.SetClassName(in typeInfo);
                    template.SetTypeGenericArguments(in typeInfo);
                    template.SetNamespace(in typeInfo);
                    template.SetUsings(in typeInfo);
                    template.SetAccessModifier(in typeInfo);

                    if (behavior.HasFlag(SingletonGenerationBehavior.ALLOW_IMPLICIT_CONSTRUCTION))
                        template.SetImplicitInstance();

                    if (behavior.HasFlag(SingletonGenerationBehavior.GENERATE_STATIC_ACCESSORS))
                        template.SetStaticAccessors(in typeInfo);

                    var tree = CSharpSyntaxTree.ParseText(template.BuildSource());
                    var root = (CompilationUnitSyntax)tree.GetRoot();
                    var formatted = root.NormalizeWhitespace(indentation: "    ", eol: Environment.NewLine);

                    context.AddSource(classDeclarationSyntax.Identifier.Text + ".singleton.g.cs", SourceText.From(formatted.ToFullString(), Encoding.UTF8));
                }
                catch
                {
                    continue;
                }
            }
        }
    }
}
