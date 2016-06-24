namespace ExBuddy.Interfaces
{
	public interface IConditionNamedItem : INamedItem
    {
		string Condition { get; set; }

        bool ConditionResult { get; }

        int ItemCount { get; set; }

        int HqItemCount { get; set; }

        int NqItemCount { get; set; }
	}
}