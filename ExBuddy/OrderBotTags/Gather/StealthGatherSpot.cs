namespace ExBuddy.OrderBotTags.Gather
{
    using System.ComponentModel;
    using System.Threading.Tasks;

    using Buddy.Coroutines;

    using Clio.Utilities;
    using Clio.XmlEngine;

    using ExBuddy.Helpers;
    using ExBuddy.Interfaces;

    using ff14bot;

    [XmlElement("StealthGatherSpot")]
    public class StealthGatherSpot : IGatherSpot
    {
        [XmlAttribute("NodeLocation")]
        public Vector3 NodeLocation { get; set; }

        [DefaultValue(true)]
        [XmlAttribute("UseMesh")]
        public bool UseMesh { get; set; }

        public virtual async Task<bool> MoveFromSpot(GatherCollectableTag tag)
        {
            if (Core.Player.HasAura((int)AbilityAura.Stealth))
            {
                return await tag.CastAura(Ability.Stealth);
            }

            return true;
        }

        public virtual async Task<bool> MoveToSpot(GatherCollectableTag tag)
        {
            var result = await Behaviors.MoveTo(
                this.NodeLocation,
                this.UseMesh,
                radius: tag.Distance,
                name: tag.Node.EnglishName,
                stopCallback: tag.MovementStopCallback,
                dismountAtDestination: true);

            await Coroutine.Yield();

            result &= await tag.CastAura(Ability.Stealth, AbilityAura.Stealth);

            return result;
        }

        public override string ToString()
        {
            return string.Format("GatherSpot -> NodeLocation: {0}, UseMesh: {1}", this.NodeLocation, this.UseMesh);
        }
    }
}