namespace ExBuddy.OrderBotTags.Gather.Rotations
{
    using System.Threading.Tasks;

    using Buddy.Coroutines;

    using ExBuddy.OrderBotTags.Common;

    using ff14bot;
    using ff14bot.Managers;

    public abstract class GatheringRotation : IGatheringRotation
    {
        protected internal readonly IGetOverridePriority GetOverridePriorityCached;
        protected internal static readonly uint[] WardSkills = { 236U, 293U, 234U, 292U, 217U, 219U };

        public virtual GatheringRotationAttribute Attributes
        {
            get
            {
                return
                    ReflectionHelper.CustomAttributes<GatheringRotationAttribute>.NotInherited[this.GetType().GUID][0];
            }
        }

        public virtual bool CanBeOverriden
        {
            get
            {
                return true;
            }
        }

        public virtual bool ShouldForceGather
        {
            get
            {
                return false;
            }
        }

        protected GatheringRotation()
        {
            this.GetOverridePriorityCached = this as IGetOverridePriority;
        }

        public virtual async Task<bool> Prepare(GatherCollectableTag tag)
        {
            if (Core.Player.HasAura((int)AbilityAura.CollectorsGlove))
            {
                return await tag.Cast(Ability.CollectorsGlove);
            }

            return true;
        }

        public virtual async Task<bool> ExecuteRotation(GatherCollectableTag tag)
        {
            return await IncreaseChance(tag);
        }

        public virtual async Task<bool> Gather(GatherCollectableTag tag)
        {
            while (tag.Node.CanGather && GatheringManager.SwingsRemaining > 0 && Behaviors.ShouldContinue)
            {
                await Wait();

                if (GatheringManager.GatheringCombo == 4 && GatheringManager.SwingsRemaining > 0)
                {
                    await tag.Cast(Ability.IncreaseGatherChanceQuality100);

                    await Wait();
                }

                if (!await tag.ResolveGatherItem())
                {
                    return false;
                }

                var swingsRemaining = GatheringManager.SwingsRemaining - 1;

                if (!tag.GatherItem.TryGatherItem())
                {
                    return false;
                }

                var ticks = 0;
                while (swingsRemaining != GatheringManager.SwingsRemaining && ticks < 60 && Behaviors.ShouldContinue)
                {
                    await Coroutine.Yield();
                    ticks++;
                }
            }

            return true;
        }

        public int ResolveOverridePriority(GatherCollectableTag tag)
        {
            if (this.GetOverridePriorityCached != null)
            {
                return this.GetOverridePriorityCached.GetOverridePriority(tag);
            }

            return -1;
        }

        protected internal static async Task Wait()
        {
            if (GatheringManager.ShouldPause(DataManager.SpellCache[(uint)Ability.Preparation]))
            {
                var ticks = 0;
                while (ticks < 60 && Behaviors.ShouldContinue)
                {
                    if (!GatheringManager.ShouldPause(DataManager.SpellCache[(uint)Ability.Preparation]))
                    {
                        break;
                    }

                    await Coroutine.Yield();
                    ticks++;
                }
            }
        }

        protected virtual async Task<bool> IncreaseChance(GatherCollectableTag tag)
        {
            if (Core.Player.CurrentGP >= 250 && tag.GatherItem.Chance < 51)
            {
                return await tag.Cast(Ability.IncreaseGatherChance50);
            }

            if (Core.Player.CurrentGP >= 100 && tag.GatherItem.Chance < 86)
            {
                if (Core.Player.ClassLevel >= 23 && GatheringManager.SwingsRemaining == 1)
                {
                    return await tag.Cast(Ability.IncreaseGatherChanceOnce15);
                }

                return await tag.Cast(Ability.IncreaseGatherChance15);
            }

            if (Core.Player.CurrentGP >= 50 && tag.GatherItem.Chance < 96)
            {
                if (Core.Player.ClassLevel >= 23 && GatheringManager.SwingsRemaining == 1)
                {
                    return await tag.Cast(Ability.IncreaseGatherChanceOnce15);
                }

                return await tag.Cast(Ability.IncreaseGatherChance5);
            }

            return true;
        }
    }
}
