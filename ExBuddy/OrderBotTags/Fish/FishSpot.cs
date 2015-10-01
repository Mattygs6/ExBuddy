namespace ExBuddy.OrderBotTags.Fish
{
	using Clio.Utilities;
	using Clio.XmlEngine;

	[XmlElement("FishSpot")]
	public class FishSpot
	{
		public FishSpot()
		{
			Location = Vector3.Zero;
			Heading = 0f;
		}

		public FishSpot(string xyz, float heading)
		{
			Location = new Vector3(xyz);
			Heading = heading;
		}

		public FishSpot(Vector3 xyz, float heading)
		{
			Location = xyz;
			Heading = heading;
		}

		[XmlAttribute("Heading")]
		public float Heading { get; set; }

		[XmlAttribute("XYZ")]
		[XmlAttribute("Location")]
		public Vector3 Location { get; set; }

		[XmlAttribute("Sit")]
		public bool Sit { get; set; }

		public override string ToString()
		{
			return this.DynamicToString();
		}
	}
}