using IPTech.DialogManager.Api;
using IPTech.DialogManager.Strange;
using IPTech.DialogManager.Strange.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPTech.DialogManager
{
	class DialogWrapper : IDialog
	{
		private IDialogFactory dialogFactory;
		private IDialog innerDialog;
		private IDialogOptions dialogOptions;

		public int ID {
			get {
				return this.dialogOptions.ID;
			}
		}

		public event EventHandler Closed;

		public void Hide() {
			if(this.innerDialog!=null) {
				this.innerDialog.Hide();
			}
		}

		public void Show(ShowType showType = ShowType.FirstOpen) {
			if(this.innerDialog==null) {
				this.innerDialog = this.dialogFactory.CreateBasicDialog(this.dialogOptions);
				this.innerDialog.Closed += ClosedHandler;
			}
			this.innerDialog.Show(showType);
		}

		private void ClosedHandler(object sender, EventArgs e) {
			if(this.Closed!=null) {
				this.Closed(this, e);
			}
		}

		public DialogWrapper(IDialogOptions dialogOptions, IDialogFactory factory) {
			this.dialogOptions = dialogOptions;
			this.dialogFactory = factory;
		}
	}
}
