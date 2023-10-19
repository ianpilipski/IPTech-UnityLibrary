using IPTech.DialogManager.Api;
using strange.extensions.mediation.api;
using strange.extensions.signal.impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPTech.DialogManager.Strange.Api
{
	public interface IDialogWindowView : IView
	{
		Signal ClickedCancelSignal { get; }
		Signal ClickedOkSignal { get; }
		void Dismiss();

		void Show();
		void Hide();

		IDialogWindowView Clone(IDialogOptions dialogOptions);

		int ID { get; }
		string Message { get; set; }
		bool AllowCancel { get; }
		Signal ConfirmSignal { get; }
		Signal CancelSignal { get; }
	}

}
