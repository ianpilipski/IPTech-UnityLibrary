using IPTech.PlayerInventory.Api;
using IPTech.PlayerInventory.Strange.Api;
using strange.extensions.mediation.api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace IPTech.PlayerInventory.Strange
{
	[MediateAs(typeof(IInventoryCollectionView))]
	class InventoryCollectionViewList : InventoryCollectionView
	{
		private IList<InventoryItemView> itemViews;

		public InventoryItemView templateView = null;

		public InventoryCollectionViewList() {
			this.itemViews = new List<InventoryItemView>();
		}

		protected override void Awake() {
			this.templateView.enabled = false;
			this.templateView.gameObject.SetActive(false);
			base.Awake();
		}

		protected override void UpdateVisuals() {
			if(this.inventoryCollection==null) {
				return;
			}

			int i = 0;
			foreach(IInventoryItem item in this.inventoryCollection) {
				InventoryItemView itemView = null;
				if (this.itemViews.Count>i) {
					itemView = this.itemViews[i];	
				} else {
					itemView = CreateNewItemViewFromTemplate(i);
					this.itemViews.Add(itemView);
				}
				itemView.SetInventoryItemID(item.InventoryItemID);
				i++;
			}
		}

		protected virtual InventoryItemView CreateNewItemViewFromTemplate(int index) {
			Vector3 position;
			Quaternion rotation;
			Transform parent;
			GetNewItemPlacement(out position, out rotation, out parent);
			InventoryItemView itemView = (InventoryItemView)Instantiate(this.templateView, position, rotation);
			itemView.transform.SetParent(parent, true);
			itemView.enabled = true;
			itemView.gameObject.SetActive(true);
			return itemView;
		}

		protected virtual void GetNewItemPlacement(out Vector3 position, out Quaternion rotation, out Transform parent) {
			position = Vector3.zero;
			rotation = Quaternion.identity;
			parent = this.transform;
		}
	}
}
