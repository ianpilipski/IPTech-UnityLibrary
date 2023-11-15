using IPTech.Model.Strange.Api;
using strange.extensions.mediation.impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IPTech.Model.Api;
using strange.extensions.signal.impl;
using UnityEngine;
using IPTech.Types;

namespace IPTech.Model.Strange
{
	public abstract class ModelView : View, IModelView
	{
		private IModel _model;
		private Signal _modelIDChangedSignal = new Signal();

		[SerializeField]
		private RName _modelID;
		
		public Signal ModelIDChangedSignal {
			get {
				return this._modelIDChangedSignal;
			}
		}

		public RName ModelID {
			get { return this._modelID; }
			set {
				if(this._modelID!=value) {
					this._modelID = value;
					ModelIDChanged();
				}
			}
		}

		public IModel Model {
			get {
				return this._model;
			}
			set {
				if (this._model != value) {
					UnregisterModelEvents();
					this._model = value;
					RegisterModelEvents();
					Refresh();
				}
			}
		}

		private void UnregisterModelEvents() {
			this._model.ModelChanged -= ModelChangedHandler;
		}

		private void RegisterModelEvents() {
			this._model.ModelChanged += ModelChangedHandler;
		}

		private void ModelChangedHandler(object sender, ModelChangedEventArgs e) {
			ModelPropertyChanged(e);
		}

		private void ModelIDChanged() {
			this._modelIDChangedSignal.Dispatch();
		}
		
		protected virtual void ModelPropertyChanged(ModelChangedEventArgs e) {

		}

		public abstract void Refresh();
	}
}
