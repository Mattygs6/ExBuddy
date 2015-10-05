namespace ExBuddy.OrderBotTags.Gather.GatherSpots
{
	using System.Collections;
	using System.Collections.Generic;

	using Clio.Utilities;
	using Clio.XmlEngine;

	using ExBuddy.Interfaces;

	public class GatherSpotCollection : IList<IGatherSpot>
	{
		public GatherSpotCollection()
		{
			Locations = new List<IGatherSpot>();
		}

		[XmlElement(XmlEngine.GENERIC_BODY)]
		private List<IGatherSpot> Locations { get; [UsedImplicitly] set; }

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

		public void Add(IGatherSpot item)
		{
			Locations.Add(item);
		}

		public void Clear()
		{
			Locations.Clear();
		}

		public bool Contains(IGatherSpot item)
		{
			return Locations.Contains(item);
		}

		public void CopyTo(IGatherSpot[] array, int arrayIndex)
		{
			Locations.CopyTo(array, arrayIndex);
		}

		public bool Remove(IGatherSpot item)
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

		public IEnumerator<IGatherSpot> GetEnumerator()
		{
			return Locations.GetEnumerator();
		}

		#endregion

		#region IList<GatherSpot> Members

		public int IndexOf(IGatherSpot item)
		{
			return Locations.IndexOf(item);
		}

		public void Insert(int index, IGatherSpot item)
		{
			Locations.Insert(index, item);
		}

		public IGatherSpot this[int index]
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