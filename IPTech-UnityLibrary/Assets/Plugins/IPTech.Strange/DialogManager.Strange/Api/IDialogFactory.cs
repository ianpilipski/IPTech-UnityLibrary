using IPTech.DialogManager.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPTech.DialogManager.Strange.Api
{
	public interface IDialogFactory
	{
		IDialog CreateBasicDialog(IDialogOptions dialogOptions);
	}
}
