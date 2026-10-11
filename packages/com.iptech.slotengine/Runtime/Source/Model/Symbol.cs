using IPTech.SlotEngine.Model.Api;
using System;
using UnityEngine;

namespace IPTech.SlotEngine.Model
{
	[Serializable]
	public class Symbol : ISymbol
	{
		private const string WEIGHT_OUT_OF_RANGE = "The value for weight must not be a negative value.";
		private const float DEFAULT_WEIGHT = 1.0f;

		[SerializeField] private float _weight;
		[SerializeField] private string _id;

		public string ID {
			get { return this._id; }
			set { this._id = value; }
		}

		public float Weight {
			get { return this._weight; }
			set {
				if(value<0.0f) {
					throw new ArgumentOutOfRangeException(WEIGHT_OUT_OF_RANGE);
				}
				this._weight = value;
			}
		}

		public Symbol() {
			this.Weight = DEFAULT_WEIGHT;
		}
	}
}