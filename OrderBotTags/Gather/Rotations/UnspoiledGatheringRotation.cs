namespace ExBuddy.OrderBotTags.Gather.Rotations
{
    using System.Threading.Tasks;

    using Buddy.Coroutines;

    using ff14bot;
    using ff14bot.Managers;

    //Name, RequiredGp, RequiredTime
    [GatheringRotation("Unspoiled", 500, 23)]
    public class UnspoiledGatheringRotation : IGatheringRotation
    {
        public GatheringRotationAttribute Attributes
        {
            get
            {
                return
                    ReflectionHelper.CustomAttributes<GatheringRotationAttribute>.NotInherited[this.GetType().GUID][0];
            }
        }

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

        public virtual async Task<bool> Prepare(GatherCollectableTag tag)
        {
            if (Core.Player.HasAura((int)AbilityAura.CollectorsGlove))
            {
                await tag.Cast(Ability.CollectorsGlove);
            }

            while (GatheringManager.ShouldPause(DataManager.SpellCache[(uint)Ability.Preparation]))
            {
                await Coroutine.Yield();
            }

            ////await
                ////Coroutine.Wait(
                ////    tag.WindowDelay + 500,
                ////    () =>
                ////    Actionmanager.CanCast(Abilities.Map[Core.Player.CurrentJob][Ability.Preparation], Core.Player));
            tag.GatherItem.GatherItem();

            return true;
        }

        public virtual async Task<bool> ExecuteRotation(GatherCollectableTag tag)
        {
            if (Core.Player.CurrentGP >= 500)
            {
                await tag.Cast(Ability.IncreaseGatherYield2);
            }

            await IncreaseChance(tag);

            return true;
        }

        public virtual async Task<bool> Gather(GatherCollectableTag tag)
        {
            int swingsRemaining;
            while ((swingsRemaining = GatheringManager.SwingsRemaining) > 0)
            {
                swingsRemaining--;

                if (!await tag.ResolveGatherItem())
                {
                    return false;
                }

                while (GatheringManager.ShouldPause(DataManager.SpellCache[(uint)Ability.Preparation]))
                {
                    await Coroutine.Yield();
                }

                if (GatheringManager.GatheringCombo == 4)
                {
                    await tag.Cast(Ability.IncreaseGatherChanceQuality100);

                    while (GatheringManager.ShouldPause(DataManager.SpellCache[(uint)Ability.Preparation]))
                    {
                        await Coroutine.Yield();
                    }
                }

                ////await
                ////    Coroutine.Wait(
                ////        500,
                ////        () =>
                ////        Actionmanager.CanCast(Abilities.Map[Core.Player.CurrentJob][Ability.Preparation], Core.Player));

                tag.GatherItem.GatherItem();

                await Coroutine.Wait(2500, () => swingsRemaining == GatheringManager.SwingsRemaining);
            }

            return true;
        }

        public virtual int ShouldOverrideSelectedGatheringRotation(GatherCollectableTag tag)
        {
            return -1;
        }

        protected virtual async Task<bool> IncreaseChance(GatherCollectableTag tag)
        {
            if (Core.Player.CurrentGP >= 50 && tag.GatherItem.Chance < 100)
            {
                return await tag.Cast(Ability.IncreaseGatherChance5);
            }

            return false;
        }
    }
}