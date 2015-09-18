namespace ExBuddy.OrderBotTags.Behaviors
{
    using System.ComponentModel;

    using Clio.XmlEngine;

    using ExBuddy.Interfaces;

    public abstract class FlightVars : ExProfileBehavior, IFlightVars
    {
        [DefaultValue(3.0f)]
        [XmlAttribute("Radius")]
        public float Radius { get; set; }

        [DefaultValue(6)]
        [XmlAttribute("InverseParabolicMagnitude")]
        public int InverseParabolicMagnitude { get; set; }

        [DefaultValue(0.05f)]
        [XmlAttribute("Smoothing")]
        public float Smoothing { get; set; }

        [DefaultValue(0)]
        [XmlAttribute("MountId")]
        public int MountId { get; set; }

        [DefaultValue(3.0f)]
        [XmlAttribute("ForcedAltitude")]
        public float ForcedAltitude { get; set; }

        [XmlAttribute("ForceLanding")]
        public bool ForceLanding { get; set; }

        [XmlAttribute("LogWaypoints")]
        public bool LogWaypoints { get; set; }
    }
}
