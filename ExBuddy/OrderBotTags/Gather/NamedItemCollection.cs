namespace ExBuddy.OrderBotTags.Gather
{
	using System.Collections;
	using System.Collections.Generic;

	using Clio.Utilities;
	using Clio.XmlEngine;

	using ExBuddy.Interfaces;

	public class NamedItemCollection : IList<INamedItem>
	{
		public NamedItemCollection()
		{
			Items = new List<INamedItem>();
		}

		[XmlElement(XmlEngine.GENERIC_BODY)]
		private List<INamedItem> Items { get; [UsedImplicitly] set; }

		#region ICollection<NamedItem> Members

		public int Count
		{
			get
			{
				return Items.Count;
			}
		}

		public bool IsReadOnly
		{
			get
			{
				return false;
			}
		}

		public void Add(INamedItem item)
		{
			Items.Add(item);
		}

		public void Clear()
		{
			Items.Clear();
		}

		public bool Contains(INamedItem item)
		{
			return Items.Contains(item);
		}

		public void CopyTo(INamedItem[] array, int arrayIndex)
		{
			Items.CopyTo(array, arrayIndex);
		}

		public bool Remove(INamedItem item)
		{
			return Items.Remove(item);
		}

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion

		#region IEnumerable<NamedItem> Members

		public IEnumerator<INamedItem> GetEnumerator()
		{
			return Items.GetEnumerator();
		}

		#endregion

		#region IList<NamedItem> Members

		public int IndexOf(INamedItem item)
		{
			return Items.IndexOf(item);
		}

		public void Insert(int index, INamedItem item)
		{
			Items.Insert(index, item);
		}

		public INamedItem this[int index]
		{
			get
			{
				return Items[index];
			}
			set
			{
				Items[index] = value;
			}
		}

		public void RemoveAt(int index)
		{
			Items.RemoveAt(index);
		}

		#endregion
	}
}
