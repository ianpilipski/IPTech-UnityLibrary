using System;
using IPTech.UI;
using UnityEngine;

namespace IPTech.UI {
	/*
	public class Text : UnityEngine.UI.Text, ITextView {
		string ITextView.Text {
			get { return text; }
			set { text = value; }
		}

		string IView.ViewId {
			get { return GetType().Name; }
		}

		bool IView.IsVisible => gameObject.activeSelf;

		event Action<IView> viewShown;
		event Action<IView> IView.ViewShown {
			add {
				viewShown += value;
			}

			remove {
				viewShown -= value;
			}
		}

		event Action<IView> viewDestroyed;
		event Action<IView> IView.ViewDestroyed {
			add {
				viewDestroyed += value;
			}

			remove {
				viewDestroyed -= value;
			}
		}

		void IView.AwakeView() {
			throw new NotImplementedException();
		}

		void IView.Hide() {
			gameObject.SetActive(false);
		}

		void IView.Show() {
			gameObject.SetActive(true);
			viewShown?.Invoke(this);
		}

		protected override void Awake() {
			base.Awake();
			ViewManagerLocator.Locate(this).RegisterView(this);
		}

		protected override void OnDestroy() {
			base.OnDestroy();
			viewDestroyed?.Invoke(this);
		}
	}*/
}
