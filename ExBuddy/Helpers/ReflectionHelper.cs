namespace ExBuddy.Helpers
{
	using System;
	using System.Collections.Concurrent;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;

	/// <summary>
	///     The reflection helper.
	/// </summary>
	public static class ReflectionHelper
	{
		/// <summary>
		///     The custom attributes.
		/// </summary>
		/// <typeparam name="TAttribute">
		///     The attribute type
		/// </typeparam>
		public static class CustomAttributes<TAttribute>
			where TAttribute : Attribute
		{
			#region Constructors and Destructors

			/// <summary>
			///     Initializes static members of the <see cref="CustomAttributes{TAttribute}" /> class.
			/// </summary>
			static CustomAttributes()
			{
				NotInherited = new ConcurrentDictionary<Guid, IList<TAttribute>>();
				Inherited = new ConcurrentDictionary<Guid, IList<TAttribute>>();
			}

			#endregion

			#region Static Fields

			/// <summary>
			///     The inherited.
			/// </summary>
			public static readonly ConcurrentDictionary<Guid, IList<TAttribute>> Inherited;

			/// <summary>
			///     The not inherited.
			/// </summary>
			public static readonly ConcurrentDictionary<Guid, IList<TAttribute>> NotInherited;

			#endregion

			#region Public Methods and Operators

			/// <summary>
			///     The register type.
			/// </summary>
			/// <param name="type">
			///     The type.
			/// </param>
			public static void RegisterType(Type type)
			{
				var guid = type.GUID;

				if (!NotInherited.ContainsKey(guid))
				{
					NotInherited[guid] = type.GetCustomAttributes<TAttribute>(false).ToArray();
				}

				if (!Inherited.ContainsKey(guid))
				{
					Inherited[guid] = type.GetCustomAttributes<TAttribute>(true).ToArray();
				}
			}

			/// <summary>
			///     The register type.
			/// </summary>
			/// <typeparam name="T">
			/// </typeparam>
			public static void RegisterType<T>() where T : class
			{
				RegisterType(typeof (T));
			}

			/// <summary>
			///     The register types.
			/// </summary>
			/// <param name="types">
			///     The types.
			/// </param>
			public static void RegisterTypes(params Type[] types)
			{
				if (types != null)
				{
					foreach (var type in types)
					{
						RegisterType(type);
					}
				}
			}

			public static void RegisterByAssembly(Assembly assembly = null, Func<Type, bool> predicate = null)
			{
				assembly = assembly ?? Assembly.GetExecutingAssembly();

				if (predicate == null)
				{
					predicate = t => t.GetCustomAttribute<TAttribute>() != null;
				}

				RegisterTypes(assembly.GetTypes().Where(predicate).ToArray());
			}

			#endregion
		}
	}

	/// <summary>
	///     The reflection helper.
	/// </summary>
	/// <typeparam name="T">
	/// </typeparam>
	public static class ReflectionHelper<T>
		where T : class
	{
		#region Public Methods and Operators

		/// <summary>
		///     The make delegate.
		/// </summary>
		/// <param name="get">
		///     The get.
		/// </param>
		/// <typeparam name="TResult">
		/// </typeparam>
		/// <returns>
		///     The <see cref="Delegate" />.
		/// </returns>
		public static Func<T, object> MakeDelegate<TResult>(MethodInfo @get)
		{
			var f = (Func<T, TResult>) Delegate.CreateDelegate(typeof (Func<T, TResult>), @get);
			return t => f(t);
		}

		#endregion

		/// <summary>
		///     The custom attributes.
		/// </summary>
		/// <typeparam name="TAttribute">
		/// </typeparam>
		public static class CustomAttributes<TAttribute>
			where TAttribute : Attribute
		{
			#region Static Fields

			/// <summary>
			///     The inherited.
			/// </summary>
			public static readonly IList<TAttribute> Inherited = typeof (T).GetCustomAttributes<TAttribute>(true).ToArray();

			/// <summary>
			///     The not inherited.
			/// </summary>
			public static readonly IList<TAttribute> NotInherited = typeof (T).GetCustomAttributes<TAttribute>(false).ToArray();

			#endregion
		}
	}
}