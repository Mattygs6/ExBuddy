namespace ExBuddy.OrderBotTags.Gather.GatherSpots
{
	using System.ComponentModel;
	using System.Threading.Tasks;
	using Buddy.Coroutines;
	using Clio.Utilities;
	using Clio.XmlEngine;
	using ExBuddy.Helpers;

	[XmlElement("IndirectApproachGatherSpot")]
	public class IndirectApproachGatherSpot : GatherSpot
	{
		[XmlAttribute("ApproachLocation")]
		public Vector3 ApproachLocation { get; set; }

		[DefaultValue(true)]
		[XmlAttribute("ReturnToApproachLocation")]
		public bool ReturnToApproachLocation { get; set; }

		public override async Task<bool> MoveFromSpot(ExGatherTag tag)
		{
			tag.StatusText = "Moving from " + this;

			var result = true;
			if (ReturnToApproachLocation)
			{
				result &= await ApproachLocation.MoveToNoMount(UseMesh, tag.Radius, tag.Node.EnglishName, tag.MovementStopCallback);
			}

			return result;
		}

		public override async Task<bool> MoveToSpot(ExGatherTag tag)
		{
			tag.StatusText = "Moving to " + this;

			if (ApproachLocation == Vector3.Zero)
			{
				return false;
			}

			var result =
				await
					ApproachLocation.MoveTo(
						UseMesh,
						radius: tag.Radius,
						name: "Approach Location",
						stopCallback: tag.MovementStopCallback);

			if (result)
			{
				await Coroutine.Yield();
				result = await NodeLocation.MoveToNoMount(UseMesh, tag.Distance, tag.Node.EnglishName, tag.MovementStopCallback);
			}

			return result;
		}
	}
}