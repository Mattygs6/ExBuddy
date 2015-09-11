namespace ExBuddy.OrderBotTags
{
    using System.Threading.Tasks;
    using System.Windows.Media;

    using Buddy.Coroutines;

    using ExBuddy.OrderBotTags.Gather.Rotations;

    using ff14bot;
    using ff14bot.Helpers;
    using ff14bot.Managers;
    using ff14bot.Objects;

    internal static class Actions
    {
        internal static async Task<bool> CastAura(uint spellId, int delay, int auraId = -1)
        {
            bool result;
            if (auraId == -1 || !Core.Player.HasAura(auraId))
            {
                SpellData spellData;
                if (GatheringManager.ShouldPause(spellData = DataManager.SpellCache[spellId]))
                {
                    await
                        Coroutine.Wait(
                            2500,
                            () => GatheringManager.ShouldPause(spellData = DataManager.SpellCache[spellId]));
                }

                result = Actionmanager.DoAction(spellId, Core.Player);

                if (result)
                {
                    Logging.Write(
                                Colors.DarkKhaki,
                                "ExBuddy: Casted Aura -> {0}",
                                spellData.Name);
                }
                else
                {
                    Logging.Write(
                                Colors.Red,
                                "ExBuddy: Failed to cast Aura -> {0}",
                                spellData.Name);
                }

                //Wait till we have the aura
                await Coroutine.Wait(2500, () => Core.Player.HasAura(auraId));
                await Coroutine.Sleep(delay);
            }
            else
            {
                result = false;
            }

            return result;
        }

        internal static async Task<bool> CastAura(Ability ability, int delay, AbilityAura aura = AbilityAura.None)
        {

            return await CastAura(Abilities.Map[Core.Player.CurrentJob][ability], delay, (int)aura);
        }

        internal static async Task<bool> Cast(uint id, int delay)
        {
            //Wait till we can cast the spell
            SpellData spellData;
            if (GatheringManager.ShouldPause(spellData = DataManager.SpellCache[id]))
            {
                await
                    Coroutine.Wait(
                        2500,
                        () => GatheringManager.ShouldPause(spellData = DataManager.SpellCache[id]));
            }

            var result = Actionmanager.DoAction(id, Core.Player);

            if (result)
            {
                Logging.Write(
                            Colors.SpringGreen,
                            "ExBuddy: Casted Ability -> {0}",
                            spellData.Name);
            }
            else
            {
                Logging.Write(
                            Colors.Red,
                            "ExBuddy: Failed to cast Ability -> {0}",
                            spellData.Name);
            }

            //Wait till we can cast again
            await GatheringRotation.Wait();
            await Coroutine.Sleep(delay);

            return result;
        }

        internal static async Task<bool> Cast(Ability ability, int delay)
        {
            return await Cast(Abilities.Map[Core.Player.CurrentJob][ability], delay);
        }
    }
}