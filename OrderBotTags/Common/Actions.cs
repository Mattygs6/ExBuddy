namespace ExBuddy.OrderBotTags
{
    using System.Threading.Tasks;

    using Buddy.Coroutines;

    using ff14bot;
    using ff14bot.Managers;

    internal static class Actions
    {
        internal static async Task<bool> CastAura(uint spellId, int auraId = -1)
        {
            bool result;
            if (auraId == -1 || !Core.Player.HasAura(auraId))
            {
                //TODO:make sleep time a param?
                await Coroutine.Wait(2200, () => Actionmanager.CanCast(spellId, Core.Player));
                result = Actionmanager.DoAction(spellId, Core.Player);

                //Wait till we can cast again
                await Coroutine.Wait(2200, () => Actionmanager.CanCast(Abilities.Map[Core.Player.CurrentJob][Ability.CollectorsGlove], Core.Player));
                await Coroutine.Sleep(150);
            }
            else
            {
                result = false;
            }

            return result;
        }

        internal static async Task<bool> CastAura(Ability ability, AbilityAura aura = AbilityAura.None)
        {

            return await CastAura(Abilities.Map[Core.Player.CurrentJob][ability], (int)aura);
        }

        internal static async Task<bool> Cast(uint id)
        {
            //TODO:make sleep time a param?
            //Wait till we can cast the spell
            await Coroutine.Wait(2200, () => Actionmanager.CanCast(id, Core.Player));
            var result = Actionmanager.DoAction(id, Core.Player);

            //Wait till we can cast again
            await Coroutine.Wait(2200, () => Actionmanager.CanCast(Abilities.Map[Core.Player.CurrentJob][Ability.CollectorsGlove], Core.Player));
            await Coroutine.Sleep(150);

            return result;
        }

        internal static async Task<bool> Cast(Ability ability)
        {
            return await Cast(Abilities.Map[Core.Player.CurrentJob][ability]);
        }
    }
}