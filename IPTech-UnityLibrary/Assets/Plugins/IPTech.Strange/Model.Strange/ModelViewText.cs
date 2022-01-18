using IPTech.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;
using IPTech.Model.Api;

namespace IPTech.Model.Strange
{
	public class ModelViewText : ModelView
	{
		const string NOTFOUND = "<notfound>";
		private List<object> values = new List<object>();
		private RName[] PropertyRNames;

		public Text text = null;
		public string FormatString = "{0}";
		public string[] PropertyNames = new string[0];

		protected override void Awake() {
			UpdatePropertyRNames();
			base.Awake();
		}

		private void UpdatePropertyRNames() {
			int i = 0;
			this.PropertyRNames = new RName[this.PropertyNames.Length];
			foreach (string propertyName in this.PropertyNames) {
				this.PropertyRNames[i++] = RName.Name(propertyName);
			}
		}

		public override void Refresh() {
			values.Clear();
			foreach(RName propRName in this.PropertyRNames) {
				if(this.Model.Contains(propRName)) {
					values.Add(this.Model[propRName]);
				} else {
					values.Add(NOTFOUND);
				}
			}

			this.text.text = string.Format(this.FormatString, values.ToArray());
		}

		protected override void ModelPropertyChanged(ModelChangedEventArgs e) {
			base.ModelPropertyChanged(e);
			foreach(IModelProperty changedProperty in e.ChangedProperties) {
				if(this.PropertyRNames.Contains(changedProperty.PropertyName)) {
					Refresh();
					return;
				}
			}
		}
	}
}
