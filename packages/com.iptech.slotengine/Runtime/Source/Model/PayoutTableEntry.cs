using System;
using System.Collections.Generic;
using IPTech.SlotEngine.Model.Api;
using UnityEngine;

namespace IPTech.SlotEngine.Model
{
	[Serializable]
	public class PayoutTableEntry : IPayoutTableEntry
	{
		[SerializeField] private string _id;
		[SerializeField] private float _payoutMultiplier;
		[SerializeReference] private List<string> _symbolIDList;
		
		public string ID 
		{ 
			get => _id; 
			set => _id = value; 
		}

		public float PayoutMultiplier
		{
			get => _payoutMultiplier;
			set => _payoutMultiplier = value;
		}

		public List<string> SymbolIDList 
		{ 
			get => _symbolIDList; 
			set => _symbolIDList = value; 
		}

		public PayoutTableEntry() {
			this.SymbolIDList = new List<string>();
			this.PayoutMultiplier = 1.0f;
		}
	}
}
