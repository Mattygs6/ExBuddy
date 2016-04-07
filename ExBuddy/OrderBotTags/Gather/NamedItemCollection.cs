namespace ExBuddy.OrderBotTags.Gather
{
	using System.Collections;
	using System.Collections.Generic;
	using Clio.XmlEngine;
	using ExBuddy.Interfaces;

	[XmlElement("Items")]
	public class NamedItemCollection : IList<IConditionNamedItem>
	{
		        
		[XmlElement(XmlEngine.GENERIC_BODY)]
		private List<IConditionNamedItem> Items { get; set; }

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion

		#region IEnumerable<IConditionNamedItem> Members

		public IEnumerator<IConditionNamedItem> GetEnumerator()
		{
			return Items.GetEnumerator();
		}

		#endregion

		#region ICollection<IConditionNamedItem> Members

		public int Count
		{
			get { return Items.Count; }
		}

		public bool IsReadOnly
		{
			get { return false; }
		}

		public void Add(IConditionNamedItem item)
		{
			Items.Add(item);
		}

		public void Clear()
		{
			Items.Clear();
		}

		public bool Contains(IConditionNamedItem item)
		{
			return Items.Contains(item);
		}

		public void CopyTo(IConditionNamedItem[] array, int arrayIndex)
		{
			Items.CopyTo(array, arrayIndex);
		}

		public bool Remove(IConditionNamedItem item)
		{
			return Items.Remove(item);
		}

		#endregion

		#region IList<IConditionNamedItem> Members

		public int IndexOf(IConditionNamedItem item)
		{
			return Items.IndexOf(item);
		}

		public void Insert(int index, IConditionNamedItem item)
		{
			Items.Insert(index, item);
		}

		public IConditionNamedItem this[int index]
		{
			get { return Items[index]; }
			set { Items[index] = value; }
		}

		public void RemoveAt(int index)
		{
			Items.RemoveAt(index);
		}

		#endregion
	}
}
