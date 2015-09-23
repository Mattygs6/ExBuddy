namespace ExBuddy.OrderBotTags.Gather
{
	using System.ComponentModel;
	using System.Threading.Tasks;

	using Buddy.Coroutines;

	using Clio.Utilities;
	using Clio.XmlEngine;

	using ExBuddy.Helpers;
	using ExBuddy.Interfaces;

	using ff14bot;

	[XmlElement("StealthApproachGatherSpot")]
	public class StealthApproachGatherSpot : IGatherSpot
	{
		[XmlAttribute("NodeLocation")]
		public Vector3 NodeLocation { get; set; }

		[XmlAttribute("StealthLocation")]
		public Vector3 StealthLocation { get; set; }

		[DefaultValue(true)]
		[XmlAttribute("ReturnToStealthLocation")]
		public bool ReturnToStealthLocation { get; set; }

		[DefaultValue(true)]
		[XmlAttribute("UseMesh")]
		public bool UseMesh { get; set; }

		[XmlAttribute("UnstealthAfter")]
		public bool UnstealthAfter { get; set; }

		public async Task<bool> MoveFromSpot(GatherCollectableTag tag)
		{
			tag.StatusText = "Moving from " + this;

			var result = true;
			if (ReturnToStealthLocation)
			{
				result &=
					await Behaviors.MoveToNoMount(StealthLocation, UseMesh, tag.Radius, tag.Node.EnglishName, tag.MovementStopCallback);
			}

			if (UnstealthAfter && Core.Player.HasAura((int)AbilityAura.Stealth))
			{
				result &= await tag.CastAura(Ability.Stealth);
			}

			return result;
		}

		public async Task<bool> MoveToSpot(GatherCollectableTag tag)
		{
			tag.StatusText = "Moving to " + this;

			if (StealthLocation == Vector3.Zero)
			{
				return false;
			}

			var result =
				await
				Behaviors.MoveTo(
					StealthLocation,
					UseMesh,
					radius: tag.Radius,
					name: "Stealth Location",
					stopCallback: tag.MovementStopCallback,
					dismountAtDestination: true);

			if (result)
			{
				await Coroutine.Yield();
				await tag.CastAura(Ability.Stealth, AbilityAura.Stealth);

				result =
					await Behaviors.MoveToNoMount(NodeLocation, UseMesh, tag.Distance, tag.Node.EnglishName, tag.MovementStopCallback);
			}

			return result;
		}

		public override string ToString()
		{
			return
				string.Format(
					"StealthApproachGatherSpot -> StealthLocation: {0}, NodeLocation: {1}, ReturnToStealthLocation: {2}, UseMesh: {3}",
					StealthLocation,
					NodeLocation,
					ReturnToStealthLocation,
					UseMesh);
		}
	}
}