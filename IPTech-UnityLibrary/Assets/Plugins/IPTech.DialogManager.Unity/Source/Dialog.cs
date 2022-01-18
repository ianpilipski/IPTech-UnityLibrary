using UnityEngine;
using System.Collections;
using IPTech.DialogManager.Api;
using UnityEngine.Events;
using System;

namespace IPTech.DialogManager.Unity {

	public class Dialog : MonoBehaviour, IDialog {
		bool alreadyCalledClose;

		public UnityEvent<ShowType> OnShow;
		public UnityEvent OnHide;
		public event EventHandler Closed;

		public void Hide() {
			OnHide.Invoke();
		}

		public void Show(ShowType showType) {
			alreadyCalledClose = false;
			OnShow.Invoke(showType);
		}

		public void Close() {
			if(alreadyCalledClose) return;

			alreadyCalledClose = true;
			Closed(this, new EventArgs());
		}

		void OnDestroy() {
			Close();
		}
	}
}
