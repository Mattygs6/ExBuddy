namespace ExBuddy.OrderBotTags
{
    using Clio.Utilities;
    using Clio.XmlEngine;

    [XmlElement("FishSpot")]
    public class FishSpot
    {
        public FishSpot()
        {
            XYZ = Vector3.Zero;
            Heading = 0f;
        }

        public FishSpot(string xyz, float heading)
        {
            XYZ = new Vector3(xyz);
            Heading = heading;
        }

        public FishSpot(Vector3 xyz, float heading)
        {
            XYZ = xyz;
            Heading = heading;
        }

        [XmlAttribute("XYZ")]
        public Vector3 XYZ { get; set; }

        [XmlAttribute("Heading")]
        public float Heading { get; set; }

        [XmlAttribute("Sit")]
        public bool Sit { get; set; }

        public override string ToString()
        {
            var ret = "[FishSpot] Location: " + XYZ + ", Heading: " + Heading;

            return ret;
        }
    }
}