using UnityEngine.UI;

namespace IPTech.Strange.Views
{
	class AmountViewText : AmountViewBase
	{
		public Text text = null;

		protected override void Awake() {
			this.text = (this.text == null) ? GetComponent<Text>() : this.text;
			base.Awake();
		}

		protected override void UpdateVisuals() {
			this.text.text = this.Amount.ToString();
		}
	}
}
