namespace ExBuddy.OrderBotTags.Gather.Rotations
{
    using System.Threading.Tasks;
    using System.Windows.Media;

    using ExBuddy.Interfaces;

    using ff14bot.Helpers;

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

        protected bool ShouldForceUseRotation(GatherCollectableTag tag, uint level)
        {
            if (!tag.GatherItemIsFallback && ((level < 50 && tag.NodesGatheredAtMaxGp > 4) || tag.NodesGatheredAtMaxGp > 6))
            {
                Logging.Write(
                    Colors.Chartreuse,
                    "GatherCollectable: Using Gp since we have gathered {0} nodes at max Gp.",
                    tag.NodesGatheredAtMaxGp);

                return true;
            }

            return false;
        }
    }
}
