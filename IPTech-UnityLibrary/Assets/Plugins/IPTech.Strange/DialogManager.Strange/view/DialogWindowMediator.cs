using IPTech.DialogManager.Api;
using IPTech.DialogManager.Strange.Api;
using strange.extensions.mediation.impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPTech.DialogManager.Strange
{
	class DialogWindowMediator : Mediator, IDialog
	{
		[Inject]
		public IDialogWindowView dialogWindowView { get; set; }

		public int ID {
			get {
				return this.dialogWindowView.ID;
			}
		}

		public event EventHandler Closed;

		public override void OnRegister() {
			base.OnRegister();
			this.dialogWindowView.ClickedCancelSignal.AddListener(ClickedCancelSignalHandler);
			this.dialogWindowView.ClickedOkSignal.AddListener(ClickedOkSignalHandler);
		}

		public override void OnRemove() {
			this.dialogWindowView.ClickedCancelSignal.RemoveListener(ClickedCancelSignalHandler);
			this.dialogWindowView.ClickedOkSignal.RemoveListener(ClickedOkSignalHandler);
			base.OnRemove();
		}

		protected void ClickedOkSignalHandler() {
			Hide();
			if (this.dialogWindowView.ConfirmSignal!=null) {
				this.dialogWindowView.ConfirmSignal.Dispatch();
			}
			DismissDialog();
		}

		protected void ClickedCancelSignalHandler() {
			if (this.dialogWindowView.AllowCancel) {
				Hide();
				if(this.dialogWindowView.CancelSignal!=null) {
					this.dialogWindowView.CancelSignal.Dispatch();
				}
				DismissDialog();
			}
		}

		public void Show(ShowType showType) {
			this.dialogWindowView.Show();
		}

		public void Hide() {
			this.dialogWindowView.Hide();
		}

		protected void DismissDialog() {
			this.dialogWindowView.Dismiss();
			OnClosed();
		}

		protected void OnClosed() {
			if(this.Closed!=null) {
				this.Closed(this, EventArgs.Empty);
			}
		}
	}
}
