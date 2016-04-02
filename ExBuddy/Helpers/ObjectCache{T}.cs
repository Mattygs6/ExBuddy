namespace ExBuddy.Helpers
{
	using System;
	using System.Collections.Generic;

	public class ObjectCache<T>
	{
		private readonly Lazy<Dictionary<string, T>> cache = new Lazy<Dictionary<string, T>>(
			() => new Dictionary<string, T>(),
			true);

		/// <summary>
		///     Get or set a value in the cache
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		/// <remarks>The key will be set with the type default if trying to get the value before it was set</remarks>
		public T this[string key]
		{
			get { return cache.Value.ContainsKey(key) ? cache.Value[key] : default(T); }
			set
			{
				if (!cache.Value.ContainsKey(key))
				{
					cache.Value.Add(key, value);
					return;
				}

				cache.Value[key] = value;
			}
		}

		/// <summary>
		///     Clears all values from the cache
		/// </summary>
		public void Clear()
		{
			cache.Value.Clear();
		}

		#region Static members

		// ReSharper disable once InconsistentNaming
		private static readonly Lazy<ObjectCache<T>> _Instance = new Lazy<ObjectCache<T>>(() => new ObjectCache<T>(), true);

		/// <summary>
		///     Gets the singleton instance of the <see cref="ObjectCache{T}" /> class
		/// </summary>
		public static ObjectCache<T> Instance
		{
			get { return _Instance.Value; }
		}

		#endregion
	}
}