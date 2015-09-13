namespace ExBuddy.OrderBotTags.Gather
{
    using System.ComponentModel;
    using System.Threading.Tasks;

    using Clio.Utilities;
    using Clio.XmlEngine;

    using ff14bot;

    public interface IGatherSpot
    {
        Vector3 NodeLocation { get; set; }

        Task<bool> MoveFromSpot(GatherCollectableTag tag);

        Task<bool> MoveToSpot(GatherCollectableTag tag);
    }

    [XmlElement("GatherSpot")]
    public class GatherSpot : StealthGatherSpot
    {
        public override async Task<bool> MoveToSpot(GatherCollectableTag tag)
        {
            var result = await Behaviors.MoveTo(NodeLocation, UseMesh, (uint)tag.MountId, tag.Distance, tag.Node.EnglishName, tag.MovementStopCallback);

            return result;
        }
    }

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
            var result = await Behaviors.MoveTo(NodeLocation, UseMesh, (uint)tag.MountId, tag.Distance, tag.Node.EnglishName, tag.MovementStopCallback, true);

            result &= await tag.CastAura(Ability.Stealth, AbilityAura.Stealth);

            return result;
        }

        public override string ToString()
        {
            return string.Format("GatherSpot -> NodeLocation: {0}, UseMesh: {1}", NodeLocation, UseMesh);
        }
    }

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
            if (ReturnToStealthLocation)
            {
                result &= await Behaviors.MoveToNoMount(StealthLocation, UseMesh, tag.Radius, tag.Node.EnglishName, tag.MovementStopCallback);
            }

            if (UnstealthAfter && Core.Player.HasAura((int)AbilityAura.Stealth))
            {
                result &= await tag.CastAura(Ability.Stealth);
            }

            return result;
        }

        public async Task<bool> MoveToSpot(GatherCollectableTag tag)
        {
            if (StealthLocation == Vector3.Zero)
            {
                return false;
            }

            var result = await Behaviors.MoveTo(StealthLocation, UseMesh, (uint)tag.MountId, tag.Radius, "Stealth Location", tag.MovementStopCallback, true);

            if (result)
            {
                await tag.CastAura(Ability.Stealth, AbilityAura.Stealth);

                result = await Behaviors.MoveToNoMount(NodeLocation, UseMesh, tag.Distance, tag.Node.EnglishName, tag.MovementStopCallback);
            }

            return result;
        }

        public override string ToString()
        {
            return string.Format(
                "StealthApproachGatherSpot -> StealthLocation: {0}, NodeLocation: {1}, ReturnToStealthLocation: {2}, UseMesh: {3}",
                StealthLocation,
                NodeLocation,
                ReturnToStealthLocation,
                UseMesh);
        }
    }
}
