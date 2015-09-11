namespace ExBuddy.OrderBotTags.Gather.Rotations
{
    using System.Threading.Tasks;

    using ff14bot;

    //[GatheringRotation("Collect240Dynamic", 200, 28)]
    public sealed class Collect240DynamicGatheringRotation : CollectableGatheringRotation, IGetOverridePriority
    {
        public override async Task<bool> Prepare(GatherCollectableTag tag)
        {
            // TODO: how much gathering to 1 hit?  needs to be added into this logic.
            if (Core.Player.CurrentGP >= 500)
            {
                await tag.Cast(Ability.Toil);
            }
            
            return await base.Prepare(tag);

        } 
        public override async Task<bool> ExecuteRotation(GatherCollectableTag tag)
        {
            if (Core.Player.ClassLevel > 50)
            {
                return await DoRotation(tag);
            }

            return await DoLevel50Rotation(tag);
        }

        private async Task<bool> DoLevel50Rotation(GatherCollectableTag tag)
        {
            // 240-345 collectability
            await Methodical(tag);
            await Methodical(tag);
            await Methodical(tag);

            if (Core.Player.CurrentGP >= 300)
            {
                await tag.Cast(Ability.AdditionalAttempt);    
            }
            
            return true;
        }

        private async Task<bool> DoRotation(GatherCollectableTag tag)
        {
            // 96-180 with min perception
            // 138-259 with max perception
            await tag.Cast(Ability.DiscerningEye);
            await tag.Cast(Ability.InstinctualAppraisal);
            
            if (CurrentRarity < 240)
            {
                var perception = Core.Player.Stats.Perception;

                // Methodical range is 80-115
                // Discerning methodical is 120-172
                if (Core.Player.CurrentGP >= 200)
                {
                    // TODO: not complete
                }

                await Methodical(tag);
            }

            await IncreaseChance(tag);

            return true;
        }

        int IGetOverridePriority.GetOverridePriority(GatherCollectableTag tag)
        {
            // if we have a collectable && the collectable value is greater than or equal to 240: Priority 240
            if (tag.CollectableItem != null && tag.CollectableItem.Value >= 240)
            {// Not complete
                //return 240;
            }

            return -1;
        }
    }
}