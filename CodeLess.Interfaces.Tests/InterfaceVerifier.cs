using CodeLess.Interfaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using Microsoft.CodeAnalysis.Text;
using NUnit.Framework;
using System.Text;

namespace CodeLess.Interfaces.Tests
{
    public class InterfaceVerifier : CSharpSourceGeneratorTest<InterfaceSourceGenerator, NUnitVerifier>
    {
        private InterfaceVerifier()
        {

        }

        public static async Task RunAsync(string testDir, string testName)
        {
            var testRootDir = TestContext.CurrentContext.TestDirectory;
            var inputPath = Path.Combine(testRootDir, testDir, $"{testName}.Input.txt");
            var expectedPath = Path.Combine(testRootDir, testDir, $"{testName}.Expected.txt");

            var inputSource = await File.ReadAllTextAsync(inputPath);
            var expectedSource = await File.ReadAllTextAsync(expectedPath);

            var expectedFileName = $"I{testName}.g.cs";              // e.g. "IAddPublicMethodToInterface.g.cs"

            var verifier = new InterfaceVerifier
            {
                TestState =
                {
                    Sources = { inputSource },
                    ReferenceAssemblies = ReferenceAssemblies.Net.Net90,
                    AdditionalReferences =
                    {
                        MetadataReference.CreateFromFile(typeof(AutoInterfaceAttribute).Assembly.Location)
                    },
                    GeneratedSources =
                    {
                        (typeof(InterfaceSourceGenerator), expectedFileName, SourceText.From(expectedSource, Encoding.UTF8))
                    }
                },
            };

            await verifier.RunAsync();
        }
    }
}
