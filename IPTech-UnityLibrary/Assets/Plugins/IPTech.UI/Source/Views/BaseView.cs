using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace IPTech.UI {
	public class BaseView : MonoBehaviour, IView {
		protected IViewManager _viewManager;

		public string ViewId;

		string IView.ViewId {
			get { return string.IsNullOrEmpty(ViewId) ? this.GetType().Name : ViewId; }
		}

		public event Action<IView> ViewDestroyed;
		public event Action<IView> ViewShown;

		virtual protected void Awake() {
			if(_viewManager == null) {
				_viewManager = ViewManagerLocator.Locate(this);
			}
			_viewManager.RegisterView(this);
		}

		virtual public void AwakeView() { }

		void OnValidate() {
			if(!string.IsNullOrEmpty(ViewId)) {
				if(string.IsNullOrEmpty(ViewId.Trim())) {
					ViewId = null;
				}
			}
		}

		protected virtual void OnDestroy() {
			ViewDestroyed?.Invoke(this);
		}

		public bool IsVisible => gameObject.activeInHierarchy;

		public void Hide() {
			gameObject.SetActive(false);
		}

		public void Show() {
			gameObject.SetActive(true);
			ViewShown?.Invoke(this);
		}
	}
}
