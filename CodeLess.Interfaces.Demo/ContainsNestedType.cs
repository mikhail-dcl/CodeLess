namespace CodeLess.Interfaces.Demo
{
    [AutoInterface]
    public class ContainsNestedType : IContainsNestedType
    {
        public delegate void UserIdOperation(string userId);

        public struct NestedType
        {
            public string UserId { get; set; }
        }

        public event UserIdOperation? OnOtherUserRemovedTheFriendship;

        public void Method(NestedType nested)
        {

        }

        public NestedType Return()
        {
            return new NestedType();
        }
    }
}
