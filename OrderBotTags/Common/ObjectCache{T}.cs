
namespace ExBuddy.OrderBotTags.Common
{
    using System;
    using System.Collections.Generic;

    public class ObjectCache<T>
    {
        #region Static members
        // ReSharper disable once InconsistentNaming
        private static readonly Lazy<ObjectCache<T>> _Instance = new Lazy<ObjectCache<T>>(() => new ObjectCache<T>(), true);

        /// <summary>
        /// Gets the singleton instance of the <see cref="ObjectCache{T}"/> class
        /// </summary>
        public static ObjectCache<T> Instance
        {
            get { return _Instance.Value; }
        }
        #endregion

        private readonly Lazy<Dictionary<string, T>> cache = new Lazy<Dictionary<string, T>>(() => new Dictionary<string, T>(), true);

        /// <summary>
        /// Get or set a value in the cache
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <remarks>The key will be set with the type default if trying to get the value before it was set</remarks>
        public T this[string key]
        {
            get { return this.cache.Value.ContainsKey(key) ? this.cache.Value[key] : default(T); }
            set
            {
                if (!this.cache.Value.ContainsKey(key))
                {
                    this.cache.Value.Add(key, value);
                    return;
                }

                this.cache.Value[key] = value;
            }
        }

        /// <summary>
        /// Clears all values from the cache
        /// </summary>
        public void Clear()
        {
            this.cache.Value.Clear();
        }
    }
}
