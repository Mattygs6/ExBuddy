namespace ExBuddy
{
	using System;
	using System.Threading.Tasks;

	using Buddy.Coroutines;

	using ff14bot;
	using ff14bot.Behavior;
	using ff14bot.NeoProfiles;

	using TreeSharp;

	public class ExCoroutineAction : TreeSharp.Action
	{
		private readonly ProfileBehavior behavior;
		private readonly Func<object, Coroutine> coroutineProducer;
		internal readonly Func<object, Task<bool>> TaskProducer;
		private Coroutine coroutine;

		public ExCoroutineAction(Func<object, Coroutine> coroutineProducer, ProfileBehavior behavior)
		{
			if (coroutineProducer == null)
			{
				throw new ArgumentNullException("coroutineProducer");
			}
				
			this.coroutineProducer = coroutineProducer;
			this.behavior = behavior;
		}

		public ExCoroutineAction(Func<object, Task<bool>> taskProducer, ProfileBehavior behavior)
			: this(obj => new Coroutine(() => taskProducer(obj)), behavior)
		{
			this.TaskProducer = taskProducer;
		}

		public ExCoroutineAction(Func<object, Task> taskProducer, ProfileBehavior behavior)
			: this(obj => new Coroutine(() => taskProducer(obj)), behavior)
		{
		}

		public ExCoroutineAction(Func<object, CoroutineTask<bool>> taskProducer, ProfileBehavior behavior)
			: this(obj => taskProducer(obj).Run(), behavior)
		{
		}

		public ExCoroutineAction(Func<object, CoroutineTask> taskProducer, ProfileBehavior behavior)
			: this(obj => taskProducer(obj).Run(), behavior)
		{
		}

		private void DisposeCoroutine()
		{
			if (this.coroutine == null)
			{
				return;
			}

			this.coroutine.Dispose();
			this.coroutine = null;
		}

		public override void Start(object context)
		{
			base.Start(context);

			this.DisposeCoroutine();
			this.coroutine = this.coroutineProducer(context);
		}

		public override void Stop(object context)
		{
			this.DisposeCoroutine();
			base.Stop(context);
		}

		protected override RunStatus Run(object context)
		{
			this.coroutine.Resume();

			TreeRoot.StatusText = behavior.StatusText;

			var status = this.coroutine.Status;

			switch (status)
			{
				case CoroutineStatus.Runnable:
					return RunStatus.Running;
				case CoroutineStatus.RanToCompletion:
					break;
				case CoroutineStatus.Stopped:
				case CoroutineStatus.Faulted:
					return RunStatus.Failure;
				default:
					throw new Exception("Unknown CoroutineStatus " + status);

			}

			if (this.coroutine.Result is bool && (!(bool)this.coroutine.Result))
			{
				return RunStatus.Failure;
			}

			return RunStatus.Success;
		}
	}
}
