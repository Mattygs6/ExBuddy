namespace ExBuddy.OrderBotTags.Gather
{
    using System.Threading.Tasks;

    using Clio.XmlEngine;

    using ExBuddy.Helpers;

    [XmlElement("GatherSpot")]
    public class GatherSpot : StealthGatherSpot
    {
        public override async Task<bool> MoveToSpot(GatherCollectableTag tag)
        {
            var result = await Behaviors.MoveTo(
                NodeLocation,
                UseMesh,
                radius: tag.Distance,
                name: tag.Node.EnglishName,
                stopCallback: tag.MovementStopCallback);

            return result;
        }
    }
}
