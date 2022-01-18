using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace IPTech.UI {
    public interface IPageView: IView {

	}

	public class PageView : BaseView, IPageView, IDragHandler, IEndDragHandler {
		Vector3 panelLocation;
		public float EaseTimeSeconds = 0.5F;
		public SwipeConstrants HorizontalSwipe = new SwipeConstrants() { Enabled = true };
		public SwipeConstrants VerticalSwipe;

		RectTransform rectTransform;

		void Start() {
			rectTransform = (RectTransform)transform;
			panelLocation = transform.position;
		}

		void IDragHandler.OnDrag(PointerEventData eventData) {
			var deltaDrag = eventData.position - eventData.pressPosition;
			deltaDrag = deltaDrag * new Vector2(HorizontalSwipe.Enabled ? 1.0f : 0.0f, VerticalSwipe.Enabled ? 1.0f : 0.0f);
			transform.position = panelLocation + new Vector3(
				deltaDrag.x,
				deltaDrag.y,
				0
			);
		}

		void IEndDragHandler.OnEndDrag(PointerEventData eventData) {
			Debug.Log("width = " + rectTransform.rect.width);

			var deltaDrag = eventData.position- eventData.pressPosition;
			Debug.Log("dd = " + deltaDrag.ToString());
			var pctDrag = deltaDrag / rectTransform.rect.width;
			Debug.Log("pctdd = " + pctDrag);
			var targetLocation = panelLocation;
			
			if(HorizontalSwipe.Enabled) {
				var absPctDrag = Mathf.Abs(pctDrag.x);
				if(absPctDrag > HorizontalSwipe.PercentThreshold) {
					Debug.Log("absPctdd = " + absPctDrag + " thresh = " + HorizontalSwipe.PercentThreshold);
					if (pctDrag.x > 0) {
						targetLocation.x += rectTransform.rect.width;
					} else {
						targetLocation.x -= rectTransform.rect.width;
					}
				}
			}
			if(VerticalSwipe.Enabled) {
				var absPctDrag = Mathf.Abs(pctDrag.y);
				if(absPctDrag > VerticalSwipe.PercentThreshold) {
					if (pctDrag.y > 0) {
						targetLocation.y += rectTransform.rect.width;
					} else {
						targetLocation.y -= rectTransform.rect.width;
					}
				}
			}

			StartCoroutine(SmoothMove(transform.position, targetLocation, EaseTimeSeconds));
			panelLocation = targetLocation;
		}

		IEnumerator SmoothMove(Vector3 startPos, Vector3 endPos, float seconds) {
			float t = 0F;
			while(t <= 1F) {
				t += Time.deltaTime / seconds;
				transform.position = Vector3.Lerp(startPos, endPos, Mathf.SmoothStep(0F, 1F, t));
				yield return null;
			}
		}

		[Serializable]
		public class SwipeConstrants {
			public bool Enabled;
			public float PercentThreshold = 0.2F;
		}
	}
}
