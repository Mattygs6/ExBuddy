namespace ExBuddy.Interfaces
{
	using System.Threading.Tasks;
	using Clio.Utilities;

	public interface IReturnStrategy : ITeleportLocation
	{
		Vector3 InitialLocation { get; set; }

		Task<bool> ReturnToLocation();

		Task<bool> ReturnToZone();
	}
}