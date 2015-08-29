namespace ExBuddy.OrderBotTags.Gather.Rotations
{
    using System.Threading.Tasks;

    using Buddy.Coroutines;

    using ff14bot;
    using ff14bot.Managers;

    //Name, RequiredGp, RequiredTime
    [GatheringRotation("Default", 0, 23)]
    public class DefaultGatheringRotation : IGatheringRotation
    {
        protected static readonly uint[] WardSkills = { 236U, 293U, 234U, 292U, 217U, 219U };
        public virtual bool ForceGatherIfMissingGpOrTime
        {
            get
            {
                return false;
            }
        }

        public virtual async Task<bool> Prepare(GatherCollectable tag)
        {
            tag.GatherItem.GatherItem();
            await Coroutine.Sleep(2200);

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

            if (Core.Player.CurrentGP >= 50 && tag.GatherItem.Chance < 100)
            {
                await Actions.Cast(Ability.IncreaseGatherChance5);
            }

            return true;
        }

        public virtual async Task<bool> Gather(GatherCollectable tag)
        {
            while (GatheringManager.SwingsRemaining > 0)
            {
                tag.ResolveGatherItem();
                await Coroutine.Sleep(500);
                tag.GatherItem.GatherItem();
            }

            await Coroutine.Sleep(1000);

            return true;
        }

        public virtual bool ShouldOverrideSelectedGatheringRotation(GatherCollectable tag)
        {
            return false;
        }
    }
}