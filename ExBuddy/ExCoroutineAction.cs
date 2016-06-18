namespace ExBuddy
{
	using System;
	using System.Runtime.CompilerServices;
	using System.Threading.Tasks;
	using Buddy.Coroutines;
	using ff14bot;
	using ff14bot.Behavior;
	using ff14bot.NeoProfiles;
	using TreeSharp;
	using Action = TreeSharp.Action;

	public class ExCoroutineAction : Action
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
			: this(CreateCoroutineProducer(taskProducer), behavior)
		{
			TaskProducer = taskProducer;
		}

		public ExCoroutineAction(Func<object, Task> taskProducer, ProfileBehavior behavior)
			: this(obj => new Coroutine(() => taskProducer(obj)), behavior) {}

		public ExCoroutineAction(Func<object, CoroutineTask<bool>> taskProducer, ProfileBehavior behavior)
			: this(obj => taskProducer(obj).Run(), behavior) {}

		public ExCoroutineAction(Func<object, CoroutineTask> taskProducer, ProfileBehavior behavior)
			: this(obj => taskProducer(obj).Run(), behavior) {}

		public override void Start(object context)
		{
			base.Start(context);

			DisposeCoroutine();
			coroutine = coroutineProducer(context);
		}

		public override void Stop(object context)
		{
			DisposeCoroutine();
			base.Stop(context);
		}

		protected override RunStatus Run(object context)
		{
			coroutine.Resume();

			TreeRoot.StatusText = behavior.StatusText;

			var status = coroutine.Status;

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

			if (coroutine.Result is bool && (!(bool) coroutine.Result))
			{
				return RunStatus.Failure;
			}

			return RunStatus.Success;
		}

		private static Func<object, Coroutine> CreateCoroutineProducer(Func<object, Task<bool>> taskProducer)
		{
			return obj =>
			{
				return new Coroutine(
					() =>
					{
						var builder = AsyncTaskMethodBuilder<object>.Create();
						try
						{
							var task = taskProducer(obj);
							var awaiter = task.GetAwaiter();
							if (!awaiter.IsCompleted)
							{
								// Result wasn’t available. Add a continuation, and return the builder. 
								awaiter.OnCompleted(
									() =>
									{
										try
										{
											builder.SetResult(awaiter.GetResult());
										}
										catch (Exception e)
										{
											builder.SetException(e);
										}
									});

								return builder.Task;
							}

							// Result was already available: proceed synchronously 
							builder.SetResult(awaiter.GetResult());
						}
						catch (Exception e)
						{
							builder.SetException(e);
						}
						return builder.Task;
					});
			};
		}

		private void DisposeCoroutine()
		{
			if (coroutine == null)
			{
				return;
			}

			coroutine.Dispose();
			coroutine = null;
		}
	}
}