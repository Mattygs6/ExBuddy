namespace ExBuddy.OrderBotTags.Gather.Rotations
{
    using System;
    using System.Threading.Tasks;

    using ff14bot.Managers;

    //Name, RequiredGp, RequiredTime
    [GatheringRotation("SmartYield", 500, 0)]
    public class SmartYieldGatheringRotation : RegularNodeGatheringRotation
    {
        public override async Task<bool> ExecuteRotation(GatherCollectableTag tag)
        {

            return true;
        }

        public override int ShouldOverrideSelectedGatheringRotation(GatherCollectableTag tag)
        {
            if (tag.Node.EnglishName.IndexOf("unspoiled", StringComparison.InvariantCultureIgnoreCase) >= 0)
            {
                return -1;
            }

            if (tag.Node.EnglishName.IndexOf("ephemeral", StringComparison.InvariantCultureIgnoreCase) >= 0)
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