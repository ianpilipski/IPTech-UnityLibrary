using IPTech.Strange.Views.Api;
using strange.extensions.mediation.impl;

namespace IPTech.Strange.Views
{
	public abstract class AmountViewBase : View, IAmountView
	{
		private object targetAmount = null;
		public virtual object Amount {
			get {
				return this.targetAmount;
			}
			set {
				if (this.targetAmount==null || !this.targetAmount.Equals(value)) {
					this.targetAmount = value;
					UpdateVisuals();
				}
			}
		}

		protected abstract void UpdateVisuals();
	}
}
