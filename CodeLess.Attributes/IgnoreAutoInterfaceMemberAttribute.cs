using System;

namespace CodeLess.Interfaces
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Event)]
    public class IgnoreAutoInterfaceMemberAttribute : Attribute
    {

    }
}
