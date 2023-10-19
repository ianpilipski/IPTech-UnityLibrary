using strange.extensions.signal.impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPTech.DialogManager.Strange.Api
{
	public interface IDialogOptions
	{
		string Message { get; }
		int ID { get; }
		bool AllowCancel { get; }

		Signal ConfirmSignal { get; }
		Signal CancelSignal { get; }
	}
}
