using IPTech.DialogManager.Strange.Api;
using strange.extensions.signal.impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPTech.DialogManager.Strange
{
	class DialogOptions : IDialogOptions
	{
		public bool AllowCancel { get; private set; }
		public int ID { get; private set; }
		public string Message { get; private set; }
		public Signal ConfirmSignal { get; private set; }
		public Signal CancelSignal { get; private set; }

		public DialogOptions(int dialogId, string message, bool allowCancel, Signal confirmSignal, Signal cancelSignal) {
			this.ID = dialogId;
			this.Message = message;
			this.AllowCancel = allowCancel;
			this.ConfirmSignal = confirmSignal;
			this.CancelSignal = cancelSignal;
		}
	}
}
