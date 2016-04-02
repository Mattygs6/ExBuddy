namespace ExBuddy.OrderBotTags.Gather.GatherSpots
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
		private HotSpot approachLocation;

		[XmlElement("HotSpots")]
		public List<HotSpot> HotSpots { get; set; }

		[XmlAttribute("ReturnToApproachLocation")]
		public bool ReturnToApproachLocation { get; set; }

		[XmlAttribute("Stealth")]
		public bool Stealth { get; set; }

		[XmlAttribute("UnstealthAfter")]
		public bool UnstealthAfter { get; set; }

		public override async Task<bool> MoveFromSpot(ExGatherTag tag)
		{
			tag.StatusText = "Moving from " + this;

			var result = true;
			if (ReturnToApproachLocation)
			{
				result &= await approachLocation.MoveToNoMount(UseMesh, tag.MovementStopCallback);
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

			if (approachLocation == Vector3.Zero)
			{
				if (HotSpots == null || HotSpots.Count == 0)
				{
					return false;
				}

				approachLocation = HotSpots.Shuffle().First();
			}

			var result = await approachLocation.MoveToPointWithin(dismountAtDestination: Stealth);

			if (result)
			{
				await Coroutine.Yield();

				if (Stealth)
				{
					await tag.CastAura(Ability.Stealth, AbilityAura.Stealth);
					result = await NodeLocation.MoveToNoMount(UseMesh, tag.Distance, tag.Node.EnglishName, tag.MovementStopCallback);
				}
				else
				{
					result =
						await
							NodeLocation.MoveTo(
								UseMesh,
								radius: tag.Distance,
								name: tag.Node.EnglishName,
								stopCallback: tag.MovementStopCallback);
				}
			}

			return result;
		}

		public override string ToString()
		{
			return this.DynamicToString("HotSpots", "Stealth", "UnstealthAfter");
		}
	}
}