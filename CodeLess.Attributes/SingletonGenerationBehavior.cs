using System;

namespace CodeLess.Attributes
{
    [Flags]
    public enum SingletonGenerationBehavior
    {
        DEFAULT = 0,

        ALLOW_IMPLICIT_CONSTRUCTION = 1 << 0,

        GENERATE_STATIC_ACCESSORS = 1 << 1,
    }
}
