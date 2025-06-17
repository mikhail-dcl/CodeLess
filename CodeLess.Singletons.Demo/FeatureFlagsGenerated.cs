using CodeLess.Attributes;
using System;

namespace CodeLess.Singletons.Demo
{
    [Singleton(SingletonGenerationBehavior.ALLOW_IMPLICIT_CONSTRUCTION | SingletonGenerationBehavior.GENERATE_STATIC_ACCESSORS)]
    public partial class FeatureFlagsGenerated
    {
        internal event Action<float> eventToExpose;

        /// <summary>
        /// Property comment
        /// </summary>
        internal string propertyToExpose { get; set; } = "default value";

        /// <summary>
        /// Comment 1
        /// </summary>
        internal void methodToExpose(int args) {}

        /// <summary>
        /// Comment 2
        /// </summary>
        internal void methodToExpose() {}

        /// <summary>
        /// Comment 3
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        internal string methodToExpose(string arg) =>
            arg;
    }
}
