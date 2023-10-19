using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace IPTech.Unity.UI
{
	public class ButtonMultiFunction : Button
	{

		private bool rightClickPressed;

		public override void OnPointerDown(PointerEventData eventData) {
			if (eventData.button == PointerEventData.InputButton.Right) {
				this.rightClickPressed = true;
			}
			base.OnPointerDown(eventData);
		}

		public override void OnPointerUp(PointerEventData eventData) {
			if (eventData.button == PointerEventData.InputButton.Right) {
				this.rightClickPressed = false;
			}
			base.OnPointerUp(eventData);
		}

		void Update() {
			if (this.rightClickPressed) {
				this.onClick.Invoke();
			}
		}
	}
}