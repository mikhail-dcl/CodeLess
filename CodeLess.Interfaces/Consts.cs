namespace CodeLess.Interfaces
{
    public static class Consts
    {
        public const string ATTRIBUTE_NAME = nameof(AutoInterfaceAttribute);

        public static readonly string ATTRIBUTE_FULLY_QUALIFIED_NAME = typeof(AutoInterfaceAttribute).FullName!;

        public const string IGNORE_MEMBER_ATTRIBUTE_NAME = nameof(IgnoreAutoInterfaceMemberAttribute);
    }
}
