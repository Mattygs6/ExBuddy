namespace ExBuddy.OrderBotTags.Gather.GatherSpots
{
	using System.Collections;
	using System.Collections.Generic;

	using Clio.XmlEngine;

	[XmlElement("GatherSpots")]
	public class GatherSpotCollection : IList<GatherSpot>
	{
		[XmlElement(XmlEngine.GENERIC_BODY)]
		private List<GatherSpot> Locations { get; set; }

		#region ICollection<GatherSpot> Members

		public int Count
		{
			get
			{
				return Locations.Count;
			}
		}

		public bool IsReadOnly
		{
			get
			{
				return false;
			}
		}

		public void Add(GatherSpot item)
		{
			Locations.Add(item);
		}

		public void Clear()
		{
			Locations.Clear();
		}

		public bool Contains(GatherSpot item)
		{
			return Locations.Contains(item);
		}

		public void CopyTo(GatherSpot[] array, int arrayIndex)
		{
			Locations.CopyTo(array, arrayIndex);
		}

		public bool Remove(GatherSpot item)
		{
			return Locations.Remove(item);
		}

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion

		#region IEnumerable<GatherSpot> Members

		public IEnumerator<GatherSpot> GetEnumerator()
		{
			return Locations.GetEnumerator();
		}

		#endregion

		#region IList<GatherSpot> Members

		public int IndexOf(GatherSpot item)
		{
			return Locations.IndexOf(item);
		}

		public void Insert(int index, GatherSpot item)
		{
			Locations.Insert(index, item);
		}

		public GatherSpot this[int index]
		{
			get
			{
				return Locations[index];
			}
			set
			{
				Locations[index] = value;
			}
		}

		public void RemoveAt(int index)
		{
			Locations.RemoveAt(index);
		}

		#endregion
	}
}