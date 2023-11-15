using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IPTech.Unity.Utilties
{
	public class GameObjectUtilities : MonoBehaviour
	{
		public void SetPosition(Transform position) {
			this.transform.position = position.position;
		}

		public void SetRotation(Transform rotation) {
			this.transform.rotation = rotation.rotation;
		}

		public void SetPostionAndRotation(Transform transform) {
			SetPosition(transform);
			SetRotation(transform);
		}
		
		public void DestroyChildren() {
			int childCount = this.transform.childCount;
			for (int i = childCount - 1; i >= 0; i--) {
				Transform childTransform = this.transform.GetChild(i);
				Destroy(childTransform.gameObject);
			}
		}
	}
}