namespace ExBuddy.OrderBotTags.Gather.Rotations
{
    //Name, RequiredGp, RequiredTime
    [GatheringRotation("RegularNode", 0, 0)]
    public sealed class RegularNodeGatheringRotation : GatheringRotation, IGetOverridePriority
    {
        int IGetOverridePriority.GetOverridePriority(GatherCollectableTag tag)
        {
            if (tag.IsEphemeral() || tag.IsUnspoiled())
            {
                return -1;
            }

            return 8000;
        }
    }
}