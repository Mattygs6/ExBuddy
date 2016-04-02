namespace ExBuddy.Plugins.Skywatcher
{
	using System;
	using System.Net.Http;
	using System.Threading.Tasks;
	using ExBuddy.Logging;
	using Newtonsoft.Json;

	public static class HttpClientExtensions
	{
		/// <summary>
		///     Gets the content async.
		/// </summary>
		/// <typeparam name="T">The type</typeparam>
		/// <param name="client">The client.</param>
		/// <param name="requestUrl">The request URL.</param>
		/// <param name="deserializer">The deserializer.</param>
		/// <returns>
		///     The Task to get the content
		/// </returns>
		public static Task<T> GetContentAsync<T>(
			this HttpClient client,
			string requestUrl,
			Func<string, T> deserializer = null) where T : class
		{
			var tcs = new TaskCompletionSource<T>();
			client.GetAsync(requestUrl).ContinueWith(
				requestTask =>
				{
					// ReSharper disable once ConditionIsAlwaysTrueOrFalse
					if (client != null)
					{
						client.Dispose();
					}

					if (!HandleRequestFaultsAndCancelation(requestTask, tcs))
					{
						var result = requestTask.Result;
						if (result.Content == null)
						{
							tcs.TrySetResult(default(T));
						}
						else
						{
							try
							{
								Action<Task<string>> continuation = contentTask =>
								{
									if (!HandleFaultsAndCancelation(contentTask, tcs))
									{
										if (deserializer == null)
										{
											deserializer = JsonConvert.DeserializeObject<T>;
										}

										var deserializedResult = deserializer(contentTask.Result);

										tcs.TrySetResult(deserializedResult);
									}
								};

								result.Content.ReadAsStringAsync().ContinueWith(continuation);
							}
							catch (HttpRequestException reqex)
							{
								tcs.TrySetException(reqex);
							}
							catch (Exception exception)
							{
								Logger.Instance.Error(exception.Message);
								tcs.TrySetException(exception);
							}
						}
					}
				});

			return tcs.Task;
		}

		/// <summary>
		///     Handles the faults and cancelation.
		/// </summary>
		/// <typeparam name="T">The type</typeparam>
		/// <param name="task">The task.</param>
		/// <param name="tcs">The TCS.</param>
		/// <returns>False if no faults and not cancelled, otherwise true</returns>
		private static bool HandleFaultsAndCancelation<T>(Task task, TaskCompletionSource<T> tcs)
		{
			if (task.IsFaulted && task.Exception != null)
			{
				tcs.TrySetException(task.Exception.GetBaseException());
				return true;
			}

			if (task.IsCanceled)
			{
				tcs.TrySetCanceled();
				return true;
			}

			return false;
		}

		/// <summary>
		///     Handles the request faults and cancelation.
		/// </summary>
		/// <typeparam name="T">The type</typeparam>
		/// <param name="task">The task.</param>
		/// <param name="tcs">The TCS.</param>
		/// <returns>False if successful request, otherwise true</returns>
		private static bool HandleRequestFaultsAndCancelation<T>(Task<HttpResponseMessage> task, TaskCompletionSource<T> tcs)
		{
			if (!HandleFaultsAndCancelation(task, tcs))
			{
				var result = task.Result;
				try
				{
					result.EnsureSuccessStatusCode();
					return false;
				}
				catch (HttpRequestException reqex)
				{
					tcs.TrySetException(reqex);
				}
				catch (Exception exception)
				{
					Logger.Instance.Error(exception.Message);
					tcs.TrySetException(exception);
				}
			}

			return true;
		}
	}
}