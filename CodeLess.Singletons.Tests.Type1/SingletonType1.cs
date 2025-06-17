using CodeLess.Attributes;

namespace CodeLess.Tests.Module1
{
    [Singleton]
    public partial class SingletonType1
    {
        private readonly string param;

        public SingletonType1(string param)
        {
            this.param = param;
        }
    }
}
