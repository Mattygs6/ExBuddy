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

            // We can see the elemental item, no need to hit the node.
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
            if (Core.Player.CurrentGP < 400)
            {
                return true;
            }

            var ward = WardSkills.FirstOrDefault(w => Actionmanager.CanCast(w, Core.Player));
            
            if(ward > 0)
            {
                Actionmanager.DoAction(ward, Core.Player);
                await IncreaseChance(tag);
            }

            return true;
        }

        public override int ShouldOverrideSelectedGatheringRotation(GatherCollectableTag tag)
        {
            // Don't use unless ward increases item yield.
            if (!DoesWardIncreaseItemYield(tag))
            {
                return -1;
            }

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