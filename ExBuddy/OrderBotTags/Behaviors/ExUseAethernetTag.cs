
#pragma warning disable 1998

namespace ExBuddy.OrderBotTags.Behaviors
{
	using System.ComponentModel;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows.Media;
	using Buddy.Coroutines;
	using Clio.Utilities;
	using Clio.XmlEngine;
	using ExBuddy.Attributes;
	using ExBuddy.Interfaces;
	using ff14bot.Behavior;
	using ff14bot.RemoteWindows;

	[LoggerName("ExUseAethernet")]
	[XmlElement("ExUseAethernet")]
	public class ExUseAethernetTag : ExProfileBehavior, IInteractWithNpc
	{
		[DefaultValue("Aethernet.")]
		[XmlAttribute("AethernetText")]
		public string AethernetText { get; set; }

		[DefaultValue(8.0f)]
		[XmlAttribute("Distance")]
		public float Distance { get; set; }

		[XmlAttribute("Slot")]
		public uint Slot { get; set; }

		protected override Color Info
		{
			get { return Colors.DodgerBlue; }
		}

		protected override async Task<bool> Main()
		{
			await this.Interact(Distance);

			await Coroutine.Wait(5000, () => SelectString.IsOpen);
			if (!SelectString.IsOpen)
			{
				Logger.Error(Localization.Localization.ExUseAethernet_SelectLineTimeout);
				return isDone = true;
			}

			if (SelectString.Lines().Any(line => line.Contains(AethernetText)))
			{
				Logger.Info(Localization.Localization.ExUseAethernet_SelectLine + AethernetText);
				SelectString.ClickLineContains(AethernetText);
				// SelectString.ClickSlot(0);  going to try to make it more compatible with possible changes to game.

				await Coroutine.Wait(5000, () => !SelectString.IsOpen);
				await Coroutine.Wait(10000, () => SelectString.IsOpen);

				if (!SelectString.IsOpen)
				{
					Logger.Error(Localization.Localization.ExUseAethernet_SelectLineTimeout);
					return isDone = true;
				}
			}

			Logger.Info(Localization.Localization.ExUseAethernet_SelectLine + Slot);
			SelectString.ClickSlot(Slot);

			await Coroutine.Wait(5000, () => CommonBehaviors.IsLoading);
			await CommonTasks.HandleLoading();

			await Coroutine.Sleep(2000); // Weird stuff happens without this.

			return isDone = true;
		}

		#region IInteractWithNpc Members

		[XmlAttribute("XYZ")]
		[XmlAttribute("Location")]
		public Vector3 Location { get; set; }

		[XmlAttribute("Id")]
		public uint NpcId { get; set; }

		#endregion
	}
}