using UnityEngine;
using System;
using System.Collections;
using System.Runtime.Serialization;

namespace IPTech.ClickerLibrary {
	public class Clicker {
		readonly ClickerInfo clickerinfo;
		float AccumulatedAmount;
		
		public event EventHandler<ClickedEventArgs> Clicked;
		public int Count = 0;
		
		public Clicker(ClickerInfo clickerinfo) {
			this.clickerinfo = clickerinfo;
		}

		public bool IsManual => this.clickerinfo.ManualClicker;
			
		public bool IsInstanceOfClickerInfo(ClickerInfo info) => this.clickerinfo == info;
		
		public string Group => this.clickerinfo.Group;
			
		public int CurrentCost => GetCost(Count+1);
		
		public float NextClicksPerSecond => clickerinfo.GetClicksPerSecond(Count + 1);

		public float ClicksPerSecond => clickerinfo.GetClicksPerSecond(Count);
			
		public string name => clickerinfo.ClickerName;
		
		public void OnClicked(ClickedEventArgs e) {
			//UnityEngine.Debug.Log ("OnClicked " + clickerinfo.name + ": amount="+e.amount.ToString());
			EventHandler<ClickedEventArgs> handler = Clicked;
			if(handler!=null) {
				handler(this, e);
			} else {
				Debug.LogWarning("No Handler, for clicker : " + clickerinfo.name);
			}
		}
		
		virtual public int GetCost(int ForCount) {
			return clickerinfo.GetCost(ForCount);
		}
		
		virtual public void Tick(float DeltaSeconds) {
			if(this.clickerinfo.ManualClicker==false && Count>0) {
				AccumulatedAmount += this.ClicksPerSecond * DeltaSeconds;
				TryClick();
			}
		}

		virtual public void Click() {
			AccumulatedAmount += this.ClicksPerSecond;
			TryClick();
		}

		protected void TryClick() {
			if (AccumulatedAmount >= 1.0f) {
				float floorAmount = Mathf.Floor(AccumulatedAmount);
				OnClicked(new ClickedEventArgs(floorAmount));
				AccumulatedAmount -= floorAmount;
				//IPTech.Debug.Assert(AccumulatedAmount>=0.0f);
			}
		}

		public class ClickedEventArgs : EventArgs {
			public float amount;
			
			public ClickedEventArgs(float amount) : base() {
				this.amount = amount;
			}
		}
	}
}
