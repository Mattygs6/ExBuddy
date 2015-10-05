namespace ExBuddy.OrderBotTags.Behaviors
{
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows.Media;

	using Clio.Utilities;
	using Clio.XmlEngine;

	using ExBuddy.Attributes;
	using ExBuddy.Enumerations;
	using ExBuddy.Helpers;

	using ff14bot.Managers;
	using ff14bot.Navigation;

	[LoggerName("ExMoveTo")]
	[XmlElement("ExMoveTo")]
	public sealed class ExMoveToTag : ExProfileBehavior
	{
		private ushort startZoneId;

		private HotSpot destination;

		[DefaultValue(3.0f)]
		[XmlAttribute("Distance")]
		public float Distance { get; set; }

		public bool UseMesh { get; set; }

		[XmlAttribute("XYZ")]
		[XmlAttribute("Location")]
		public Vector3 Location { get; set; }

		[XmlElement("HotSpots")]
		public List<HotSpot> HotSpots { get; set; }

		[DefaultValue(MoveToType.Auto)]
		[XmlAttribute("Type")]
		public MoveToType Type { get; set; }

		protected override Color Info
		{
			get
			{
				return Colors.Aqua;
			}
		}

		protected override void DoReset()
		{
			startZoneId = 0;
		}

		protected override async Task<bool> Main()
		{
			if (startZoneId != WorldManager.ZoneId)
			{
				return isDone = true;
			}

			if (Me.Distance(Location) <= Distance)
			{
				return isDone = true;
			}

			if (HotSpots != null)
			{
				if (Type == MoveToType.Auto)
				{
					Type = MoveToType.RandomPointWithin;
				}

				var locations = new List<HotSpot>(HotSpots) { new HotSpot(Location, Distance) { Name = Name } };
				destination = locations.Shuffle().First();

				Logger.Info("Using random location -> {0}", Location);
			}
			else
			{
				if (Type == MoveToType.Auto)
				{
					Type = MoveToType.StopWithinRange;
				}

				destination = new HotSpot(Location, Distance) { Name = Name };
			}

			switch (Type)
			{
				case MoveToType.StopWithinRange:
					await destination.MoveTo(UseMesh);
					break;
				case MoveToType.RandomPointWithin:
					await destination.MoveToPointWithin();
					break;
			}

			return isDone = true;
		}

		protected override void OnStart()
		{
			startZoneId = WorldManager.ZoneId;
		}

		protected override void OnDone()
		{
			Navigator.PlayerMover.MoveStop();
		}
	}
}
