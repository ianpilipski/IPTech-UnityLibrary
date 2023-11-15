using IPTech.PlayerInventory.Strange.Api;
using strange.extensions.mediation.impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IPTech.PlayerInventory.Api;

namespace IPTech.PlayerInventory.Strange
{
	public abstract class InventoryCollectionView : View, IInventoryCollectionView
	{
		protected IInventoryCollection inventoryCollection { get; private set; }
		protected long totalAmount { get; private set; }
		protected int totalCount { get; private set; }

		public string InventoryItemID = string.Empty;
		public string collectionName = string.Empty;

		public string CollectionName {
			get { return this.collectionName; }
		}

		protected override void Start() {
			base.Start();
			InternalUpdate();
		}

		public void UpdateFor(IInventoryCollection inventoryCollection) {
			this.inventoryCollection = inventoryCollection;
			InternalUpdate();
		}

		private void InternalUpdate() {
			RecalculateTotalAmount();
			UpdateVisuals();
		}

		protected abstract void UpdateVisuals();

		private void RecalculateTotalAmount() {
			this.totalAmount = 0;
			this.totalCount = 0;
			if (this.inventoryCollection != null) {
				bool showAll = string.IsNullOrEmpty(this.InventoryItemID);
				foreach (IInventoryItem item in this.inventoryCollection) {
					if (showAll || item.IsInventoryTypeOf(this.InventoryItemID)) {
						this.totalAmount += item.Amount;
						this.totalCount++;
					}
				}
			}
		}
	}
}
