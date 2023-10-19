using IPTech.DialogManager.Api;
using IPTech.DialogManager.Strange.Api;
using strange.extensions.mediation.api;
using strange.extensions.mediation.impl;
using strange.extensions.signal.impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace IPTech.DialogManager.Strange
{
	[MediateAs(typeof(IDialogWindowView))]
	class DialogWindowBasicView : View, IDialogWindowView {
		private bool allowCancel;
		
		public int DialogID = -1;
		public Text text = null;

		public GameObject ConfirmationOnlyObject = null;
		public GameObject ConfirmationWithCancelObject = null;

		public Signal ClickedCancelSignal { get; private set; }
		public Signal ClickedOkSignal { get; private set; }

		public bool AllowCancel {
			get {
				return this.allowCancel;
			}
			private set {
				SetAllowCancel(value);
			}
		}

		public string Message {
			get {
				return this.text.text;
			}

			set {
				this.text.text = value;
			}
		}

		public int ID {
			get {
				return this.DialogID;
			}
			private set {
				this.DialogID = value;
			}
		}

		public Signal ConfirmSignal { get; private set; }
		public Signal CancelSignal { get; private set; }

		public DialogWindowBasicView() {
			this.ClickedCancelSignal = new Signal();
			this.ClickedOkSignal = new Signal();
			this.autoRegisterWithContext = false;
		}

		protected override void Awake() {
			//base.Awake();
			this.gameObject.SetActive(false);
		}

		void OnEnable() { 
			this.transform.SetAsLastSibling();
		}

		protected override void Start() {
			base.Start();
			RecalculateLayout();
		}

		public void Hide() {
			this.gameObject.SetActive(false);
		}

		public void Show() {
			this.gameObject.SetActive(true);
		}

		public void OnClickedOk() {
			this.ClickedOkSignal.Dispatch();
		}

		public void OnClickedCancel() {
			if (this.allowCancel) {
				this.ClickedCancelSignal.Dispatch();
			}
		}

		public void Dismiss() {
			Destroy(this.gameObject);
		}

		IDialogWindowView IDialogWindowView.Clone(IDialogOptions dialogOptions) {
			DialogWindowBasicView newDialogWindowBasicView = Instantiate<DialogWindowBasicView>(this);
			newDialogWindowBasicView.transform.SetParent(this.transform.parent, false);
			newDialogWindowBasicView.gameObject.SetActive(true);

			newDialogWindowBasicView.ID = dialogOptions.ID;
			newDialogWindowBasicView.Message = dialogOptions.Message;
			newDialogWindowBasicView.AllowCancel = dialogOptions.AllowCancel;
			newDialogWindowBasicView.ConfirmSignal = dialogOptions.ConfirmSignal;
			newDialogWindowBasicView.CancelSignal = dialogOptions.CancelSignal;

			newDialogWindowBasicView.bubbleToContext(newDialogWindowBasicView, true, false);
			return (IDialogWindowView)newDialogWindowBasicView;
		}

		private void SetAllowCancel(bool newValue) {
			if(this.allowCancel!=newValue) {
				this.allowCancel = newValue;
				RecalculateLayout();
			}
		}

		private void RecalculateLayout() {
			if(this.allowCancel) {
				HideConfirmationOnly();
				ShowConfirmationWithCancel();
			} else {
				HideConfirmationWithCancel();
				ShowConfirmationOnly();
			}
		}

		private void HideConfirmationOnly() {
			this.ConfirmationOnlyObject.SetActive(false);
		}

		private void HideConfirmationWithCancel() {
			this.ConfirmationWithCancelObject.SetActive(false);
		}

		private void ShowConfirmationOnly() {
			this.ConfirmationOnlyObject.SetActive(true);
		}

		private void ShowConfirmationWithCancel() {
			this.ConfirmationWithCancelObject.SetActive(true);
		}
	}
}
