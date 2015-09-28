namespace ExBuddy.OrderBotTags.Gather
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;

	using Buddy.Coroutines;

	using Clio.Utilities;
	using Clio.XmlEngine;

	using ExBuddy.Helpers;

	using ff14bot;
	using ff14bot.Navigation;

	[XmlElement("RandomApproachGatherSpot")]
	public class RandomApproachGatherSpot : GatherSpot
	{
		public HotSpot ApproachLocation { get; protected set; }

		[XmlAttribute("Stealth")]
		public bool Stealth { get; set; }

		[XmlAttribute("ReturnToApproachLocation")]
		public bool ReturnToApproachLocation { get; set; }

		[XmlAttribute("UnstealthAfter")]
		public bool UnstealthAfter { get; set; }

		[XmlElement("HotSpots")]
		public List<HotSpot> HotSpots { get; set; }

		public override async Task<bool> MoveFromSpot(ExGatherTag tag)
		{
			tag.StatusText = "Moving from " + this;

			var result = true;
			if (ReturnToApproachLocation)
			{
				result &=
					await Behaviors.MoveToNoMount(ApproachLocation, UseMesh, tag.Radius, tag.Node.EnglishName, tag.MovementStopCallback);
			}

			if (UnstealthAfter && Core.Player.HasAura((int)AbilityAura.Stealth))
			{
				result &= await tag.CastAura(Ability.Stealth);
			}

			return result;
		}

		public override async Task<bool> MoveToSpot(ExGatherTag tag)
		{
			tag.StatusText = "Moving to " + this;

			if (ApproachLocation == Vector3.Zero)
			{
				if(HotSpots == null || HotSpots.Count == 0)
				{
					return false;
				}

				ApproachLocation = HotSpots.Shuffle().First();
			}

			var result =
				await
				Behaviors.MoveTo(
					ApproachLocation,
					UseMesh,
					radius: ApproachLocation.Radius, // TODO: make new method for MoveToSpotWithin
					name: "Approach Location",
					stopCallback: tag.MovementStopCallback,
					dismountAtDestination: Stealth);

			if (result)
			{
				await Coroutine.Yield();

				if (Stealth)
				{
					await tag.CastAura(Ability.Stealth, AbilityAura.Stealth);
					result =
						await Behaviors.MoveToNoMount(NodeLocation, UseMesh, tag.Distance, tag.Node.EnglishName, tag.MovementStopCallback);
				}
				else
				{
					result =
						await Behaviors.MoveTo(NodeLocation, UseMesh, radius: tag.Distance, name: tag.Node.EnglishName, stopCallback: tag.MovementStopCallback);
				}
			}

			return result;
		}

		public override string ToString()
		{
			return
				string.Format(
					"RandomApproachGatherSpot -> ApproachLocation: {0}, NodeLocation: {1}, ReturnToApproachLocation: {2}, UseMesh: {3}",
					ApproachLocation,
					NodeLocation,
					ReturnToApproachLocation,
					UseMesh);
		}
	}
}
