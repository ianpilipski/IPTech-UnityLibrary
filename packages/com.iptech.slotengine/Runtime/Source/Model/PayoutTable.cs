using System;
using System.Collections.Generic;
using IPTech.SlotEngine.Model.Api;
using UnityEngine;

namespace IPTech.SlotEngine.Model
{
	[Serializable]
	public class PayoutTable : IPayoutTable
	{
		[SerializeField] private string _id;
		[SerializeReference] private List<IPayoutTableEntry> _payoutTableEntries;

		public string ID 
		{ 
			get => _id; 
			set => _id = value; 
		}

		public List<IPayoutTableEntry> PayoutTableEntries 
		{ 
			get => _payoutTableEntries; 
			set => _payoutTableEntries = value; 
		}

		public PayoutTable() {
			PayoutTableEntries = new List<IPayoutTableEntry>();
		}
	}
}
