using IPTech.Model.Strange.Api;
using strange.extensions.mediation.api;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace IPTech.Model.Strange
{
	[MediateAs(typeof(IModelView))]
	public class ModelViewCollection : ModelViewProperty
	{
		private IList<ModelView> modelViews;

		public ModelView templateView = null;

		public ModelViewCollection() {
			this.modelViews = new List<ModelView>();
		}

		protected override void Awake() {
			this.templateView.enabled = false;
			this.templateView.gameObject.SetActive(false);
			base.Awake();
		}

		public override void Refresh() {
			base.Refresh();

			IEnumerable e = this.propertyValue as IEnumerable;

			if(e==null) {
				throw new Exception("Property " + this.PropertyName + " of " + this.Model.TargetType().Name + " is not IEnumerable");
			}

			int i = 0;
			foreach (object value in e) {
				ModelView modelView = null;
				if (this.modelViews.Count > i) {
					modelView = this.modelViews[i];
				} else {
					modelView = CreateNewModelViewFromTemplate(i);
					this.modelViews.Add(modelView);
				}
				Model vModel = value as Model;
				if(vModel==null) {
					vModel = new Model(value);
				}
				modelView.Model = vModel;
				i++;
			}
		}

		protected virtual ModelView CreateNewModelViewFromTemplate(int index) {
			Vector3 position;
			Quaternion rotation;
			Transform parent;
			GetNewItemPlacement(out position, out rotation, out parent);
			ModelView view = (ModelView)Instantiate(this.templateView, position, rotation);
			view.transform.SetParent(parent, true);
			view.enabled = true;
			view.gameObject.SetActive(true);
			return view;
		}

		protected virtual void GetNewItemPlacement(out Vector3 position, out Quaternion rotation, out Transform parent) {
			position = Vector3.zero;
			rotation = Quaternion.identity;
			parent = this.transform;
		}
	}
}
