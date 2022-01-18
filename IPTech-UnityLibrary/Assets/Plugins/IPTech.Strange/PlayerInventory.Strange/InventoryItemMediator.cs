using strange.extensions.mediation.impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IPTech.PlayerInventory.Api;
using IPTech.PlayerInventory.Strange.Api;

namespace IPTech.PlayerInventory.Strange
{
	class InventoryItemMediator : Mediator
	{
		[Inject]
		public IInventoryItemView inventoryItemView { get; set; }

		[Inject]
		public InventoryItemAmountChangedSignal inventoryItemAmountChangedSignal { get; set; }
		 
		[Inject]
		public NeedInventoryAmountChangedSignal needInventoryAmountChangedSignal { get; set; }

		public override void OnRegister() {
			base.OnRegister();
			this.inventoryItemAmountChangedSignal.AddListener(InventoryItemAmountChangedHandler);
			this.inventoryItemView.InventoryItemIDChangedSignal.AddListener(ViewInventoryItemIDChangedHandler);
		}

		
		public override void OnRemove() {
			this.inventoryItemView.InventoryItemIDChangedSignal.RemoveListener(ViewInventoryItemIDChangedHandler);
			this.inventoryItemAmountChangedSignal.RemoveListener(InventoryItemAmountChangedHandler);
			base.OnRemove();
		}

		private void InventoryItemAmountChangedHandler(IInventoryItem obj) {
			this.inventoryItemView.SetAmount(obj);
		}

		private void ViewInventoryItemIDChangedHandler(string inventoryItemID) {
			this.needInventoryAmountChangedSignal.Dispatch(inventoryItemID);
		}

	}
}
