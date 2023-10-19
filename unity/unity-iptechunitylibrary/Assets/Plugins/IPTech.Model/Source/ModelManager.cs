using IPTech.Model.Api;
//using IPTech.Model.Strange.Api;
using IPTech.Types;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace IPTech.Model
{
	public class ModelManager : Collection<IModel>, IModelManager
	{
		public event ModelManagerUpdatedDelegate ModelManagerUpdated;

		public IModel this[RName modelID] {
			get {
				if (this.Count > 0) {
					IModel model = this.First(m => m.ModelID == modelID);
					if (model != null) {
						return model;
					}
				}
				throw new KeyNotFoundException(modelID.ToString());
			}
		}

		protected override void InsertItem(int index, IModel item) {
			base.InsertItem(index, item);
			ModelAdded(item);
		}

		protected override void SetItem(int index, IModel item) {
			IModel model = this[index];
			base.SetItem(index, item);
			if(model!=item) {
				ModelAdded(item);
				ModelRemoved(model);
			}
		}

		protected override void RemoveItem(int index) {
			IModel model = this[index];
			base.RemoveItem(index);
			ModelRemoved(model);
		}

		protected override void ClearItems() {
			IModel[] models = this.Distinct().ToArray();
			base.ClearItems();
			foreach(IModel model in models) {
				ModelRemoved(model);
			}
		}

		private void ModelAdded(IModel model) {
			if(model!=null && this.Where(m => m==model).Count()==1) {
				OnModelManagerUpdated(ModelManagerUpdateType.ModelAdded, model);
			}
		}

		private void ModelRemoved(IModel model) {
			if(model!=null && this.Where(m=>m==model).Count()==0) {
				OnModelManagerUpdated(ModelManagerUpdateType.ModelRemoved, model);
			}
		}

		protected virtual void OnModelManagerUpdated(ModelManagerUpdateType updateType, IModel model) {
			if(this.ModelManagerUpdated!=null) {
				ModelManagerUpdatedEventArgs modelManagerUpdatedEventArgs = new ModelManagerUpdatedEventArgs(updateType, model);
				this.ModelManagerUpdated(this, modelManagerUpdatedEventArgs);
			}
		}

		public bool Contains(RName modelID) {
			if (this.Count > 0) {
				return this.Any(m => m.ModelID == modelID);
			}
			return false;
		}
	}
}
