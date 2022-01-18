using UnityEngine;
using System.Collections;

namespace IPTech.Unity.Utils
{
	public class OnAwakeDeactivate : MonoBehaviour
	{
		void Awake() {
			this.gameObject.SetActive(false);
		}
	}
}