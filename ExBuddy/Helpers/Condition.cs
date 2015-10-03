namespace ExBuddy.Helpers
{
	using System;
	using System.Collections.Concurrent;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using System.Threading;

	using Clio.Utilities;

	using ExBuddy.Logging;

	using ff14bot.Managers;

	public static class Condition
	{
		public static readonly TimeSpan OneDay = new TimeSpan(1, 0, 0, 0);

		internal static readonly ConcurrentDictionary<int, ConditionTimer> Timers =
			new ConcurrentDictionary<int, ConditionTimer>();

		static Condition()
		{
			AddNamespacesToScriptManager("ExBuddy", "ExBuddy.Helpers");
		}

		public static bool All(params object[] param)
		{
			if (param == null || param.Length == 0)
			{
				return false;
			}

			return param.All(IsTrue);
		}

		public static bool Any(params object[] param)
		{
			if (param == null || param.Length == 0)
			{
				return false;
			}

			return param.Any(IsTrue);
		}

		public static float Distance2D(float x, float y, float z)
		{
			return GameObjectManager.LocalPlayer.Location.Distance2D(new Vector3(x, y, z));
		}

		public static float Distance3D(float x, float y, float z)
		{
			return GameObjectManager.LocalPlayer.Location.Distance3D(new Vector3(x, y, z));
		}

		// Is overnight between =)
		public static bool IsTimeBetween(double start, double end)
		{
			if (Math.Abs(start - end) < double.Epsilon)
			{
				return false;
			}

			start = start.Clamp(0, 24);
			end = end.Clamp(0, 24);

			var eorzea = WorldManager.EorzaTime.TimeOfDay;
			var startTimeOffset = TimeSpan.FromHours(start);
			var endTimeOffset = TimeSpan.FromHours(end);

			if (start > end)
			{
				return eorzea.InRange(startTimeOffset, OneDay) || eorzea.InRange(TimeSpan.Zero, endTimeOffset);
			}

			return eorzea.InRange(startTimeOffset, endTimeOffset);
		}

		public static bool IsTrue(this object value)
		{
			var result = string.Concat(value).ConvertToBoolean().GetValueOrDefault();
			return result;
		}

		public static bool TrueFor(int id, TimeSpan span)
		{
			ConditionTimer timer;
			if (Timers.TryGetValue(id, out timer))
			{
				if (timer.TimeSpan != span)
				{
					Timers[id] = new ConditionTimer(id, span);
					timer.Timer.Dispose();
					return true;
				}

				return timer.IsValid;
			}

			Timers[id] = new ConditionTimer(id, span);

			return true;
		}

		internal static void AddNamespacesToScriptManager(params string[] param)
		{
			var field =
				typeof(ScriptManager).GetFields(BindingFlags.Static | BindingFlags.NonPublic)
					.FirstOrDefault(f => f.FieldType == typeof(List<string>));

			if (field == null)
			{
				Logger.Instance.Error("RebornBuddy update has moved or changed the type we are modifying, try updating ExBuddy or contact the author ExMatt.");
				return;
			}

			try
			{
				var list = field.GetValue(null) as List<string>;
				if (list == null)
				{
					return;
				}

				foreach (var ns in param)
				{
					if (!list.Contains(ns))
					{
						list.Add(ns);
						Logger.Instance.Info("Added namespace '{0}' to ScriptManager", ns);
					}
				}
			}
			catch
			{
				Logger.Instance.Error("Failed to add namespaces to ScriptManager, this can cause issues with some profiles.");
			}
		}
	}

	public class ConditionTimer : IDisposable
	{
		private bool disposed;

		private bool isValid = true;

		public ConditionTimer(int id, TimeSpan timeSpan)
		{
			Id = id;
			TimeSpan = timeSpan;
			Timer = new Timer(ToggleValid, this, timeSpan, TimeSpan.FromMilliseconds(-1));
		}

		public int Id { get; private set; }

		public bool IsValid
		{
			get
			{
				if (!isValid)
				{
					ConditionTimer timer;
					Condition.Timers.TryRemove(Id, out timer);
					Dispose();
				}

				return isValid;
			}
		}

		public Timer Timer { get; private set; }

		public TimeSpan TimeSpan { get; private set; }

		#region IDisposable Members

		public void Dispose()
		{
			if (!disposed)
			{
				disposed = true;
				if (Timer != null)
				{
					Timer.Dispose();
				}
			}
		}

		#endregion

		private static void ToggleValid(object context)
		{
			var _this = context as ConditionTimer;
			if (_this != null)
			{
				_this.isValid = !_this.isValid;
				_this.Timer.Change(_this.TimeSpan, TimeSpan.FromMilliseconds(-1));
			}
		}
	}
}