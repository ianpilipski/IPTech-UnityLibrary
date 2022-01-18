using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IPTech.PlayerInventory.Api;
using UnityEngine;
using strange.extensions.mediation.impl;
using System.Collections;
using IPTech.PlayerInventory.Strange.Api;
using strange.extensions.signal.impl;

namespace IPTech.PlayerInventory.Strange
{
	public abstract class InventoryItemView : View, IInventoryItemView
	{
		private Signal<string> _inventoryItemIDChangedSignal = new Signal<string>();
		public Signal<string> InventoryItemIDChangedSignal {
			get {
				return this._inventoryItemIDChangedSignal;
			}
		}

		public string inventoryItemID = string.Empty;

		public long animationSpeedInMilliseconds = 1000;
		public bool negativeAdjustmentIsImmediate = true;

		private long targetAmount;

		public long Amount {
			get { return targetAmount; }
			set {
				if (targetAmount != value) {
					targetAmount = value;
					EnsureAnimateCoroutineStarted();
				}
			}
		}

		protected long currentAmount;
		private Coroutine animateToTargetAmountCoroutine;

		public void SetInventoryItemID(string newInventoryItemID) {
			if(this.inventoryItemID!=newInventoryItemID) {
				this.inventoryItemID = newInventoryItemID;
				this.InventoryItemIDChangedSignal.Dispatch(this.inventoryItemID);
			}
		}

		protected override void Start() {
			base.Start();
			UpdateVisuals();
		}

        private void EnsureAnimateCoroutineStarted() {
            if(this==null) {
                return;
            }
			if (this.gameObject.activeInHierarchy) {
				if (animateToTargetAmountCoroutine == null) {
					this.animateToTargetAmountCoroutine = StartCoroutine(AnimateToTargetAmount());
				}
			} else {
				if (animateToTargetAmountCoroutine!=null) {
					StopCoroutine(this.animateToTargetAmountCoroutine);
					this.animateToTargetAmountCoroutine = null;
				}
				currentAmount = targetAmount;
				UpdateVisuals();
			}
		}
		private IEnumerator AnimateToTargetAmount() {
			
			while (this.currentAmount != this.targetAmount) {

				float startTime = Time.time;
				float startAmount = this.currentAmount;
				long endAmount = this.targetAmount;
				float animationTime = this.animationSpeedInMilliseconds / 1000F;

				while (this.currentAmount != endAmount) {
					if (this.targetAmount != endAmount) {
						if (targetAmount < this.currentAmount && this.negativeAdjustmentIsImmediate) {
							this.currentAmount = targetAmount;
							UpdateVisuals();
							break;
						}
					}
					long delta = endAmount - currentAmount;
					float alpha = 1F;
					if (animationTime > 0F) {
						alpha = Mathf.Clamp((Time.time - startTime) / animationTime, 0F, 1F);
					}
					currentAmount = (long)Mathf.Lerp(startAmount, endAmount, alpha);

					if (delta < 0) {
						currentAmount = currentAmount < endAmount ? endAmount : currentAmount;
					} else {
						currentAmount = currentAmount > endAmount ? endAmount : currentAmount;
					}
					UpdateVisuals();
					yield return null;
				}
			}
			this.animateToTargetAmountCoroutine = null;
			yield break;
		}

		abstract protected void UpdateVisuals();

		public void AddAmount(IInventoryItem additionalInventoryItemAmount) {
			if (this.inventoryItemID == additionalInventoryItemAmount.InventoryItemID) {
				this.Amount += additionalInventoryItemAmount.Amount;
			}
		}

		public void SetAmount(IInventoryItem newInventoryItemAmount) {
			if (this.inventoryItemID == newInventoryItemAmount.InventoryItemID) {
				this.Amount = newInventoryItemAmount.Amount;
			}
		}

		private void OnDisable() {
			this.animateToTargetAmountCoroutine = null;
			currentAmount = targetAmount;
		}

		private void OnEnable() {
			UpdateVisuals();
		}
	}
}
