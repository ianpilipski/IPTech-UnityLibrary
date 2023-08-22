using IPTech.DialogManager.Api;
using IPTech.DialogManager.Strange.Api;
using strange.extensions.context.api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPTech.DialogManager.Strange
{
	class DialogFactory : IDialogFactory
	{
		[Inject(name="BASICDIALOG")]
		public IDialogWindowView basicDialogTemplate { get; set; }

		public IDialog CreateBasicDialog(IDialogOptions dialogOptions) {
			IDialogWindowView dialogWindowBasic = (IDialogWindowView)this.basicDialogTemplate.Clone(dialogOptions);
			return (IDialog)dialogWindowBasic.GetMediatorComponent(typeof(IDialog));
		}
	}
}
