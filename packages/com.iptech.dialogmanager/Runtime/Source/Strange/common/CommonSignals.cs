using IPTech.DialogManager.Strange.Api;
using strange.extensions.signal.impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPTech.DialogManager.Strange
{
	public class StartSignal : Signal { }

	public class ShowDialogSignal : Signal<IDialogOptions> {
		public void Dispatch(int ID, string message) {
			this.Dispatch(ID, message, null, false, null);
		}

		public void Dispatch(int ID, string message, Signal confirmSignal) {
			this.Dispatch(ID, message, confirmSignal, false, null);
		}

		public void Dispatch(int ID, string message, Signal confirmSignal, bool AllowCancel) {
			this.Dispatch(ID, message, confirmSignal, AllowCancel, null);
		}
		
		public void Dispatch(int ID, string message, Signal confirmSignal, bool AllowCancel, Signal cancelSignal) {
			IDialogOptions dialogOptions = new DialogOptions(ID, message, AllowCancel, confirmSignal, cancelSignal);
			this.Dispatch(dialogOptions);
		}
	}
}
