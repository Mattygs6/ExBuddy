namespace ExBuddy.OrderBotTags
{
    using System;
    using System.Data.SqlTypes;
    using System.Linq;
    using System.Reflection;
    using System.Windows.Media;

    using Clio.Common;
    using Clio.Utilities;

    using ff14bot;
    using ff14bot.Helpers;
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

        public static bool IsUnknownChance(this GatheringItem gatheringItem)
        {
            if (gatheringItem.IsUnknown || gatheringItem.Chance == 25)
            {
                return true;
            }

            var lastSpellId = Actionmanager.LastSpellId;
            if (gatheringItem.Chance == 30 && lastSpellId == Abilities.Map[Core.Player.CurrentJob][Ability.IncreaseGatherChance5])
            {
                return true;
            }

            if (gatheringItem.Chance == 40 && lastSpellId == Abilities.Map[Core.Player.CurrentJob][Ability.IncreaseGatherChance15])
            {
                return true;
            }

            if (gatheringItem.Chance == 75 && lastSpellId == Abilities.Map[Core.Player.CurrentJob][Ability.IncreaseGatherChance50])
            {
                return true;
            }

            return false;
        }

        public static bool TryGatherItem(this GatheringItem gatheringItem)
        {
            try
            {
                return gatheringItem.GatherItem();
            }
            catch (NullReferenceException)
            {
                Logging.WriteDiagnostic(
                    Colors.PaleVioletRed,
                    "GatherItem became null between resolving it and gathering it due to the Gathering Window closing, moving on.");

                return false;
            }
        }

        public static Vector3 CorrectLanding(this Vector3 vector, float radius = 2.7f)
        {


            return vector;
        }

        public static bool IsGround(this Vector3 vector, float range = 3.0f)
        {
            // TODO: probably need to make diagonal checks
            range = range <= 0 ? 0.1f : range;
            Vector3 above = new Vector3(vector.X, vector.Y + 1.0f, vector.Z);
            Vector3 below = new Vector3(vector.X, vector.Y - range, vector.Z);
            Vector3 hit, distances;
            if (WorldManager.Raycast(above, below, out hit, out distances))
            {
                return true;
            }

            return false;
        }

        public static Vector3 HeightCorrection(this Vector3 vector, float range = 5.0f)
        {
            range = range <= 0 ? 0.1f : range;
            Vector3 above = new Vector3(vector.X, vector.Y + range, vector.Z);
            Vector3 below = new Vector3(vector.X, vector.Y - range, vector.Z);
            Vector3 hit, distances;
            if (WorldManager.Raycast(vector + new Vector3(0,-2, 0), above, out hit, out distances))
            {
                vector = hit;
                vector.Y -= range;
                return vector;
            }

            if (WorldManager.Raycast(vector + new Vector3(0, 2, 0), below, out hit, out distances))
            {
                vector = hit;
                vector.Y += range;
            }

            return vector;
        }

        public static bool IsSafeSphere(this Vector3 vector, float range = 3.0f)
        {
            range = range <= 0 ? 0.1f : range;
            Vector3 above = new Vector3(vector.X, vector.Y + range, vector.Z);
            Vector3 below = new Vector3(vector.X, vector.Y - range, vector.Z);
            Vector3 hit, distances;

            if (WorldManager.Raycast(vector, above, out hit, out distances))
            {
                return false;
            }

            if (WorldManager.Raycast(vector, below, out hit, out distances))
            {
                return false;
            }

            var side = range / (float)Math.Sqrt(2);

            var vector1 = new Vector3(vector.X + side, vector.Y, vector.Z + side);
            var vector2 = new Vector3(vector.X + side, vector.Y, vector.Z - side);
            var vector3 = new Vector3(vector.X - side, vector.Y, vector.Z + side);
            var vector4 = new Vector3(vector.X - side, vector.Y, vector.Z - side);

            if (WorldManager.Raycast(vector, vector1, out hit, out distances))
            {
                return false;
            }
            if (WorldManager.Raycast(vector, vector2, out hit, out distances))
            {
                return false;
            }
            if (WorldManager.Raycast(vector, vector3, out hit, out distances))
            {
                return false;
            }
            if (WorldManager.Raycast(vector, vector4, out hit, out distances))
            {
                return false;
            }

            return true;
        }

        public static Vector3 GetFloor(this Vector3 vector, float maxDistanceToCheck = 1000.0f)
        {
            Vector3 below = new Vector3(vector.X, vector.Y - maxDistanceToCheck, vector.Z);
            Vector3 hit, distances;
            if (WorldManager.Raycast(vector, below, out hit, out distances))
            {
                return hit;
            }

            return vector;
        }

        public static Vector3 GetCeiling(this Vector3 vector, float maxDistanceToCheck = 1000.0f)
        {
            Vector3 above = new Vector3(vector.X, vector.Y + maxDistanceToCheck, vector.Z);
            Vector3 hit, distances;
            if (WorldManager.Raycast(vector, above, out hit, out distances))
            {
                return hit;
            }

            return vector;
        }

        public static Vector3 AddRandomDirection(this Vector3 vector, float range = 2.0f)
        {

            var random = new Vector3(
                vector.X + (float)MathEx.Random(0.0, range),
                vector.Y + (float)MathEx.Random(0.0, range),
                vector.Z + (float)MathEx.Random(0.0, range));

            Logging.WriteDiagnostic(
                Colors.DarkKhaki,
                "ExBuddy: Adding Random Direction.  from {0} to {1}",
                vector,
                random);

            return random;
        }

        public static Vector3 AddRandomDirection2D(this Vector3 vector, float range = 2.0f)
        {
            var random = new Vector3(
                vector.X + (float)MathEx.Random(0.0, range),
                vector.Y,
                vector.Z + (float)MathEx.Random(0.0, range));

            Logging.WriteDiagnostic(
                Colors.DarkKhaki,
                "ExBuddy: Adding Random Direction2D.  from {0} to {1}",
                vector,
                random);

            return random;
        }
    }
}
