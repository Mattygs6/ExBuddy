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

    [XmlElement("StealthApproachGatherSpot")]
    public class StealthApproachGatherSpot : IGatherSpot
    {
        [XmlAttribute("NodeLocation")]
        public Vector3 NodeLocation { get; set; }

        [XmlAttribute("StealthLocation")]
        public Vector3 StealthLocation { get; set; }
            
        [DefaultValue(true)]
        [XmlAttribute("ReturnToStealthLocation")]
        public bool ReturnToStealthLocation { get; set; }

        [DefaultValue(true)]
        [XmlAttribute("UseMesh")]
        public bool UseMesh { get; set; }

        [XmlAttribute("UnstealthAfter")]
        public bool UnstealthAfter { get; set; }

        public async Task<bool> MoveFromSpot(GatherCollectableTag tag)
        {
            var result = true;
            if (this.ReturnToStealthLocation)
            {
                result &= await Behaviors.MoveToNoMount(this.StealthLocation, this.UseMesh, tag.Radius, tag.Node.EnglishName, tag.MovementStopCallback);
            }

            if (this.UnstealthAfter && Core.Player.HasAura((int)AbilityAura.Stealth))
            {
                result &= await tag.CastAura(Ability.Stealth);
            }

            return result;
        }

        public async Task<bool> MoveToSpot(GatherCollectableTag tag)
        {
            if (this.StealthLocation == Vector3.Zero)
            {
                return false;
            }

            var result = await Behaviors.MoveTo(
                this.StealthLocation,
                this.UseMesh,
                radius: tag.Radius,
                name: "Stealth Location",
                stopCallback: tag.MovementStopCallback,
                dismountAtDestination: true);

            if (result)
            {
                await Coroutine.Yield();
                await tag.CastAura(Ability.Stealth, AbilityAura.Stealth);

                result = await Behaviors.MoveToNoMount(this.NodeLocation, this.UseMesh, tag.Distance, tag.Node.EnglishName, tag.MovementStopCallback);
            }

            return result;
        }

        public override string ToString()
        {
            return string.Format(
                "StealthApproachGatherSpot -> StealthLocation: {0}, NodeLocation: {1}, ReturnToStealthLocation: {2}, UseMesh: {3}",
                this.StealthLocation,
                this.NodeLocation,
                this.ReturnToStealthLocation,
                this.UseMesh);
        }
    }
}