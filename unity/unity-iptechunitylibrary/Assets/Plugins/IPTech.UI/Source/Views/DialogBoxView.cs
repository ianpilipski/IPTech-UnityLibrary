using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Zenject;
using System;

namespace IPTech.UI {
	public interface IDialogBoxView : IView {
		string Message { get; set; }
		void ClearOptions();
		void AddOption(string text, System.Action onClicked);
		void Dismiss();
	}

	public class DialogBoxView : BaseView, IDialogBoxView {
		List<Button> _dynamicOptions;
		bool _isInitialized;

		public GameObject MessageBox;
		public Text Message;
		public Button OptionButton;
		public GameObject Content;

		public UnityEvent Dismissed;

		string IDialogBoxView.Message {
			get { return this.Message.text; }
			set { 
				this.Message.text = value; 
				this.MessageBox.SetActive(!string.IsNullOrEmpty(value));
			}
		}

		public override void AwakeView() {
			if(Content != null) {
				Content.SetActive(true);
			}
			EnsureInitialized();
		}

		void EnsureInitialized() {
			if(!_isInitialized) {
				_dynamicOptions = new List<Button>();
				OptionButton.gameObject.SetActive(false);
				_isInitialized = true;
			}
		}

		void IDialogBoxView.AddOption(string text, System.Action onClicked) {
			EnsureInitialized();
			Button b = OptionButton;
			b = Instantiate<Button>(OptionButton, OptionButton.transform.parent);
			Text t = b.GetComponentInChildren<Text>();
			if(t!=null) {
				t.text = text;
			}
			b.onClick.AddListener(() => { onClicked(); });
			b.gameObject.SetActive(true);
			_dynamicOptions.Add(b);
		}

		void IDialogBoxView.ClearOptions() {
			EnsureInitialized();
			foreach(var option in _dynamicOptions) {
				UnityEngine.Object.Destroy(option.gameObject);
			}
			_dynamicOptions.Clear();
		}

		void IDialogBoxView.Dismiss() {
			Hide();
			Dismissed.Invoke();
		}
	}

}