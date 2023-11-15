using IPTech.PlayerInventory.Strange.Api;
using strange.extensions.mediation.api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;

namespace IPTech.PlayerInventory.Strange
{
	[MediateAs(typeof(IInventoryCollectionView))]
	public class InventoryCollectionViewAmountText : InventoryCollectionView
	{
		public Text text = null;

		protected override void Awake() {
			InitializeTextReference();
			base.Awake();
		}

		protected override void Start() {
			InitializeTextReference();
			base.Start();
		}

		private void InitializeTextReference() {
			if (this.text == null) {
				this.text = this.GetComponent<Text>();
			}
		}

		protected override void UpdateVisuals() {
			if(this.text!=null) {
				this.text.text = this.totalAmount.ToString();
			}
		}
	}
}
