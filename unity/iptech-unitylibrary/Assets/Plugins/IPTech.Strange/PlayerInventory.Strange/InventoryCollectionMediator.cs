using IPTech.PlayerInventory.Strange.Api;
using strange.extensions.mediation.impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IPTech.PlayerInventory.Api;
using strange.extensions.injector.impl;
using strange.extensions.injector.api;

namespace IPTech.PlayerInventory.Strange
{
	public class InventoryCollectionMediator : Mediator
	{
		[Inject]
		public IInventoryCollectionView inventoryCollectionView { get; set; }

		[Inject]
		public InventoryItemAmountChangedSignal inventoryItemAmountChangedSignal { get; set; }

		[Inject]
		public IInjectionBinder injectionBinder { get; set; }

		public IInventoryCollection inventoryCollection { get; set; }

		[PostConstruct]
		public void PostConstruct() {
			if (!string.IsNullOrEmpty(this.inventoryCollectionView.CollectionName)) {
				this.inventoryCollection = this.injectionBinder.GetInstance<IInventoryCollection>(this.inventoryCollectionView.CollectionName);
			} else {
				this.inventoryCollection = this.injectionBinder.GetInstance<IInventoryCollection>();
			}
		}

		public override void OnRegister() {
			base.OnRegister();
			this.inventoryItemAmountChangedSignal.AddListener(OnInventoryAmountChanged);
			this.inventoryCollectionView.UpdateFor(this.inventoryCollection);
		}

		public override void OnRemove() {
			this.inventoryItemAmountChangedSignal.RemoveListener(OnInventoryAmountChanged);
			base.OnRemove();
		}

		private void OnInventoryAmountChanged(IInventoryItem obj) {
			if(this.inventoryCollection.Contains(obj.InventoryItemID)) {
				this.inventoryCollectionView.UpdateFor(this.inventoryCollection);
			}
		}
	}
}
