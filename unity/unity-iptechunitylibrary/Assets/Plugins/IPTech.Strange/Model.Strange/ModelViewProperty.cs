using IPTech.Model.Strange.Api;
using strange.extensions.mediation.impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IPTech.Model.Api;
using IPTech.Types;

namespace IPTech.Model.Strange
{
	public class ModelViewProperty : ModelView
	{
		private RName propertyRName;
		protected object propertyValue { get; private set; }

		public string PropertyName = string.Empty;

		protected override void Awake() {
			UpdatePropertyRName();
			base.Awake();
		}

		private void UpdatePropertyRName() {
			this.propertyRName = RName.Name(this.PropertyName);
		}

		protected override void ModelPropertyChanged(ModelChangedEventArgs e) {
			base.ModelPropertyChanged(e);
			if(e.ChangedProperties.Contains(propertyRName)) {
				Refresh();
			}
		}

		public override void Refresh() {
			this.propertyValue = this.Model[this.propertyRName].CurrentValue;
		}
	}
}
