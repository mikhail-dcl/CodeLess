using CodeLess.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace CodeLess.Singletons
{
    [Generator]
    public class SingletonRegistryGenerator : IIncrementalGenerator
    {
        private static readonly ThreadLocal<SingletonRegistryTemplate> TEMPLATE = new (() => new SingletonRegistryTemplate());

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // Provide the compilation to the pipeline
            var compilationProvider = context.CompilationProvider;

            // Compute all matching type names once per compilation
            var candidates = compilationProvider.Select((compilation, _) =>
            {
                var attributeSymbol = compilation.GetTypeByMetadataName(Consts.ATTRIBUTE_FULLY_QUALIFIED_NAME);
                if (attributeSymbol == null)
                {
                    return (assemblies: (IReadOnlyList<IAssemblySymbol>)[], types: (IReadOnlyList<ITypeSymbol>)[]);
                }

                // Gather all assemblies (metadata + source), omitting default ones
                var assemblies = compilation.References
                                            .Select(compilation.GetAssemblyOrModuleSymbol)
                                            .OfType<IAssemblySymbol>()
                                            .Append(compilation.Assembly)
                                            .Where(IsUserAssembly)
                                             // Filter out assemblies where the attribute is not defined
                                            //.Where(asm =>
                                            //     RoslynInternalsInvoker.GetTypeByMetadataName(asm, Consts.ATTRIBUTE_FULLY_QUALIFIED_NAME, true, false) != null)
                                            .ToList();

                var results = new List<ITypeSymbol>();
                foreach (var asm in assemblies)
                {
                    CollectAnnotatedTypes(asm.GlobalNamespace, results);
                }

                return (assemblies, types: results);
            });

            // Register a source output to emit the registry
            context.RegisterSourceOutput(candidates, (spc, data) =>
            {
                GenerateRegistrySource(data.assemblies, data.types, spc);
            });
        }

        private static void CollectAnnotatedTypes(INamespaceOrTypeSymbol symbol, List<ITypeSymbol> results)
        {
            // Recurse namespaces
            if (symbol is INamespaceSymbol ns)
            {
                foreach (var member in ns.GetMembers())
                {
                    CollectAnnotatedTypes(member, results);
                }
            }
            // Check types (including nested)
            else if (symbol is INamedTypeSymbol type)
            {
                if (type.TypeKind == TypeKind.Class)
                    if (symbol.TryGetAttribute(Consts.ATTRIBUTE_NAME, out _))
                        results.Add(type);

                foreach (var nested in type.GetTypeMembers())
                    CollectAnnotatedTypes(nested, results);
            }
        }

        private void GenerateRegistrySource(IReadOnlyList<IAssemblySymbol> assemblies, IReadOnlyList<ITypeSymbol> types, SourceProductionContext spc)
        {
            var template = TEMPLATE.Value;
            template.Clear();

            // Add visited assemblies info
            foreach (var asm in assemblies)
            {
                template.AppendVisitedAssembly(asm.Identity.Name);
            }

            foreach (ITypeSymbol typeInfo in types)
            {
                template.AppendUsings(in typeInfo);
                template.AppendResetMethod(in typeInfo);
            }

            var tree = CSharpSyntaxTree.ParseText(template.BuildSource());
            var root = (CompilationUnitSyntax)tree.GetRoot();
            var formatted = root.NormalizeWhitespace(indentation: "    ", eol: Environment.NewLine);

            spc.AddSource("SingletonRegistry.g.cs", SourceText.From(formatted.ToFullString(), Encoding.UTF8));
        }

        private static bool IsUserAssembly(IAssemblySymbol asm)
        {
            var name = asm.Identity.Name;

            // 1) Core .NET assemblies
            var corePrefixes = new[]
            {
                "mscorlib",
                "netstandard",
                "System",
                "Microsoft"
            };
            if (corePrefixes.Any(prefix =>
                    name.Equals(prefix, StringComparison.OrdinalIgnoreCase) ||
                    name.StartsWith(prefix + ".", StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            // 2) Unity assemblies
            if (name.StartsWith("UnityEngine", StringComparison.OrdinalIgnoreCase) ||
                name.StartsWith("UnityEditor", StringComparison.OrdinalIgnoreCase) ||
                name.StartsWith("Unity.", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            // 3) Popular test frameworks
            if (name.StartsWith("nunit", StringComparison.OrdinalIgnoreCase) ||    // e.g. NUnit.Framework, NUnit.Engine
                name.StartsWith("xunit", StringComparison.OrdinalIgnoreCase) ||    // e.g. xunit.core, xunit.assert
                name.StartsWith("mstest", StringComparison.OrdinalIgnoreCase) ||   // e.g. MSTest.TestFramework
                name.IndexOf("TestFramework", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return false;
            }

            // 4) (Optional) any other assemblies you know you want to skip:
            //    e.g. Moq, AutoFixture, etc.  Just add more conditions here.

            // Everything else is considered “user” or third-party library
            return true;
        }

    }
}
