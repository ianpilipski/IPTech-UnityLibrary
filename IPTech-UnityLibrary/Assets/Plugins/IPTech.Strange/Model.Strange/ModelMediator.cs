using IPTech.Model.Api;
using IPTech.Model.Strange.Api;
using strange.extensions.injector.api;
using strange.extensions.mediation.impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPTech.Model.Strange
{
	/*
	 * NOTES
	 * 
	 * I'm using c# events from IModelManager directly to drive updates through this mediator.
	 * The view itself is also using events to drive property changes.
	 * This may not be the "Strange" way to do it as it prefers signals over events
	 * to keep the separation clean.  However an interface contract seems simlar to a signal contract
	 * to me?  Maybe not, need to think about this more.
	 * 
	 * Is there a benefit to using signals over interface?
	 */

	public class ModelMediator : Mediator
	{
		[Inject]
		public IModelView modelView { get; set; }

		[Inject]
		public IModelManager modelManager { get; set; }

		public override void OnRegister() {
			base.OnRegister();
			this.modelManager.ModelManagerUpdated += ModelManagerUpdatedHandler;
			this.modelView.ModelIDChangedSignal.AddListener(ModelIDChangedHandler);
			UpdateViewWithProperModel();
		}

		private void UpdateViewWithProperModel() {
			if(this.modelManager.Contains(this.modelView.ModelID)) {
				SetModel(this.modelManager[this.modelView.ModelID]);
			}
		}

		private void SetModel(IModel model) {
			this.modelView.Model = model;
		}

		public override void OnRemove() {
			this.modelManager.ModelManagerUpdated -= ModelManagerUpdatedHandler;
			this.modelView.ModelIDChangedSignal.RemoveListener(ModelIDChangedHandler);
			base.OnRemove();
		}

		private void ModelManagerUpdatedHandler(object sender, ModelManagerUpdatedEventArgs modelManagerUpdatedEventArgs) {
			if(modelManagerUpdatedEventArgs.UpdateType == ModelManagerUpdateType.ModelAdded) {
				IModel model = modelManagerUpdatedEventArgs.Model;
				if(model.ModelID == this.modelView.ModelID) {
					SetModel(model);
				}
			} else if(modelManagerUpdatedEventArgs.UpdateType == ModelManagerUpdateType.ModelRemoved) {
				IModel model = modelManagerUpdatedEventArgs.Model;
				if(model.ModelID == this.modelView.ModelID) {
					SetModel(null);
				}
			}
		}

		private void ModelIDChangedHandler() {
			UpdateViewWithProperModel();
		}
	}
}
