namespace ExBuddy.OrderBotTags.Fish
{
	using System.ComponentModel;
	using System.Threading.Tasks;
	using Buddy.Coroutines;
	using Clio.Utilities;
	using Clio.XmlEngine;
	using ExBuddy.Helpers;
	using ff14bot;

	public interface IFishSpot
	{
		float Heading { get; set; }

		Vector3 Location { get; set; }

		bool Sit { get; set; }

		Task<bool> MoveFromLocation(ExFishTag tag);

		Task<bool> MoveToLocation(ExFishTag tag);
	}

	[XmlElement("FishSpot")]
	public class FishSpot : IFishSpot
	{
		public FishSpot()
		{
			Location = Vector3.Zero;
			Heading = 0f;
		}

		public FishSpot(string xyz, float heading)
		{
			Location = new Vector3(xyz);
			Heading = heading;
		}

		public FishSpot(Vector3 xyz, float heading)
		{
			Location = xyz;
			Heading = heading;
		}

		[DefaultValue(true)]
		[XmlAttribute("UseMesh")]
		public bool UseMesh { get; set; }

		public override string ToString()
		{
			return this.DynamicToString();
		}

		#region IFishSpot Members

		[XmlAttribute("Heading")]
		public float Heading { get; set; }

		[XmlAttribute("XYZ")]
		[XmlAttribute("Location")]
		public Vector3 Location { get; set; }

		[XmlAttribute("Sit")]
		public bool Sit { get; set; }

		public virtual async Task<bool> MoveFromLocation(ExFishTag tag)
		{
			return true;
		}

		public virtual async Task<bool> MoveToLocation(ExFishTag tag)
		{
			return true;
		}

		#endregion
	}

	public class StealthApproachFishSpot : FishSpot
	{
		[DefaultValue(true)]
		[XmlAttribute("ReturnToStealthLocation")]
		public bool ReturnToStealthLocation { get; set; }

		[XmlAttribute("StealthLocation")]
		public Vector3 StealthLocation { get; set; }

		[XmlAttribute("UnstealthAfter")]
		public bool UnstealthAfter { get; set; }

		public override async Task<bool> MoveFromLocation(ExFishTag tag)
		{
			tag.StatusText = "Moving from " + this;

			var result = true;
			if (ReturnToStealthLocation)
			{
				result &= await StealthLocation.MoveToNoMount(UseMesh, tag.Radius, "Stealth Location", tag.MovementStopCallback);
			}

			if (UnstealthAfter && Core.Player.HasAura((int) AbilityAura.Stealth))
			{
				result &= tag.DoAbility(ExFishTag.Abilities.Stealth); // TODO: move into abilities map?
			}

			return result;
		}

		public override async Task<bool> MoveToLocation(ExFishTag tag)
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
				if (!Core.Player.HasAura((int) AbilityAura.Stealth))
				{
					tag.DoAbility(ExFishTag.Abilities.Stealth);
				}

				result = await Location.MoveToNoMount(UseMesh, tag.Radius, tag.Name, tag.MovementStopCallback);
			}

			return result;
		}

		public override string ToString()
		{
			return this.DynamicToString("UnstealthAfter");
		}
	}

	public class IndirectApproachFishSpot : FishSpot {}
}