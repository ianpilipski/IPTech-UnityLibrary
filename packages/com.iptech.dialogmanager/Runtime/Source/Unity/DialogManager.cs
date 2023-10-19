using System.Collections;
using System.Collections.Generic;
using IPTech.DialogManager.Api;
using UnityEngine;

namespace IPTech.DialogManager.Unity {

	public class DialogManager : MonoBehaviour {
	
		IDialogManager dialogManager;

		void Awake() {
			dialogManager = new DialogManagerBasic();
		}
	
		public void ShowDialog(Dialog dialog) {
			dialogManager.ShowDialog((IDialog)dialog);
		}
		
		public void EnqueueDialog(Dialog dialog) {
			dialogManager.EnqueueDialog((IDialog)dialog);
		}
	}
}