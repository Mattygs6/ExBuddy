namespace ExBuddy.OrderBotTags.Gather
{
	using System.Collections;
	using System.Collections.Generic;

	using Clio.Utilities;
	using Clio.XmlEngine;

	using ExBuddy.Interfaces;

	[XmlElement("Items")]
	public class NamedItemCollection : IList<IConditionNamedItem>
	{
		public NamedItemCollection()
		{
			Items = new List<IConditionNamedItem>();
		}

        public IConditionNamedItem this[int index]
        {
            get
            {
                return ((IList<IConditionNamedItem>)Items)[index];
            }

            set
            {
                ((IList<IConditionNamedItem>)Items)[index] = value;
            }
        }

        public int Count
        {
            get
            {
                return ((IList<IConditionNamedItem>)Items).Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return ((IList<IConditionNamedItem>)Items).IsReadOnly;
            }
        }

        [XmlElement(XmlEngine.GENERIC_BODY)]
		private List<IConditionNamedItem> Items { get; [UsedImplicitly] set; }

        public void Add(IConditionNamedItem item)
        {
            ((IList<IConditionNamedItem>)Items).Add(item);
        }

        public void Clear()
        {
            ((IList<IConditionNamedItem>)Items).Clear();
        }

        public bool Contains(IConditionNamedItem item)
        {
            return ((IList<IConditionNamedItem>)Items).Contains(item);
        }

        public void CopyTo(IConditionNamedItem[] array, int arrayIndex)
        {
            ((IList<IConditionNamedItem>)Items).CopyTo(array, arrayIndex);
        }

        public IEnumerator<IConditionNamedItem> GetEnumerator()
        {
            return ((IList<IConditionNamedItem>)Items).GetEnumerator();
        }

        public int IndexOf(IConditionNamedItem item)
        {
            return ((IList<IConditionNamedItem>)Items).IndexOf(item);
        }

        public void Insert(int index, IConditionNamedItem item)
        {
            ((IList<IConditionNamedItem>)Items).Insert(index, item);
        }

        public bool Remove(IConditionNamedItem item)
        {
            return ((IList<IConditionNamedItem>)Items).Remove(item);
        }

        public void RemoveAt(int index)
        {
            ((IList<IConditionNamedItem>)Items).RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IList<IConditionNamedItem>)Items).GetEnumerator();
        }
    }
}
