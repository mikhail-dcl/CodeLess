using CodeLess.Attributes;
using System;

namespace CodeLess.Singletons
{
    internal static class Consts
    {
        public const string ATTRIBUTE_NAME = nameof(SingletonAttribute);

        public static readonly string ATTRIBUTE_FULLY_QUALIFIED_NAME =
            $"{typeof(SingletonAttribute).FullName}";

        public static readonly string DISPOSE_FULLY_QUALIFIED_NAME =
            $"{typeof(IDisposable).FullName}";
    }
}
