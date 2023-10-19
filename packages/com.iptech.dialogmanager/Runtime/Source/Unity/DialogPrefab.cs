using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using IPTech.DialogManager.Api;
using System;

namespace IPTech.DialogManager.Unity {

	public class DialogPrefab : Dialog {
	
		Dialog prefabInstance;
		
		public Dialog Prefab;
		public Transform InstantiateParent;
		public bool WorldPositionStays;
		
		// Use this for initialization
		void Awake() {
			RegisterListeners();
		}
	
		void RegisterListeners() {
			this.OnHide.AddListener(HandleOnHide);
			this.OnShow.AddListener(HandleOnShow);
			this.Closed += HandleClosed;
		}
		
		void UnregisterListeners() {
			this.OnHide.RemoveListener(HandleOnHide);
			this.OnShow.RemoveListener(HandleOnShow);
			this.Closed -= HandleClosed;
		}
		
		void HandleOnHide() {
			if(prefabInstance!=null) {
				prefabInstance.Hide();
			}
		}
		
		void HandleOnShow(ShowType showType) {
			if(showType == ShowType.FirstOpen) {
				prefabInstance = Instantiate<Dialog>(Prefab, InstantiateParent, WorldPositionStays);
				prefabInstance.Closed += HandleClosed;
			}
			if(prefabInstance!=null) {
				prefabInstance.Show(showType);
			}
		}
		
		void HandleClosed(object sender, EventArgs eventArgs) {
			UnregisterListeners();
			if(prefabInstance!=null) {
				Destroy(prefabInstance);
				prefabInstance = null;
			}
		}
	}
}
