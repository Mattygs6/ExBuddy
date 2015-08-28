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
                // TODO: look into efficienes here
                await Coroutine.Wait(4000, () => Actionmanager.CanCast(spellId, Core.Player));
                result = Actionmanager.DoAction(spellId, Core.Player);

                //Wait till we can cast methodical again
                await Coroutine.Wait(4000, () => Actionmanager.CanCast(Abilities.Map[Core.Player.CurrentJob][Ability.MethodicalAppraisal], Core.Player));
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
            // TODO: look into efficienes here
            //Wait till we can cast the spell
            await Coroutine.Wait(4000, () => Actionmanager.CanCast(id, Core.Player));
            var result = Actionmanager.DoAction(id, Core.Player);

            //Wait till we can cast methodical again
            await Coroutine.Wait(4000, () => Actionmanager.CanCast(Abilities.Map[Core.Player.CurrentJob][Ability.MethodicalAppraisal], Core.Player));

            return result;
        }

        internal static async Task<bool> Cast(Ability ability)
        {
            return await Cast(Abilities.Map[Core.Player.CurrentJob][ability]);
        }
    }
}