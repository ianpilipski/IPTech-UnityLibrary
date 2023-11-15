using IPTech.PlayerInventory.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace IPTech.PlayerInventory
{
	[Serializable]
	class BasicInventoryCollection : IInventoryCollection
	{
		private IList<IInventoryItem> inventoryList;

		public BasicInventoryCollection() {
			this.inventoryList = new List<IInventoryItem>();
		}

		public IInventoryItem this[int index] {
			get {
				return this.inventoryList[index];
			}
		}

		public int Count {
			get {
				return this.inventoryList.Count;
			}
		}

		public string ID {
			get {
				throw new NotImplementedException();
			}

			set {
				throw new NotImplementedException();
			}
		}

		public bool IsReadOnly {
			get {
				return this.inventoryList.IsReadOnly;
			}
		}

		public void Add(IInventoryItem item) {
			this.inventoryList.Add(item);
		}

		public void Clear() {
			this.inventoryList.Clear();
		}

		public bool Contains(IInventoryItem item) {
			return this.inventoryList.Contains(item);
		}

		public bool Contains(string InventoryItemID) {
			return this.inventoryList.Any(item => item.IsInventoryTypeOf(InventoryItemID));
		}

		public void CopyTo(IInventoryItem[] array, int arrayIndex) {
			this.inventoryList.CopyTo(array, arrayIndex);
		}

		public bool TryFindInventoryForID(string inventoryID, out IInventoryItem inventoryFound) {
			foreach(IInventoryItem inventory in this.inventoryList) {
				if(inventory.InventoryItemID == inventoryID) {
					inventoryFound = inventory;
					return true;
				}
			}
			inventoryFound = null;
			return false;
		}

		public IEnumerator<IInventoryItem> GetEnumerator() {
			return this.inventoryList.GetEnumerator();
		}

		public bool Remove(IInventoryItem item) {
			return this.inventoryList.Remove(item);
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return this.inventoryList.GetEnumerator();
		}
	}
}
