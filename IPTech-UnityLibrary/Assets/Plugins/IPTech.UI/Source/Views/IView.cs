using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IPTech.UI {
	public interface IView {
		event Action<IView> ViewShown;
		event Action<IView> ViewDestroyed;

		string ViewId { get; }
		bool IsVisible { get; }
		void Show();
		void Hide();

		void AwakeView();
	}
}
