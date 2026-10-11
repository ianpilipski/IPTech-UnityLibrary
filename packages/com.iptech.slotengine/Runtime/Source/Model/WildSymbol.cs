using IPTech.SlotEngine.Model.Api;
using System;
using UnityEngine;

namespace IPTech.SlotEngine.Model
{
	[Serializable]
	public class WildSymbol : IWildSymbol
	{
		[SerializeField] private string _id;

		public string ID {
			get { return this._id; }
			set { this._id = value; }
		}
	}
}
