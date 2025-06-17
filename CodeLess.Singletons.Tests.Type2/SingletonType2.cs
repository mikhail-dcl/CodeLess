using CodeLess.Attributes;

namespace CodeLess.Tests
{
    [Singleton]
    public partial class SingletonType2
    {
        private readonly string param;

        public SingletonType2(string param)
        {
            this.param = param;
        }
    }
}
