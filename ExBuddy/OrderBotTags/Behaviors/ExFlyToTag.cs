namespace ExBuddy.OrderBotTags.Behaviors
{
	using System.ComponentModel;
	using System.Diagnostics;
	using System.Threading.Tasks;
	using System.Windows.Media;

	using Buddy.Coroutines;

	using Clio.Utilities;
	using Clio.XmlEngine;

	using ExBuddy.Attributes;
	using ExBuddy.Helpers;
	using ExBuddy.Interfaces;
	using ExBuddy.Navigation;

	using ff14bot.Behavior;
	using ff14bot.Interfaces;
	using ff14bot.Managers;
	using ff14bot.Navigation;

	[LoggerName("ExFlyTo")]
	[XmlElement("ExFlyTo")]
	[XmlElement("FlightPathTo")]
	public class ExFlyToTag : ExProfileBehavior, IFlightVars
	{
		private readonly Stopwatch landingStopwatch = new Stopwatch();

		private readonly IPlayerMover playerMover = new SlideMover();

		protected override Color Info
		{
			get
			{
				return Colors.DeepSkyBlue;
			}
		}

		[XmlAttribute("XYZ")]
		public Vector3 Target { get; set; }

		[DefaultValue(3.0f)]
		[XmlAttribute("Radius")]
		public float Radius { get; set; }

		[DefaultValue(6)]
		[XmlAttribute("InverseParabolicMagnitude")]
		public int InverseParabolicMagnitude { get; set; }

		[DefaultValue(0.05f)]
		[XmlAttribute("Smoothing")]
		public float Smoothing { get; set; }

		[DefaultValue(0)]
		[XmlAttribute("MountId")]
		public int MountId { get; set; }

		[DefaultValue(6.0f)]
		[XmlAttribute("ForcedAltitude")]
		public float ForcedAltitude { get; set; }

		[XmlAttribute("ForceLanding")]
		public bool ForceLanding { get; set; }

		protected override async Task<bool> Main()
		{
			FlightPath flightPath = new StraightOrParabolicFlightPath(Me.Location, Target, this);

			var distance = flightPath.Distance;

			if (distance < Radius)
			{
				Logger.Info("Already in range -> Start: {0}, End: {1}", flightPath.Start, flightPath.End);
				isDone = true;
				return true;
			}

			StatusText = "Generating Path";

			FlightPath path;
			if (FlightPath.Paths.TryGetValue(flightPath.Key, out path))
			{
				flightPath = path;
				Logger.Info("Using existing FlightPath {0} from {1} to {2}", flightPath.Key, flightPath.Start, flightPath.End);
			}
			else
			{
				Logger.Info("Building new FlightPath {0} from {1} to {2}", flightPath.Key, flightPath.Start, flightPath.End);

				if (await flightPath.BuildPath())
				{
					FlightPath.Paths[flightPath.Key] = flightPath;
				}
			}

			if (flightPath.Count > 0)
			{
				StatusText = "Target: " + Target;
				do
				{
					if (flightPath.Current.IsDeviation)
					{
						Logger.Verbose("Deviating from course to waypoint: {0}", flightPath.Current);
					}
					else
					{
						Logger.Verbose("Moving to waypoint: {0}", flightPath.Current);
					}

					await MoveToWithinRadius(flightPath.Current, Radius);
				}
				while (flightPath.Next());

				flightPath.Reset();
			}
			else
			{
				Logger.Error("No viable path computed for {0}.", Target);
			}

			if (ForceLanding)
			{
				await ForceLand();
			}

			isDone = true;
			return true;
		}

		public async Task<bool> EnsureMounted()
		{
			while (!Me.IsMounted && Behaviors.ShouldContinue)
			{
				if (MountId > 0)
				{
					if (!await CommonTasks.MountUp((uint)MountId))
					{
						await CommonTasks.MountUp();
					}
				}
				else
				{
					await CommonTasks.MountUp();
				}

				await Coroutine.Yield();
			}
			return true;
		}

		public async Task<bool> EnsureFlying()
		{
			await EnsureMounted();
			if (!ff14bot.Managers.MovementManager.IsFlying)
			{
				return await CommonTasks.TakeOff();
			}
			return true;
		}

		public async Task<bool> MoveToWithinRadius(Vector3 to, float radius)
		{
			while (Me.Location.Distance3D(to) > Radius && Behaviors.ShouldContinue)
			{
				await EnsureFlying();

				playerMover.MoveTowards(to);
				await Coroutine.Yield();
			}
			playerMover.MoveStop();
			return true;
		}

		public async Task<bool> ForceLand()
		{
			StatusText = "Landing";
			landingStopwatch.Restart();
			while (MovementManager.IsFlying && Behaviors.ShouldContinue)
			{
				MovementManager.StartDescending();

				if (landingStopwatch.ElapsedMilliseconds > 2000 && MovementManager.IsFlying)
				{
					var move = Me.Location.AddRandomDirection2D(10).GetFloor(15);
					await Behaviors.MoveToNoMount(move, false, 0.5f);
					landingStopwatch.Restart();
				}

				await Coroutine.Yield();
			}

			Logger.Info("Landing took {0} ms", landingStopwatch.Elapsed);

			landingStopwatch.Reset();

			return true;
		}
	}
}