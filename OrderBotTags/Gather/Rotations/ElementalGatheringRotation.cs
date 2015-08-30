namespace ExBuddy.OrderBotTags.Gather.Rotations
{
    using System.Linq;
    using System.Threading.Tasks;

    using Buddy.Coroutines;

    using ff14bot;
    using ff14bot.Managers;

    //Name, RequiredGp, RequiredTime
    [GatheringRotation("Elemental", 0, 0)]
    public class ElementalGatheringRotation : UnspoiledGatheringRotation
    {
        public override async Task<bool> Prepare(GatherCollectableTag tag)
        {
            if (Core.Player.HasAura((int)AbilityAura.CollectorsGlove))
            {
                await tag.Cast(Ability.CollectorsGlove);
            }

            if (WardSkills.Any(ward => Actionmanager.CanCast(ward, Core.Player)))
            {
                return true;
            }

            tag.GatherItem.GatherItem();
            await Coroutine.Sleep(1000);

            return true;
        }

        public override async Task<bool> ExecuteRotation(GatherCollectableTag tag)
        {
            if (!DoesWardIncreaseItemYield(tag))
            {
                return true;
            }

            foreach (var ward in WardSkills)
            {
                // TODO: if we change actions to only wait for cast if we can cast the ability
                // TODO: and make sure we have collectors glove/ if not, no need to wait, just use timeout param

                //if (await Actions.Cast(ward))
                //{
                //    break;
                //}

                if (Actionmanager.CanCast(ward, Core.Player))
                {
                    Actionmanager.DoAction(ward, Core.Player);
                    await IncreaseChance(tag);
                    break;
                }
            }

            return true;
        }

        public override int ShouldOverrideSelectedGatheringRotation(GatherCollectableTag tag)
        {
            if (tag.GatherItem.ItemId < 20)
            {
                return 10000;
            }
            
            return -1;
        }

        protected bool DoesWardIncreaseItemYield(GatherCollectableTag tag)
        {
            if (tag.GatherItem.ItemId < 8)
            {
                return true;
            }

            if (tag.GatherItem.ItemId < 14 && Core.Player.ClassLevel >= 41)
            {
                return true;
            }

            if (Core.Player.ClassLevel >= 50)
            {
                return true;
            }

            return false;
        }
    }
}