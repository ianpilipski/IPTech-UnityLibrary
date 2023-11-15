using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace IPTech.Unity.Utils
{
	public class OnAwakeEvent : MonoBehaviour
	{
		public UnityEvent EventToCallOnAwake =null;

		protected void Awake() {
			EventToCallOnAwake.Invoke();
		}
	}
}
