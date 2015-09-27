namespace ExBuddy
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using System.Security.Cryptography;
	using System.Text;

	using Clio.Common;
	using Clio.Utilities;

	using ExBuddy.Enumerations;
	using ExBuddy.Helpers;
	using ExBuddy.Logging;

	using ff14bot;
	using ff14bot.Managers;

	using GreyMagic;

	public static partial class Extensions
	{
		/// <summary>
		/// Restricts a value to be within a specified range.
		/// </summary>
		/// <param name="value">The value to clamp.</param>
		/// <param name="min">The minimum value. If <c>value</c> is less than <c>min</c>, <c>min</c> will be returned.</param>
		/// <param name="max">The maximum value. If <c>value</c> is greater than <c>max</c>, <c>max</c> will be returned.</param>
		/// <returns>The clamped value.</returns>
		public static double Clamp(this double value, double min, double max)
		{
			// First we check to see if we're greater than the max
			value = (value > max) ? max : value;

			// Then we check to see if we're less than the min.
			value = (value < min) ? min : value;

			// There's no check to see if min > max.
			return value;
		}

		/// <summary>
		/// Restricts a value to be within a specified range.
		/// </summary>
		/// <param name="value">The value to clamp.</param>
		/// <param name="min">The minimum value. If <c>value</c> is less than <c>min</c>, <c>min</c> will be returned.</param>
		/// <param name="max">The maximum value. If <c>value</c> is greater than <c>max</c>, <c>max</c> will be returned.</param>
		/// <returns>The clamped value.</returns>
		public static float Clamp(this float value, float min, float max)
		{
			// First we check to see if we're greater than the max
			value = (value > max) ? max : value;

			// Then we check to see if we're less than the min.
			value = (value < min) ? min : value;

			// There's no check to see if min > max.
			return value;
		}

		/// <summary>
		/// Restricts a value to be within a specified range.
		/// </summary>
		/// <param name="value">The value to clamp.</param>
		/// <param name="min">The minimum value. If <c>value</c> is less than <c>min</c>, <c>min</c> will be returned.</param>
		/// <param name="max">The maximum value. If <c>value</c> is greater than <c>max</c>, <c>max</c> will be returned.</param>
		/// <returns>The clamped value.</returns>
		public static int Clamp(this int value, int min, int max)
		{
			// First we check to see if we're greater than the max
			value = (value > max) ? max : value;

			// Then we check to see if we're less than the min.
			value = (value < min) ? min : value;

			// There's no check to see if min > max.
			return value;
		}

		/// <summary>
		///     String conversion to typeof nulllable(bool) utility
		/// </summary>
		/// <param name="input">String version of the object</param>
		/// <returns>string param as nullable bool</returns>
		/// <example>View code: <br />
		///     bool? id = "true".ConvertToBoolean();<br />
		///     bool? id = Extensions.ConvertToBoolean("true");<br />
		/// </example>
		public static bool? ConvertToBoolean(this string input)
		{
			if (!string.IsNullOrEmpty(input))
			{
				return string.Equals(input, bool.TrueString, StringComparison.OrdinalIgnoreCase)
						|| string.Equals(input, "1", StringComparison.OrdinalIgnoreCase);
			}

			return null;
		}

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
			Func<TAttribute, TResult> func,
			TResult defaultValue = default(TResult)) where TAttribute : Attribute
		{
			IList<TAttribute> attrList;
			if (!ReflectionHelper.CustomAttributes<TAttribute>.NotInherited.TryGetValue(type.GUID, out attrList))
			{
				ReflectionHelper.CustomAttributes<TAttribute>.RegisterType(type);
			}

			if (ReflectionHelper.CustomAttributes<TAttribute>.NotInherited.TryGetValue(type.GUID, out attrList))
			{
				var attr = attrList.FirstOrDefault();

				if (attr != null)
				{
					return func(attr);
				}
			}

			return defaultValue;
		}

		public static int IndexOf<T>(this IList<T> list, T value, [NotNull]IEqualityComparer<T> comparer)
		{
			var index = -1;

			foreach (var item in list)
			{
				index++;
				if (comparer.Equals(item, value))
				{
					return index;
				}
			}

			return -1;
		}

		public static bool InRange<T>(this T value, T from, T to) where T : IComparable<T>
		{
			return value.CompareTo(from) >= 1 && value.CompareTo(to) <= -1;
		}

		public static SendActionResult TrySendAction(this AtkAddonControl window, int pairCount, params uint[] param)
		{
			if (window == null || !window.IsValid)
			{
				return SendActionResult.InvalidWindow;
			}

			try
			{
				window.SendAction(pairCount, param);
				return SendActionResult.Success;
			}
			catch (InjectionException ex)
			{
				Logger.Instance.Error(ex.Message);
				return SendActionResult.InjectionError;
			}
		}

		public static bool IsFullStack(this BagSlot bagSlot, bool includeNonStackable = false)
		{
			return bagSlot != null && bagSlot.Item != null && (bagSlot.Count == bagSlot.Item.StackSize && (includeNonStackable || bagSlot.Item.StackSize > 1)) ;
		}

		public static bool IsUnknownChance(this GatheringItem gatheringItem)
		{
			if (gatheringItem.IsUnknown || gatheringItem.Chance == 25)
			{
				return true;
			}

			if (!gatheringItem.IsFilled && gatheringItem.Unk2 != byte.MaxValue && gatheringItem.Unk1 == uint.MaxValue)
			{
				if (gatheringItem.Chance <= 0)
				{
					Logger.Instance.Warn(
						"Found unknown item in slot {0}, but it seems we don't have enough gathering skill. Check the node for more information. Gathering: {1}",
						gatheringItem.SlotIndex + 1,
						Core.Player.Stats.Gathering);
					return false;
				}

				Logger.Instance.Info("Found unknown item in slot {0}", gatheringItem.SlotIndex + 1);
				return true;
			}

			var lastSpellId = Actionmanager.LastSpellId;
			if (gatheringItem.Chance == 30 && lastSpellId == Abilities.Map[Core.Player.CurrentJob][Ability.IncreaseGatherChance5])
			{
				return true;
			}

			if (gatheringItem.Chance == 40
				&& lastSpellId == Abilities.Map[Core.Player.CurrentJob][Ability.IncreaseGatherChance15])
			{
				return true;
			}

			if (gatheringItem.Chance == 75
				&& lastSpellId == Abilities.Map[Core.Player.CurrentJob][Ability.IncreaseGatherChance50])
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
				Logger.Instance.Warn(
					"GatherItem became null between resolving it and gathering it due to the Gathering Window closing, moving on.");

				return false;
			}
		}

		public static bool IsGround(this Vector3 vector, float range = 3.0f)
		{
			// TODO: probably need to make diagonal checks
			range = range <= 0 ? 0.1f : range;
			var above = new Vector3(vector.X, vector.Y + 1.5f, vector.Z);
			var below = new Vector3(vector.X, vector.Y - range, vector.Z);
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
			var above = new Vector3(vector.X, vector.Y + range, vector.Z);
			var below = new Vector3(vector.X, vector.Y - range, vector.Z);
			Vector3 hit, distances;
			if (WorldManager.Raycast(vector + new Vector3(0, -2, 0), above, out hit, out distances))
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

		public static Vector3 CorrectLanding(this Vector3 vector, float radius = 2.7f)
		{
			Vector3 hit;
			Vector3 distances;
			var side = radius / (float)Math.Sqrt(2);

			var vectorSouthEast = new Vector3(vector.X + side, vector.Y, vector.Z + side);
			var vectorNorthEast = new Vector3(vector.X + side, vector.Y, vector.Z - side);
			var vectorSouthWest = new Vector3(vector.X - side, vector.Y, vector.Z + side);
			var vectorNorthWest = new Vector3(vector.X - side, vector.Y, vector.Z - side);

			var myGround = vector.GetFloor();
			var southEastGround = vectorSouthEast.GetFloor();
			var northEastGround = vectorNorthEast.GetFloor();
			var southWestGround = vectorSouthWest.GetFloor();
			var northWestGround = vectorNorthWest.GetFloor();

			float average;
			var sd = StandardDeviation(
				new[] { myGround, southEastGround, northEastGround, southWestGround, northWestGround },
				out average);

			return vector;
		}

		private static float StandardDeviation(IEnumerable<Vector3> vectors, out float average)
		{
			return StandardDeviation(vectors.Select(v => v.Magnitude).ToArray(), out average);
		}

		private static float StandardDeviation(IList<float> values, out float average)
		{
			average = values.Average();
			var a = average;
			var sumOfSquaresOfDiffs = values.Select(v => (v - a) * (v - a)).Sum();
			var sd = Math.Sqrt(sumOfSquaresOfDiffs / values.Count);

			return (float)sd;
		}

		public static bool IsSafeSphere(this Vector3 vector, float range = 3.0f)
		{
			range = range <= 0 ? 0.1f : range;
			var above = new Vector3(vector.X, vector.Y + range, vector.Z);
			var below = new Vector3(vector.X, vector.Y - range, vector.Z);
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
			var below = new Vector3(vector.X, vector.Y - maxDistanceToCheck, vector.Z);
			Vector3 hit, distances;
			if (WorldManager.Raycast(vector, below, out hit, out distances))
			{
				return hit;
			}

			return vector;
		}

		public static Vector3 GetCeiling(this Vector3 vector, float maxDistanceToCheck = 1000.0f)
		{
			var above = new Vector3(vector.X, vector.Y + maxDistanceToCheck, vector.Z);
			Vector3 hit, distances;
			if (WorldManager.Raycast(vector, above, out hit, out distances))
			{
				return hit;
			}

			return vector;
		}

		public static Vector3 AddRandomDirection(this Vector3 vector, float range = 2.0f)
		{
			var side = range / Math.Sqrt(3);
			var random = new Vector3(
				vector.X + (float)MathEx.Random(-side, side),
				vector.Y + (float)MathEx.Random(-side, side),
				vector.Z + (float)MathEx.Random(-side, side));

			Vector3 hit;
			var ticks = 0;
			while (WorldManager.Raycast(vector, random, out hit) && ticks++ < 1000)
			{
				random = new Vector3(
					vector.X + (float)MathEx.Random(-side, side),
					vector.Y + (float)MathEx.Random(-side, side),
					vector.Z + (float)MathEx.Random(-side, side));
			}

			if (ticks > 1000)
			{
				Logger.Instance.Error("Attempted to add Random Direction from {0} but failed", vector);

				return vector;
			}

			Logger.Instance.Info("Adding Random Direction.  from {0} to {1}", vector, random);

			return random;
		}

		public static Vector3 AddRandomDirection2D(this Vector3 vector, float range = 2.0f)
		{
			var side = range / Math.Sqrt(2);
			var random = new Vector3(
				vector.X + (float)MathEx.Random(-side, side),
				vector.Y,
				vector.Z + (float)MathEx.Random(-side, side));

			Vector3 hit;
			var ticks = 0;
			while (WorldManager.Raycast(vector, random, out hit) && ticks++ < 1000)
			{
				random = new Vector3(
					vector.X + (float)MathEx.Random(-side, side),
					vector.Y,
					vector.Z + (float)MathEx.Random(-side, side));
			}

			if (ticks > 1000)
			{
				Logger.Instance.Error("Attempted to add Random Direction from {0} but failed", vector);

				return vector;
			}

			Logger.Instance.Info("Adding Random Direction2D.  from {0} to {1}", vector, random);

			return random;
		}

		public static Guid ToGuid(this string input)
		{
			var provider = new MD5CryptoServiceProvider();

			var inputBytes = Encoding.Default.GetBytes(input);

			var hashBytes = provider.ComputeHash(inputBytes);

			var hashGuid = new Guid(hashBytes);

			return hashGuid;
		}

		public static IList<T> Shuffle<T>(this IList<T> list)
		{
			if (list.Count > 1)
			{
				using (var provider = new RNGCryptoServiceProvider())
				{
					var n = list.Count;
					while (n > 1)
					{
						var box = new byte[1];
						do
						{
							provider.GetBytes(box);
						}
							// ReSharper disable once LoopVariableIsNeverChangedInsideLoop
						while (!(box[0] < n * (byte.MaxValue / n)));

						var k = box[0] % n;
						n--;
						var value = list[k];
						list[k] = list[n];
						list[n] = value;
					}
				}
			}

			return list;
		}
	}
}