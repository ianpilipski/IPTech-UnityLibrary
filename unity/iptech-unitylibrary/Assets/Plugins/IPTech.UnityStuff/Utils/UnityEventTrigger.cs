using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace IPTech.Unity.Utils
{
	class UnityEventTrigger : MonoBehaviour
	{
		public enum EAutoTriggerType
		{
			None,
			Awake,
			Start,
			OnDestroy
		}

		public EAutoTriggerType AutoTriggerType = EAutoTriggerType.None;

		public UnityEvent OnTriggerEvent = null;

		public void TriggerEvents() {
			if(this.OnTriggerEvent!=null) {
				this.OnTriggerEvent.Invoke();
			}
		}

		public void ConditionalTriggerEvents(EAutoTriggerType autoTriggerType) {
			if(this.AutoTriggerType==autoTriggerType) {
				TriggerEvents();
			}
		}

		void Awake() {
			ConditionalTriggerEvents(EAutoTriggerType.Awake);
		}

		void Start() {
			ConditionalTriggerEvents(EAutoTriggerType.Start);
		}

		void OnDestroy() {
			ConditionalTriggerEvents(EAutoTriggerType.OnDestroy);
		}

	}
}
