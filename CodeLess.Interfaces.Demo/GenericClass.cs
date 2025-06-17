namespace CodeLess.Interfaces.Demo
{
    [AutoInterface]
    public class GenericClass<T1, T2> : IGenericClass<T1, T2>
        where T1 : class
        where T2 : struct
    {
        public void Method(T1 arg1, T2 arg2)
        {
        }
    }
}
