using IPTech.DialogManager.Api;
using IPTech.DialogManager.Strange.Api;
using strange.extensions.command.impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPTech.DialogManager.Strange
{
	class ShowDialogWindowCommand : Command {

		[Inject]
		public IDialogOptions dialogOptions { get; set; }

		[Inject]
		public IDialogManager dialogManager { get; set; }

		[Inject]
		public IDialogFactory dialogFactory { get; set; }

		public override void Execute() {
			IDialog dialog = new DialogWrapper(this.dialogOptions, this.dialogFactory);
			this.dialogManager.EnqueueDialog(dialog);
		}
	}
}
