using System;

namespace CodeLess.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class SingletonAttribute(SingletonGenerationBehavior behavior = SingletonGenerationBehavior.DEFAULT) : Attribute
    {
        public readonly SingletonGenerationBehavior Behavior = behavior;
    }
}
