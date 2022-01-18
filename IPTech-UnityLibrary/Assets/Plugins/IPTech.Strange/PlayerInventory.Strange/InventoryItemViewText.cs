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
	public class InventoryItemViewText : InventoryItemView
	{
		public Text text = null;

		protected override void Awake() {
			InitializeTextReference();
			base.Awake();
		}

		private void InitializeTextReference() {
			if (this.text == null) {
				this.text = this.GetComponent<Text>();
			}
		}

		protected override void UpdateVisuals() {
			this.text.text = this.currentAmount.ToString();
		}
	}
}
