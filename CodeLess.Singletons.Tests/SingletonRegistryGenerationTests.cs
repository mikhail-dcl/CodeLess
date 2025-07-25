using CodeLess.Attributes;
using CodeLess.Tests;
using CodeLess.Tests.Module1;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;
using System;
using System.Linq;
using System.Reflection;

namespace CodeLess.Singletons.Tests
{
    public class SingletonRegistryGeneratorTests
    {
        [Test]
        public void GeneratesExpectedSingletonRegistrySource()
        {
            // Arrange: create a compilation with the singleton types and generator
            var sources = new[]
            {
                // minimal source for FeatureFlags
                """
                using CodeLess.Attributes;
                using System;

                namespace CodeLess.Singletons.Tests
                {
                    [Singleton]
                    public partial class FeatureFlagsGenerated
                    {
                        internal event Action<float> eventToExpose;

                        /// <summary>
                        /// Property comment
                        /// </summary>
                        internal string propertyToExpose { get; set; } = "default value";

                        /// <summary>
                        /// Comment 1
                        /// </summary>
                        internal void methodToExpose(int args) {}
                    }
                }
                """
            };

            var compilation = CSharpCompilation.Create(
                "TestAssembly",
                syntaxTrees: sources.Select(s => CSharpSyntaxTree.ParseText(s)),
                references: new[]
                {
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(SingletonType1).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(SingletonType2).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(SingletonAttribute).Assembly.Location)
                },
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            );

            var generator = new SingletonRegistryGenerator();

            // Act: run the generator
            GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
            driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out Compilation _, out _);

            var generated = driver.GetRunResult()
                .Results
                .SelectMany(r => r.GeneratedSources)
                .FirstOrDefault(s => s.HintName == "SingletonRegistry.g.cs")
                .SourceText
                .ToString(); // The output is normalized

            var expected = """
                           // <auto-generated>
                           //     The following assemblies were visited to generate this file:
                           //     CodeLess.Singletons.Tests.Type1
                           //     CodeLess.Singletons.Tests.Type2
                           //     CodeLess.Singletons
                           //     TestAssembly
                           // </auto-generated>
                           #nullable enable
                           using CodeLess.Tests.Module1;
                           using CodeLess.Tests;
                           using CodeLess.Singletons.Tests;

                           namespace CodeLess.Singletons
                           {
                               public class SingletonRegistry
                               {
                                   /// <summary>
                                   ///     Resets all singletons registered in the registry.
                                   /// </summary>
                                   /// <param name="dispose">If true, disposable singletons will be disposed before resetting.</param>
                                   public static void Reset(bool dispose = true)
                                   {
                                       SingletonType1.Reset(dispose);
                                       SingletonType2.Reset(dispose);
                                       FeatureFlagsGenerated.Reset(dispose);
                                   }
                               }
                           }

                           """;

            // Assert: whitespace/indentation independent

            var tree = CSharpSyntaxTree.ParseText(expected);
            var root = (CompilationUnitSyntax)tree.GetRoot();
            var formattedExpected = root.NormalizeWhitespace(indentation: "    ", eol: Environment.NewLine).ToFullString();

            Assert.That(generated, Is.EqualTo(formattedExpected));
        }
    }
}
