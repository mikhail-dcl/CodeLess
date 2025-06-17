using System;

namespace CodeLess.Interfaces.Demo
{
    [AutoInterface]
    public abstract class NonGenericClass : INonGenericClass
    {
        /// <summary>
        /// Comment for SomethingHappened event
        /// </summary>
        public event Action<bool>? OnSomethingHappened;

        public event EventHandler Custom
        {
            add { }
            remove { }
        }

        public event Action Evt
        {
            add { }
            remove { }
        }

        public int this[int index]
        {
            get => 0;
            set
            {
            }
        }

        public string OpenGetterSetter { get; set; }

        public int OpenGetterOnly { get; }

        public abstract string AbstractGetter { get; }

        public abstract void Foo();
        public abstract int Bar(string input);

        /// <summary>
        /// Comment for PropertyWithBody
        /// </summary>
        public int PropertyWithBody
        {
            get
            {
                return 1;
            }

            // Trivia
            set => value += 5;
        }

        internal float InternalProperty { get; set; }

        float INonGenericClass.InternalProperty
        {
            get => InternalProperty;

            set => InternalProperty = value;
        }

        public string this[string s]
        {
            get => s.ToUpper();
            set => CloseGetterSetter = value;
        }

        /// <summary>
        /// Comment for CloseGetterSetter
        /// </summary>
        private string CloseGetterSetter { get; set; }

        public string GenericMethodWithMultipleParameters<T, U>(T a, U b)
        {
            return $"{a} - {b}";
        }

        public int GenericMethodWithConstraints<T>(T a, T b) where T : class, new()
        {
            return a.ToString().Length + b.ToString().Length;
        }

        public string GenericMethod<T>(T a, T b)
        {
            return $"{a} - {b}";
        }

        /// <summary>
        /// Comment
        /// </summary>
        public void Method1()
        {

        }

        /// <summary>
        /// Comment for Method2
        /// </summary>
        /// <param name="a">rrr</param>
        /// <param name="b">bbb fff</param>
        /// <returns></returns>
        public string Method2(int a, string b)
        {
            return $"{a} - {b}";
        }

        public int Method3(int a, string b) => a + b.Length;

        public float Method4(float a, float b) => a * b;
    }
}
