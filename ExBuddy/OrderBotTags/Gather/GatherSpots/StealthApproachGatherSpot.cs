namespace ExBuddy.OrderBotTags.Gather.GatherSpots
{
	using System.ComponentModel;
	using System.Threading.Tasks;
	using Buddy.Coroutines;
	using Clio.Utilities;
	using Clio.XmlEngine;
	using ExBuddy.Helpers;
	using ff14bot;

	[XmlElement("StealthApproachGatherSpot")]
	public class StealthApproachGatherSpot : GatherSpot
	{
		[DefaultValue(true)]
		[XmlAttribute("ReturnToStealthLocation")]
		[XmlAttribute("ReturnToApproachLocation")]
		public bool ReturnToStealthLocation { get; set; }

		[XmlAttribute("StealthLocation")]
		public Vector3 StealthLocation { get; set; }

		[XmlAttribute("UnstealthAfter")]
		public bool UnstealthAfter { get; set; }

		public override async Task<bool> MoveFromSpot(ExGatherTag tag)
		{
			tag.StatusText = "Moving from " + this;

			var result = true;
			if (ReturnToStealthLocation)
			{
				result &= await StealthLocation.MoveToNoMount(UseMesh, tag.Radius, tag.Node.EnglishName, tag.MovementStopCallback);
			}

			if (UnstealthAfter && Core.Player.HasAura((int) AbilityAura.Stealth))
			{
				result &= await tag.CastAura(Ability.Stealth);
			}

			return result;
		}

		public override async Task<bool> MoveToSpot(ExGatherTag tag)
		{
			tag.StatusText = "Moving to " + this;

			if (StealthLocation == Vector3.Zero)
			{
				return false;
			}

			var result =
				await
					StealthLocation.MoveTo(
						UseMesh,
						radius: tag.Radius,
						name: "Stealth Location",
						stopCallback: tag.MovementStopCallback,
						dismountAtDestination: true);

			if (result)
			{
				await Coroutine.Yield();
				await tag.CastAura(Ability.Stealth, AbilityAura.Stealth);

				result = await NodeLocation.MoveToNoMount(UseMesh, tag.Distance, tag.Node.EnglishName, tag.MovementStopCallback);
			}

			return result;
		}

		public override string ToString()
		{
			return this.DynamicToString("UnstealthAfter");
		}
	}
}