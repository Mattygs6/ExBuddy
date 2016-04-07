namespace ExBuddy.Helpers
{
	using System.Threading.Tasks;
	using Buddy.Coroutines;
	using ExBuddy.Logging;
	using ff14bot;
	using ff14bot.Managers;
	using ff14bot.Objects;

	internal static class Actions
	{
		internal static async Task<bool> Cast(uint id, int delay)
		{
			//TODO: check affinity, cost type, spell type, and add more informational logging and procedures to casting
			//Wait till we can cast the spell
			SpellData spellData;
			if (GatheringManager.ShouldPause(spellData = DataManager.SpellCache[id]))
			{
				await Coroutine.Wait(3500, () => !GatheringManager.ShouldPause(spellData));
			}

			var result = Actionmanager.DoAction(id, Core.Player);

			var ticks = 0;
			while (result == false && ticks++ < 5 && Behaviors.ShouldContinue)
			{
				result = Actionmanager.DoAction(id, Core.Player);
				await Coroutine.Yield();
			}

			if (result)
			{
				Logger.Instance.Info("Casted Ability -> {0}", spellData.Name);
			}
			else
			{
				Logger.Instance.Error("Failed to cast Ability -> {0}", spellData.Name);
			}

			//Wait till we can cast again
			if (GatheringManager.ShouldPause(spellData))
			{
				await Coroutine.Wait(3500, () => !GatheringManager.ShouldPause(spellData));
			}
			if (delay > 0)
			{
				await Coroutine.Sleep(delay);
			}
			else
			{
				await Coroutine.Yield();
			}

			return result;
		}

		internal static async Task<bool> Cast(Ability ability, int delay)
		{
			return await Cast(Abilities.Map[Core.Player.CurrentJob][ability], delay);
		}

		internal static async Task<bool> CastAura(uint spellId, int delay, int auraId = -1)
		{
			var result = false;
			if (auraId == -1 || !Core.Player.HasAura(auraId))
			{
				SpellData spellData;
				if (GatheringManager.ShouldPause(spellData = DataManager.SpellCache[spellId]))
				{
					await Coroutine.Wait(3500, () => !GatheringManager.ShouldPause(DataManager.SpellCache[spellId]));
				}

				result = Actionmanager.DoAction(spellId, Core.Player);
				var ticks = 0;
				while (result == false && ticks++ < 5 && Behaviors.ShouldContinue)
				{
					result = Actionmanager.DoAction(spellId, Core.Player);
					await Coroutine.Yield();
				}

				if (result)
				{
					Logger.Instance.Info("Casted Aura -> {0}", spellData.Name);
				}
				else
				{
					Logger.Instance.Error("Failed to cast Aura -> {0}", spellData.Name);
				}

				//Wait till we have the aura
				await Coroutine.Wait(3500, () => Core.Player.HasAura(auraId));
				if (delay > 0)
				{
					await Coroutine.Sleep(delay);
				}
				else
				{
					await Coroutine.Yield();
				}
			}

			return result;
		}

		internal static async Task<bool> CastAura(Ability ability, int delay, AbilityAura aura = AbilityAura.None)
		{
			return await CastAura(Abilities.Map[Core.Player.CurrentJob][ability], delay, (int) aura);
		}
	}
}