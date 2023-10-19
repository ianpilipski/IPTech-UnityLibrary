using IPTech.PlayerInventory.Strange.Api;
using strange.extensions.mediation.api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;

namespace IPTech.PlayerInventory.Strange
{
	[MediateAs(typeof(IInventoryItemView))]
	public class InventoryItemViewCurrency : InventoryItemView
	{
		public Text text = null;

		private void InitializeTextReference() {
			if (this.text == null) {
				this.text = this.GetComponent<Text>();
			}
		}

		protected override void UpdateVisuals() {
            InitializeTextReference();
			this.text.text = string.Format("{0:C}", this.currentAmount / 100F);
		}
	}
}
