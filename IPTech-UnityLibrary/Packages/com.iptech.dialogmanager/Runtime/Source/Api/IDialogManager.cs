using UnityEngine;
using System.Collections;

namespace IPTech.DialogManager.Api
{
	public interface IDialogManager
	{
		void ShowDialog(IDialog dialogWindow);
		void EnqueueDialog(IDialog dialogWindow);
	}
}
