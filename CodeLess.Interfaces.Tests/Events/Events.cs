using NUnit.Framework;

namespace CodeLess.Interfaces.Tests
{
    [TestFixture]
    public class Events
    {
        private const string TEST_DIR = nameof(Events);

        [Test] public Task SimpleEvent()                          => InterfaceVerifier.RunAsync(TEST_DIR, nameof(SimpleEvent));
        [Test] public Task GenericEvent()                         => InterfaceVerifier.RunAsync(TEST_DIR, nameof(GenericEvent));
        [Test] public Task InitializedEvent()                     => InterfaceVerifier.RunAsync(TEST_DIR, nameof(InitializedEvent));
        [Test] public Task ExplicitEvent()                        => InterfaceVerifier.RunAsync(TEST_DIR, nameof(ExplicitEvent));
        [Test] public Task AttributeEvent()                       => InterfaceVerifier.RunAsync(TEST_DIR, nameof(AttributeEvent));
        [Test] public Task MultipleFieldEvents()                  => InterfaceVerifier.RunAsync(TEST_DIR, nameof(MultipleFieldEvents));
        [Test] public Task GenericClassEvent()                    => InterfaceVerifier.RunAsync(TEST_DIR, nameof(GenericClassEvent));
        [Test] public Task EventWithInitializerAndAttributes()    => InterfaceVerifier.RunAsync(TEST_DIR, nameof(EventWithInitializerAndAttributes));
        [Test] public Task XmlDocEvent()                          => InterfaceVerifier.RunAsync(TEST_DIR, nameof(XmlDocEvent));
    }
}
