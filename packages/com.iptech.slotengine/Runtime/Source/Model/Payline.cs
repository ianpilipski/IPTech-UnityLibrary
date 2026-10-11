using System;
using IPTech.SlotEngine.Model.Api;
using System.Collections.Generic;
using UnityEngine;

namespace IPTech.SlotEngine.Model
{
	[Serializable]
	public class Payline : IPayline
	{
		[SerializeReference]
		private List<IPaylineEntry> _paylineEntries;

		public List<IPaylineEntry> PaylineEntries 
		{
			get => _paylineEntries;
			set => _paylineEntries = value;
		}

		public Payline() 
		{
			PaylineEntries = new List<IPaylineEntry>();
		}

	}
}
