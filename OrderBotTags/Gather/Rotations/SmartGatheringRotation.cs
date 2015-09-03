namespace ExBuddy.OrderBotTags.Gather.Rotations
{
    using System.Threading.Tasks;

    public abstract class SmartGatheringRotation: GatheringRotation
    {
        public override Task<bool> Prepare(GatherCollectableTag tag)
        {
            return ResolveInternalGatheringRotation(tag).Prepare(tag);
        }

        protected virtual IGatheringRotation ResolveInternalGatheringRotation(GatherCollectableTag tag)
        {
            if (tag.IsUnspoiled())
            {
                return GatherCollectableTag.Rotations["Unspoiled"];
            }

            return GatherCollectableTag.Rotations["RegularNode"];
        }
    }
}
