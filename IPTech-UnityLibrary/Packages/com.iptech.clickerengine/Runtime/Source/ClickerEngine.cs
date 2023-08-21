using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace IPTech.ClickerLibrary {
	public class ClickerEngine {
		public delegate int AdjustClicksDelegate(int clicksamount);

		public class TotalClicksChangedEventArgs : EventArgs {
			public int ClickAmount { get; private set; }
			public TotalClicksChangedEventArgs(int clickAmount) { this.ClickAmount = clickAmount; }
		}

		public class ClickerAddedClicksEventArgs : EventArgs {
			public int ClickAmount { get; private set; }
			public ClickerAddedClicksEventArgs(int clickAmount) { this.ClickAmount = clickAmount; }
		}

		private AdjustClicksDelegate AdjustClicksHandler = (x) => { return x; };
		public event EventHandler<TotalClicksChangedEventArgs> TotalClicksChanged;
		public event EventHandler<EventArgs> LevelUp;
		public event EventHandler<ClickerAddedClicksEventArgs> ClickerAddedClicks;
		

		private ClickerEngineInfo clickerengineinfo;
		private float ClicksTimeAccumulator = 0;
		private int ClicksLastCheck = 0;
		
		public int TotalClicks = 0;
		public int TotalClicksLastTick = 0;
		public int Clicks = 0;
		public int Level = 0;
		public int MaxLevel = 80;
		
		public List<Clicker> Clickers;
		
		public float AverageCPS {
			get;
			private set;
		}

		public int NextLevelCost {
			get {
				return clickerengineinfo.GetLevelCost(Level+1);
			}
		}
		
		public void Init(ClickerEngineInfo engineinfo) {
			clickerengineinfo = engineinfo;
			Clickers = new List<Clicker>();
			if (engineinfo.Clickers != null) {
				foreach (ClickerInfo clickerinfo in engineinfo.Clickers) {
					AddClicker(new Clicker(clickerinfo));
				}
			}
		}

		public void SetAdjustClicksHandler(AdjustClicksDelegate newAdjustClicksHandler) { 
			if(newAdjustClicksHandler==null) {
				ClearAdjustClicksHandler();
			} else {
				this.AdjustClicksHandler = newAdjustClicksHandler;
			}
		}

		public void ClearAdjustClicksHandler() {
			this.AdjustClicksHandler = (x) => { return x; };
		}
			
		public void AddClicker(Clicker newClicker) {
			newClicker.Clicked += ClickerClickedHandler;
			Clickers.Add (newClicker);			
		}
		
		public void Tick(float DeltaSeconds) {
			if(clickerengineinfo==null) {
				return;
			}

			foreach(Clicker clicker in Clickers) {
				clicker.Tick(DeltaSeconds);
			}

			int deltaClicks = this.TotalClicks - this.TotalClicksLastTick;
			if (deltaClicks!=0) {
				OnTotalClicksChanged(deltaClicks);
				this.TotalClicksLastTick = this.TotalClicks;
			}
			
			ClicksTimeAccumulator += DeltaSeconds;
			if(ClicksTimeAccumulator>=1.0f) {
				AverageCPS = AverageCPS * 0.66f + (((float)(TotalClicks - ClicksLastCheck)) / ClicksTimeAccumulator)*0.34f;
				//AverageCPS = (((float)(TotalClicks - ClicksLastCheck)) / ClicksTimeAccumulator);
				ClicksTimeAccumulator = 0.0f;
				ClicksLastCheck = TotalClicks;
			}
		}

		private void OnTotalClicksChanged(int deltaClicks) {
			if(this.TotalClicksChanged!=null) {
				this.TotalClicksChanged(this, new TotalClicksChangedEventArgs(deltaClicks));
			} 
		}
		
		void ClickerClickedHandler(object sender, Clicker.ClickedEventArgs args) {
			int clicksBefore = TotalClicks;
			Click((int)args.amount);	
			if(clicksBefore!=TotalClicks) {
				OnClickerAddedClicks((Clicker)sender, TotalClicks-clicksBefore);
			}
		}

		private void OnClickerAddedClicks(Clicker clicker, int clickAmount) {
			if(ClickerAddedClicks!=null) {
				ClickerAddedClicks(clicker, new ClickerAddedClicksEventArgs(clickAmount));
			}
		}	

		public void ClickManualClickers() {
			foreach(var mc in Clickers) {
				if(mc.IsManual) {
					mc.Click();
				}
			}
		}

		public virtual void Click(int numberclicks=1) {
			int adjustedClicks = AdjustClicksHandler(numberclicks);
			if (adjustedClicks!=0) {
				TotalClicks += adjustedClicks;
				Clicks += adjustedClicks;

				CheckLevelProgression();
			}
		}

		private void CheckLevelProgression() {
			while (ShouldLevelUp()) {
				Level++;
				if (LevelUp != null) {
					LevelUp(this, EventArgs.Empty);
				}
			}
		}

		private bool ShouldLevelUp() {
			return this.TotalClicks >= this.clickerengineinfo.GetLevelCost(this.Level + 1) && this.Level < this.MaxLevel;
		}
		
		public PurchaseClickerRetVal PurchaseClicker(int index) {
			Clicker clickerToPurchase = Clickers[index];
			int cost = clickerToPurchase.CurrentCost;
			if ( Clicks >= cost ) {
				clickerToPurchase.Count++;
				Clicks -= cost;
				return PurchaseClickerRetVal.Success;
			} else {
				return PurchaseClickerRetVal.InsufficientFunds;
			}
		}
		
		public enum PurchaseClickerRetVal {
			Success,
			InsufficientFunds,
			MaxedOut
		}

		public void ResetToDefaults()
		{
			this.TotalClicks = 0;
			this.Clicks = 0;
			this.Level = 0;
		
			foreach(Clicker clicker in this.Clickers)
			{
				clicker.Count = 0;
			}
		}

		#region Serialization
		public void RestoreFromObjectData(Data data) {
			this.TotalClicks = data.TotalClicks;
			this.Clicks = data.Clicks;
			this.Level = data.Level;

			if(data.Clickers!=null) {
				foreach(Data.ClickerData cd in data.Clickers) {
					bool found = false;
					foreach(Clicker clicker in this.Clickers) {
						if(clicker.name==cd.Name) {
							clicker.Count = cd.Count;
							found = true;
							break;
						}
					}

					if(!found) {
						//Error out here? Missing clicker? Maybe config changed and saved data is not valid?
					}
				}
			}
		}

		public Data GetObjectData() {
			var data = new Data {
				TotalClicks = this.TotalClicks,
				Clicks = this.Clicks,
				Level = this.Level,
				Clickers = new List<Data.ClickerData>()
			};

			for(int i=0;i<this.Clickers.Count;i++) {
				var c = this.Clickers[i];
				data.Clickers.Add(new Data.ClickerData {
					Name = c.name,
					Count = c.Count
				});
			}
			return data;
		}

		[Serializable]
		public class Data {
			public int TotalClicks;
			public int Clicks;
			public int Level;
			public List<ClickerData> Clickers;


			[Serializable]
			public class ClickerData {
				public string Name;
				public int Count;
			}
		}
		#endregion
	}
}
