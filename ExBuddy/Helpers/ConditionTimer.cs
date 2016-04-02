namespace ExBuddy.Helpers
{
	using System;
	using System.Threading;

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

		public int Id { get; set; }

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

		public Timer Timer { get; set; }

		public TimeSpan TimeSpan { get; set; }

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