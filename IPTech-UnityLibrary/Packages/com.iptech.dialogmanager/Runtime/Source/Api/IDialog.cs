using UnityEngine;
using System.Collections;
using System;

namespace IPTech.DialogManager.Api
{
	public enum ShowType {
		FirstOpen,
		RestoreFromHide
	}
	
	public interface IDialog
	{
		event EventHandler Closed;
		void Show(ShowType showType);
		void Hide();
	}
}
