namespace ExBuddy.OrderBotTags.Common
{
    using System.ComponentModel;

    using Clio.XmlEngine;

    using ff14bot.NeoProfiles;

    public interface IFlightVars
    {
        float EdgeDetection { get; set; }

        float Radius { get; set; }

        int InverseParabolicMagnitude { get; set; }

        float Smoothing { get; set; }

        int MountId { get; set; }

        float NavHeight { get; set; }

        bool DismountAtDestination { get; set; }

        bool LogWaypoints { get; set; }
    }

    public abstract class FlightVars : ProfileBehavior, IFlightVars
    {
        [DefaultValue(50.0f)]
        [XmlAttribute("EdgeDetection")]
        public float EdgeDetection { get; set; }

        [DefaultValue(2.7f)]
        [XmlAttribute("Radius")]
        public float Radius { get; set; }

        [DefaultValue(10)]
        [XmlAttribute("InverseParabolicMagnitude")]
        public int InverseParabolicMagnitude { get; set; }

        [DefaultValue(0.1f)]
        [XmlAttribute("Smoothing")]
        public float Smoothing { get; set; }

        [DefaultValue(45)]
        [XmlAttribute("MountId")]
        public int MountId { get; set; }

        [DefaultValue(0.0f)]
        [XmlAttribute("NavHeight")]
        public float NavHeight { get; set; }

        [DefaultValue(true)]
        [XmlAttribute("DismountAtDestination")]
        public bool DismountAtDestination { get; set; }

        [XmlAttribute("LogWaypoints")]
        public bool LogWaypoints { get; set; }
    }
}
