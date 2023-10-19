using IPTech.DialogManager.Api;
using System;
using System.Collections.Generic;
using IPTech;
using System.Linq;
using System.Text;

namespace IPTech.DialogManager
{
	public class DialogManagerBasic : IDialogManager
	{
		private List<DialogItem> dialogWindowList;
		
		private DialogItem currentlyActiveDialogWindow;

		public DialogManagerBasic() {
			this.dialogWindowList = new List<DialogItem>();
		}

		public void EnqueueDialog(IDialog dialogWindow) {
            //Debug.Assert(dialogWindow != null);
			if(IsAlreadyRegistered(dialogWindow)) {
				return;
			}
			ListEnqueu(dialogWindow);
			ProcessDisplayNextWindow();
		}

		public void ShowDialog(IDialog dialogWindow) {
			//Debug.Assert(dialogWindow != null);
			
			if(IsCurrentlyShowingADialog()) {
				if(IsCurrentlyShowing(dialogWindow)) return;
				
				PushCurrentlyShowingDialogToTopOfList();
				ClearCurrentlyShowingDialogWindow();
			}

			if(IsAlreadyRegistered(dialogWindow)) {
				ListMoveToTop(dialogWindow);
			} else {
				ListPush(dialogWindow);
			}
			ProcessDisplayNextWindow();
		}

		private bool IsAlreadyRegistered(IDialog dialogWindow) {
			bool inQueue = this.dialogWindowList.Any(d => d.IsDialog(dialogWindow));
			return inQueue || IsCurrentlyShowing(dialogWindow);
		}
		
		private bool IsCurrentlyShowing(IDialog dialogWindow) {
			return this.currentlyActiveDialogWindow!=null && this.currentlyActiveDialogWindow.IsDialog(dialogWindow);
		}

		private void PushCurrentlyShowingDialogToTopOfList() {
			if (this.currentlyActiveDialogWindow != null) {
				ListPush(this.currentlyActiveDialogWindow);
			}
		}

		private void ProcessDisplayNextWindow() {
			if(!IsCurrentlyShowingADialog()) {
				ShowNextDialogInQueue();
			}
		}

		private bool IsCurrentlyShowingADialog() {
			return this.currentlyActiveDialogWindow != null;
		}

		private void ShowNextDialogInQueue() {
            //Debug.Assert(this.currentlyActiveDialogWindow == null);
			if (this.dialogWindowList.Count > 0) {
				Show(ListPop());
			}
		}

		private void Show(DialogItem dialogItem) {
			this.currentlyActiveDialogWindow = dialogItem;
			this.currentlyActiveDialogWindow.Show();
		}

		private void CurrentlyActiveDialogWindow_Closed(DialogItem dialogItem) {
			HandleDialogWindowClosed(dialogItem);
		}

		private void HandleDialogWindowClosed(DialogItem dialogItem) { 
			dialogItem.Closed -= CurrentlyActiveDialogWindow_Closed;
			if (this.currentlyActiveDialogWindow == dialogItem) {
				ClearCurrentlyShowingDialogWindow();
				ProcessDisplayNextWindow();
			} else {
				EnsureDialogWindowIsRemovedFromQueue(dialogItem);
			}
		}

		private void EnsureDialogWindowIsRemovedFromQueue(DialogItem dialogItem) {
			this.dialogWindowList = new List<DialogItem>(this.dialogWindowList.Where(item => item != dialogItem));
		}

		private void ClearCurrentlyShowingDialogWindow() {
			if(this.currentlyActiveDialogWindow!=null) {
				this.currentlyActiveDialogWindow.Hide();
			}
			this.currentlyActiveDialogWindow = null;
		}
		
		void ListPush(IDialog dialog) {
			this.dialogWindowList.Insert(0, CreateDialogItem(dialog));
		}
		
		void ListPush(DialogItem dialogItem) {
			this.dialogWindowList.Insert(0, dialogItem);
		}
		
		DialogItem ListPop() {
			DialogItem retVal = this.dialogWindowList[0];
			this.dialogWindowList.RemoveAt(0);
			return retVal;
		}
		
		void ListEnqueu(IDialog dialog) {
			this.dialogWindowList.Add(CreateDialogItem(dialog));
		}
		
		void ListMoveToTop(IDialog dialog) {
			DialogItem di = this.dialogWindowList.Find( d => d.IsDialog(dialog));
			this.dialogWindowList.Remove(di);
			this.dialogWindowList.Insert(0, di);
		}
		
		DialogItem CreateDialogItem(IDialog dialog) {
			DialogItem dialogItem = new DialogItem(dialog);
			dialogItem.Closed += HandleDialogWindowClosed;
			return dialogItem;
		}
		
		
		class DialogItem {
			readonly IDialog dialog;
			public bool AlreadyShownOneTime { get; private set; }
			public Action<DialogItem> Closed;
			
			public DialogItem(IDialog dialog) {
				this.dialog = dialog;
				this.dialog.Closed += Dialog_Closed;
			}
			
			void Dialog_Closed(object sender, EventArgs eventArgs) {
				this.dialog.Closed -= Dialog_Closed;
				Closed(this);
			}
			
			public void Show() {
				ShowType showType = AlreadyShownOneTime ? ShowType.RestoreFromHide : ShowType.FirstOpen;
				AlreadyShownOneTime = true;
				dialog.Show(showType);
			}
			
			public void Hide() {
				dialog.Hide();
			} 
			
			public bool IsDialog(IDialog dialogToTest) {
				return object.ReferenceEquals(dialog, dialogToTest);
			}
		}
	}
}
