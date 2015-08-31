namespace ExBuddy.OrderBotTags.Gather.Rotations
{
    using System.Threading.Tasks;

    using ff14bot;
    using ff14bot.Managers;

    //Name, RequiredGp, RequiredTime
    [GatheringRotation("SmartYield", 500, 0)]
    public class SmartYieldGatheringRotation : RegularNodeGatheringRotation
    {
        public override async Task<bool> ExecuteRotation(GatherCollectableTag tag)
        {
            if (Core.Player.CurrentGP >= 500)
            {
                await tag.Cast(Ability.IncreaseGatherYield2);
            }

            await IncreaseChance(tag);

            return true;
        }

        public override int ShouldOverrideSelectedGatheringRotation(GatherCollectableTag tag)
        {
            if (tag.IsEphemeral() || tag.IsUnspoiled())
            {
                return -1;
            }

            // Use smart yield if we have more than 4 swings remaining on a regular node
            if (GatheringManager.SwingsRemaining > 4)
            {
                return 9001;
            }

            return -1;
        }
    }
}