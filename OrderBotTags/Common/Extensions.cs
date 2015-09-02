namespace ExBuddy.OrderBotTags
{
    using System;
    using System.Linq;
    using System.Reflection;

    using Clio.Utilities;

    using ff14bot.Managers;

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

        public static bool IsGround(this Vector3 vector)
        {
            Vector3 above = new Vector3(vector.X, vector.Y + 0.2f, vector.Z);
            Vector3 below = new Vector3(vector.X, vector.Y + 2.0f, vector.Z);
            Vector3 hit;
            Vector3 distances;
            if (WorldManager.Raycast(above, below, out hit, out distances) && hit != Vector3.Zero)
            {
                return true;
            }

            return false;
        }

        public static Vector3 GetFloor(this Vector3 vector, float maxDistanceToCheck = 100.0f)
        {
            Vector3 above = new Vector3(vector.X, vector.Y + 0.2f, vector.Z);
            Vector3 below = new Vector3(vector.X, vector.Y - maxDistanceToCheck, vector.Z);
            Vector3 hit;
            Vector3 distances;
            if (WorldManager.Raycast(above, below, out hit, out distances) && hit != Vector3.Zero)
            {
                return hit;
            }

            return Vector3.Zero;
        }

        public static Vector3 GetCeiling(this Vector3 vector, float maxDistanceToCheck = 100.0f)
        {
            Vector3 above = new Vector3(vector.X, vector.Y + maxDistanceToCheck, vector.Z);
            Vector3 below = new Vector3(vector.X, vector.Y - 0.2f, vector.Z);
            Vector3 hit;
            Vector3 distances;
            if (WorldManager.Raycast(below, above, out hit, out distances) && hit != Vector3.Zero)
            {
                return hit;
            }

            return Vector3.Zero;
        }
    }
}
