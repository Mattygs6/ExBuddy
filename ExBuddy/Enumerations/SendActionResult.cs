namespace ExBuddy.Enumerations
{
	public enum SendActionResult : byte
	{
		None = 0,

		InjectionError = 1,

		UnexpectedResult = 2,

		InvalidWindow = 3,

		Success = 4
	}
}