namespace ExBuddy.OrderBotTags.Gather
{
	using System.Threading.Tasks;

	using Clio.XmlEngine;

	using ExBuddy.Helpers;

	////[System.Xml.Serialization.XmlInclude(typeof(StealthGatherSpot))]
	////[System.Xml.Serialization.XmlInclude(typeof(StealthApproachGatherSpot))]
	[XmlElement("GatherSpot")]
	public class GatherSpot : StealthGatherSpot
	{
		public override async Task<bool> MoveToSpot(ExGatherTag tag)
		{
			tag.StatusText = "Moving to " + this;

			var result =
				await
				Behaviors.MoveTo(
					NodeLocation,
					UseMesh,
					radius: tag.Distance,
					name: tag.Node.EnglishName,
					stopCallback: tag.MovementStopCallback);

			return result;
		}
	}
}