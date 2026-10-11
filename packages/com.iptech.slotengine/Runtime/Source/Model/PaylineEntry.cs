using IPTech.SlotEngine.Model.Api;
using System;
using UnityEngine;

namespace IPTech.SlotEngine.Model
{
	[Serializable]
	public class PaylineEntry : IPaylineEntry
	{
		[SerializeField] private int _reelColumn;
		[SerializeField] private int _reelRow;

		public int ReelColumn 
		{ 
			get => _reelColumn; 
			set => _reelColumn = value; 
		}

		public int ReelRow 
		{ 
			get => _reelRow; 
			set => _reelRow = value; 
		}

		public PaylineEntry() { }
	}
}
