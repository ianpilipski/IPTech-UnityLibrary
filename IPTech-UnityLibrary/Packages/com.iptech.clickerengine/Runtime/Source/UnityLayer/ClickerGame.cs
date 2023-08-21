using UnityEngine;
using System;
using System.Collections;

namespace IPTech.ClickerLibrary {
	public class ClickerGame : MonoBehaviour {

		public bool AutoTick = false;

		public ClickerEngineInfo clickerengineinfo;
		
		public ClickerEngine clickerengine {
			get;
			private set;
		}
		
		private bool debugdraw = false;
		
		/*
		object ShowDebug(params string[] args) {
			debugdraw = !debugdraw;
			return "ShowDebug is " + (debugdraw ? "On" : "Off"); 
		}
		
		object Click(params string[] args) {
			int amount = DebugConsole.ArgAsType<int>(args,1,1);
			clickerengine.Click (amount);
			return "Clicked " + amount.ToString ();
		}
		
		object Purchase(params string[] args) {
			int index = DebugConsole.ArgAsType<int>(args,1,0);
			ClickerEngine.PurchaseClickerRetVal retVal = clickerengine.PurchaseClicker(index);
			return retVal;
		}
		*/

		// Use this for initialization
		virtual public void Awake () {
			clickerengine = new ClickerEngine();
			if (clickerengineinfo != null) {
				clickerengine.Init(clickerengineinfo);
				if(AutoTick) {
					StartCoroutine(AutoTickCoroutine());
				}
			}
		}
		
		virtual public void Start() {
			//DebugConsole.RegisterCommand("ShowDebug",ShowDebug);
			//DebugConsole.RegisterCommand("Purchase",Purchase);
			//DebugConsole.RegisterCommand("Click",Click);
		}
		
		private IEnumerator AutoTickCoroutine() {
			while (true) {
				clickerengine.Tick(Time.deltaTime);
				yield return null;
			}
		}
		
		private Vector2 scrollPos;
		void OnGUI() {
			if(Application.platform == RuntimePlatform.IPhonePlayer) {
				if(GUILayout.Button ("toggle console")) {
					//DebugConsole.IsOpen = !DebugConsole.IsOpen;
				}
			} else if(Event.current.isKey && Event.current.keyCode == KeyCode.BackQuote) {
				//DebugConsole.IsOpen = !DebugConsole.IsOpen;
			}
			
			if(debugdraw) {
				if(clickerengine!=null) {
					GUILayout.BeginVertical();
					GUILayout.Label("Total Clicks: " + clickerengine.TotalClicks.ToString ());
					GUILayout.Label("Clicks: " + clickerengine.Clicks.ToString ());
					GUILayout.Label ("CPS: " + clickerengine.AverageCPS.ToString ());
					GUILayout.Label("Level: " + clickerengine.Level.ToString());
					GUILayout.Label ("Next Level: " + clickerengine.NextLevelCost.ToString());
					GUILayout.Label ("-= Clickers =-");
					scrollPos = GUILayout.BeginScrollView(scrollPos);
					foreach(Clicker clicker in clickerengine.Clickers) {
						GUILayout.Label(clicker.name + " Count=" + clicker.Count + " CPS=" + clicker.ClicksPerSecond);
					}
					GUILayout.EndScrollView();
					GUILayout.EndVertical();
				} else {
					GUILayout.Label("No clicker engine found");	
				}
			}
		}
		
		static public ClickerGame GetInstance(Transform transform) {
			return GetInstance<ClickerGame>(transform);
		}
		
		static public ClickGameClass GetInstance<ClickGameClass>(Transform transform) where ClickGameClass : ClickerGame {
			return FindComponentSearchUpward<ClickGameClass>(transform);
		}
		
		static public ComponentClass FindComponentSearchUpward<ComponentClass>(Transform InTransform) where ComponentClass : Component {
			ComponentClass retVal = null;
			Transform currentparent = InTransform.parent;
			while(currentparent!=null) {
				foreach(Transform child in currentparent) {
					retVal = child.GetComponent<ComponentClass>();
					if(retVal!=null) {
						return retVal;
					}
				}
				currentparent = currentparent.parent;
			}
			
			return (ComponentClass)FindObjectOfType(typeof(ComponentClass));
		}	
	}
}
