namespace ExBuddy.Interfaces
{
	using System.Threading.Tasks;
	using Clio.Utilities;
	using ExBuddy.OrderBotTags.Gather;

	public interface IGatherSpot
	{
		Vector3 NodeLocation { get; set; }

		Task<bool> MoveFromSpot(ExGatherTag tag);

		Task<bool> MoveToSpot(ExGatherTag tag);
	}
}