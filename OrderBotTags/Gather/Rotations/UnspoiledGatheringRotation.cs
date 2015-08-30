namespace ExBuddy.OrderBotTags.Gather.Rotations
{
    using System.Threading.Tasks;

    using Buddy.Coroutines;

    using ff14bot;
    using ff14bot.Managers;

    //Name, RequiredGp, RequiredTime
    [GatheringRotation("Unspoiled", 0, 23)]
    public class UnspoiledGatheringRotation : IGatheringRotation
    {
        protected static readonly uint[] WardSkills = { 236U, 293U, 234U, 292U, 217U, 219U };

        public virtual bool CanOverride
        {
            get
            {
                return true;
            }
        }

        public virtual bool ForceGatherIfMissingGpOrTime
        {
            get
            {
                return false;
            }
        }

        public virtual async Task<bool> Prepare(GatherCollectable tag)
        {
            if (Core.Player.HasAura((int)AbilityAura.CollectorsGlove))
            {
                await Actions.Cast(Ability.CollectorsGlove);
            }

            tag.GatherItem.GatherItem();
            await Coroutine.Sleep(1000);

            return true;
        }

        public virtual async Task<bool> ExecuteRotation(GatherCollectable tag)
        {
            if (tag.GatherItem.ItemId < 20)
            {
                foreach (var ward in WardSkills)
                {
                    if (Actionmanager.CanCast(ward, Core.Player))
                    {
                        Actionmanager.DoAction(ward, Core.Player);
                        break;
                    }
                }
            } 
            else if (Core.Player.CurrentGP >= 500)
            {
                await Actions.Cast(Ability.IncreaseGatherYield2);
            }

            await IncreaseChance(tag);

            return true;
        }

        public virtual async Task<bool> Gather(GatherCollectable tag)
        {
            while (GatheringManager.SwingsRemaining > 0)
            {
                tag.ResolveGatherItem();

                if (GatheringManager.GatheringCombo == 4)
                {
                    if (
                        await
                        Coroutine.Wait(
                            1000,
                            () =>
                            Actionmanager.CanCast(
                                Abilities.Map[Core.Player.CurrentJob][Ability.IncreaseGatherChanceQuality100],
                                Core.Player)))
                    {
                        await Actions.Cast(Ability.IncreaseGatherChanceQuality100);
                    }
                }

                do
                {
                    await Coroutine.Sleep(200);
                }
                while (!tag.GatherItem.GatherItem());
            }

            return true;
        }

        public virtual int ShouldOverrideSelectedGatheringRotation(GatherCollectable tag)
        {
            return -1;
        }

        protected virtual async Task<bool> IncreaseChance(GatherCollectable tag)
        {
            if (Core.Player.CurrentGP >= 50 && tag.GatherItem.Chance < 100)
            {
                return await Actions.Cast(Ability.IncreaseGatherChance5);
            }

            return false;
        }
    }
}