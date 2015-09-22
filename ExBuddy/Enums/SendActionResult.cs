namespace ExBuddy.Enums
{
    public enum SendActionResult : byte
    {
        None = 0,
        InjectionError = 1,
        InvalidWindow = 2,
        UnexpectedResult = 3,
        Success = 4
    }
}