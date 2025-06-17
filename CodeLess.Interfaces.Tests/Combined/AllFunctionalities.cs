using NUnit.Framework;

namespace CodeLess.Interfaces.Tests
{
    public class Compilation
    {
        [TestFixture]
        public class Combined
        {
            private const string TEST_DIR = nameof(Combined);

            [Test]
            public Task AllFunctionalities() =>
                InterfaceVerifier.RunAsync(TEST_DIR, nameof(AllFunctionalities));
        }
    }
}
