using NUnit.Framework;

namespace CodeLess.Interfaces.Tests
{
    [TestFixture]
    public class Methods
    {
        private const string TEST_DIR = nameof(Methods);

        [Test]
        public Task AddPublicMethodToInterface() =>
            InterfaceVerifier.RunAsync(TEST_DIR, nameof(AddPublicMethodToInterface));

        [Test]
        public Task AsyncMethodWithIntReturn() =>
            InterfaceVerifier.RunAsync(TEST_DIR, nameof(AsyncMethodWithIntReturn));

        [Test]
        public Task MethodWithFiveParameters() =>
            InterfaceVerifier.RunAsync(TEST_DIR, nameof(MethodWithFiveParameters));

        [Test]
        public Task MethodWithOutParameter() =>
            InterfaceVerifier.RunAsync(TEST_DIR, nameof(MethodWithOutParameter));

        [Test]
        public Task MethodWithInParameter() =>
            InterfaceVerifier.RunAsync(TEST_DIR, nameof(MethodWithInParameter));

        [Test]
        public Task MethodWithRefParameter() =>
            InterfaceVerifier.RunAsync(TEST_DIR, nameof(MethodWithRefParameter));

        [Test]
        public Task MethodOutInRefAndCustomReturn() =>
            InterfaceVerifier.RunAsync(TEST_DIR, nameof(MethodOutInRefAndCustomReturn));

        [Test]
        public Task AbstractClassWithTwoAbstractMethods() =>
            InterfaceVerifier.RunAsync(TEST_DIR, nameof(AbstractClassWithTwoAbstractMethods));

        [Test]
        public Task MethodWithNullableStringParameter() =>
            InterfaceVerifier.RunAsync(TEST_DIR, nameof(MethodWithNullableStringParameter));

        [Test]
        public Task MethodWithNestedTypes() =>
            InterfaceVerifier.RunAsync(TEST_DIR, nameof(MethodWithNestedTypes));
    }
}
