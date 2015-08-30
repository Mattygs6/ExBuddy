namespace ExBuddy.OrderBotTags
{
    using System;
    using System.Linq;
    using System.Reflection;

    public static partial class Extensions
    {
        /// <summary>
        /// Creates the instance.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <param name="args">
        /// The args.
        /// </param>
        /// <returns>
        /// The new instance of specified type
        /// </returns>
        public static object CreateInstance(this Type type, params object[] args)
        {
            if (type != null)
            {
                if (args != null && args.Length > 0)
                {
                    return Activator.CreateInstance(
                        type,
                        BindingFlags.CreateInstance | BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic,
                        null,
                        args,
                        null,
                        null);
                }

                return Activator.CreateInstance(type);
            }

            return null;
        }

        /// <summary>
        /// Creates the instance.
        /// </summary>
        /// <typeparam name="T">
        /// The type
        /// </typeparam>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <param name="args">
        /// The args.
        /// </param>
        /// <returns>
        /// The new instance of specified type
        /// </returns>
        public static T CreateInstance<T>(this Type type, params object[] args) where T : class
        {
            return type.CreateInstance(args) as T;
        }

        /// <summary>
        /// The get custom attribute property value.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <param name="func">
        /// The func.
        /// </param>
        /// <param name="defaultValue">
        /// The defaultValue if attribute doesn't exist
        /// </param>
        /// <typeparam name="TAttribute">
        /// The Attribute type
        /// </typeparam>
        /// <typeparam name="TResult">
        /// The Result type
        /// </typeparam>
        /// <returns>
        /// The <see cref="TResult"/>.
        /// </returns>
        public static TResult GetCustomAttributePropertyValue<TAttribute, TResult>(
            this Type type,
            Func<TAttribute, TResult> func, TResult defaultValue = default(TResult)) where TAttribute : Attribute
        {
            var attr = ReflectionHelper.CustomAttributes<TAttribute>.NotInherited[type.GUID].FirstOrDefault();

            if (attr != null)
            {
                return func(attr);
            }

            return defaultValue;
        }
    }
}
