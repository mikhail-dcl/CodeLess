using NUnit.Framework;

namespace CodeLess.Interfaces.Tests
{
    [TestFixture]
    public class Properties
    {
        private const string TEST_DIR = nameof(Properties);

        [Test] public Task AutoProperty_ReadWrite()              => InterfaceVerifier.RunAsync(TEST_DIR, nameof(AutoProperty_ReadWrite));
        [Test] public Task AutoProperty_ReadOnly()               => InterfaceVerifier.RunAsync(TEST_DIR, nameof(AutoProperty_ReadOnly));
        [Test] public Task AutoProperty_WriteOnly()              => InterfaceVerifier.RunAsync(TEST_DIR, nameof(AutoProperty_WriteOnly));
        [Test] public Task InitOnlyProperty()                    => InterfaceVerifier.RunAsync(TEST_DIR, nameof(InitOnlyProperty));
        [Test] public Task IndexedProperty()                     => InterfaceVerifier.RunAsync(TEST_DIR, nameof(IndexedProperty));
        [Test] public Task ExpressionBodiedProperty()            => InterfaceVerifier.RunAsync(TEST_DIR, nameof(ExpressionBodiedProperty));
        [Test] public Task PropertyWithInternalGetter()          => InterfaceVerifier.RunAsync(TEST_DIR, nameof(PropertyWithInternalGetter));
        [Test] public Task PropertyWithInternalSetter()          => InterfaceVerifier.RunAsync(TEST_DIR, nameof(PropertyWithInternalSetter));
        [Test] public Task PropertyWithAttributes()              => InterfaceVerifier.RunAsync(TEST_DIR, nameof(PropertyWithAttributes));
        [Test] public Task GenericTypeProperty()                 => InterfaceVerifier.RunAsync(TEST_DIR, nameof(GenericTypeProperty));
    }
}
