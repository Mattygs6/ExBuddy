namespace ExBuddy
{
	using System;
	using System.Linq;
	using System.Reflection;
	using System.Threading.Tasks;
	using ExBuddy.Attributes;
	using ff14bot;
	using GreyMagic;
	using logr = ff14bot.Helpers.Logging;

	public static class SecondaryOffsetManager
	{
		public static IntPtr Rebase;

		public static bool Initalized { get; private set; }

		public static void IntalizeOffsets()
		{
			if (SecondaryOffsetManager.Initalized)
			{
				return;
			}

			Rebase = Core.Memory.Process.MainModule.BaseAddress;

			//The namespace specified should only be containers for offsets
			var types =
				Assembly.GetExecutingAssembly()
					.GetTypes()
					.Where(t => t.Namespace == "ExBuddy.Offsets" && t.IsClass)
					.OrderBy(t => t.Name)
					.ToArray();

			Parallel.ForEach(
				types,
				type =>
				{
					var pf = new PatternFinder(Core.Memory);

					foreach (var info in type.GetFields())
					{
#if RB_X64
                            var offset = (Offset64)Attribute.GetCustomAttributes(info, typeof(Offset64)).FirstOrDefault();

#else
						var offset =
							(Offset)
								Attribute.GetCustomAttributes(info, typeof (Offset)).FirstOrDefault(r => r.GetType() != typeof (OffsetCN));

#if RB_CN
						     var tmp = (Offset)Attribute.GetCustomAttribute(info, typeof(OffsetCN));
						     if (tmp != null)
						     {
						        offset = tmp;
						     }
    #endif
#endif

						if (offset == null)
						{
							continue;
						}

						try
						{
							var markedDontRebase = false;

							var pattern = offset.Pattern;
							if (!pattern.Trim().EndsWith("DontRebase"))
							{
								pattern = pattern + " DontRebase";
							}

							var results = pf.FindMany(pattern, ref markedDontRebase);
							if (results == null)
							{
								//Failed to find a pattern match.
								logr.Write("No match for {0} some functionality may not work correctly", info.Name);
								continue;
							}

							if (results.Length > 1)
							{
								lock (Core.Memory)
								{
									if (offset.MultipleResults)
									{
										if (results.Distinct().Count() == 1)
										{
											//Multiple matches were expected but there was only one result, double check that our pattern is still finding what we wanted
											logr.Write(
												"Multiple matches for {0} were expected, but only one result was found, some functionality may not work correctly",
												info.Name);
										}
									}
									else
									{
										//Multiple matches to the provided pattern were found and we were not expecting this
										logr.Write(
											"Multiple matches for {0} which was not expected, some functionality may not work correctly",
											info.Name);
									}
								}
							}

#if RB_X64
                                var addrz = (long)results[0];

								if (offset.Modifier != 0)
								{
									addrz = (long)(addrz + offset.Modifier);
								}

								logr.Write("[SecondaryOffsetManager] Found 0x{0:X} for {1}", addrz, info.Name);

								if (info.FieldType == typeof(IntPtr))
								{
									info.SetValue(null, (IntPtr)addrz);
								}
								else
								{
									info.SetValue(null, (int)addrz);
								}
#else

							var addrz = (uint) results[0];

							if (offset.Modifier != 0)
							{
								addrz = (uint) (addrz + offset.Modifier);
							}

							logr.Write("[SecondaryOffsetManager] Found 0x{0:X} for {1}", addrz, info.Name);

							if (info.FieldType == typeof (IntPtr))
							{
								info.SetValue(null, (IntPtr) addrz);
							}
							else
							{
								info.SetValue(null, (int) addrz);
							}

#endif
						}
						catch (Exception e)
						{
							//Something went wrong
							logr.WriteException(e);
						}
					}
				});

			SecondaryOffsetManager.Initalized = true;
		}
	}
}