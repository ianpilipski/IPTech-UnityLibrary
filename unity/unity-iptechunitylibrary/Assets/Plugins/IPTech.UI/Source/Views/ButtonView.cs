using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace IPTech.UI {
	public interface IButtonView : IView {
		void SetText(string text);
		void SetIcon(Sprite icon);
		void SetEnabled(bool enabled);
		event Action<IButtonView> ClickStarted;
		event Action<IButtonView> Clicked;
	}

	public class ButtonView : BaseView, IButtonView {
		public Button ButtonObject;
		public Text ButtonText;
		public Image Icon;
		public Animator ButtonAnimation;
		public String ButtonAnimationTrigger;
		public int MilliSecsWaitForAnimation;

		public event Action<IButtonView> ClickStarted;
		public event Action<IButtonView> Clicked;

		protected virtual void Start() {
			if(ButtonObject != null) {
				ButtonObject.onClick.AddListener(HandleClicked);
			}
		}

		override protected void OnDestroy() {
			base.OnDestroy();
			if(ButtonObject != null) {
				ButtonObject.onClick.RemoveListener(HandleClicked);
			}
			Clicked = null;
			ClickStarted = null;
		}

		async void HandleClicked() {
			if(ButtonAnimation != null) {
				ClickStarted?.Invoke(this);
				await PlayButtonAnimation();
				ButtonAnimation.Rebind();
				Clicked?.Invoke(this);
			} else {
				ClickStarted?.Invoke(this);
				Clicked?.Invoke(this);
			}
		}

		async Task PlayButtonAnimation() {
			ButtonAnimation.SetTrigger(ButtonAnimationTrigger);
			await Task.Delay(MilliSecsWaitForAnimation);
		}

		public void SetText(string text) {
			if(ButtonText != null) {
				ButtonText.text = text;
			}
		}

		public void SetIcon(Sprite icon) {
			if(Icon != null) {
				Icon.sprite = icon;
			}
		}

		public void SetEnabled(bool enabled) {
			if(ButtonObject!=null) {
				ButtonObject.interactable = enabled;
			}
		}

		
	}
}
