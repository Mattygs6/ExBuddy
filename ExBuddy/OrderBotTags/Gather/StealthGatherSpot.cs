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

        [XmlAttribute("UnstealthAfter")]
        public bool UnstealthAfter { get; set; }

        public virtual async Task<bool> MoveFromSpot(GatherCollectableTag tag)
        {
            tag.StatusText = "Moving from " + this;

            if (UnstealthAfter && Core.Player.HasAura((int)AbilityAura.Stealth))
            {
                return await tag.CastAura(Ability.Stealth);
            }

            return true;
        }

        public virtual async Task<bool> MoveToSpot(GatherCollectableTag tag)
        {
            tag.StatusText = "Moving to " + this;

            var result = await Behaviors.MoveTo(
                NodeLocation,
                UseMesh,
                radius: tag.Distance,
                name: tag.Node.EnglishName,
                stopCallback: tag.MovementStopCallback,
                dismountAtDestination: true);

            if (result)
            {
                await Coroutine.Yield();
                await tag.CastAura(Ability.Stealth, AbilityAura.Stealth);
            }

            await Coroutine.Yield();

            return result;
        }

        public override string ToString()
        {
            return string.Format("GatherSpot -> NodeLocation: {0}, UseMesh: {1}", NodeLocation, UseMesh);
        }
    }
}