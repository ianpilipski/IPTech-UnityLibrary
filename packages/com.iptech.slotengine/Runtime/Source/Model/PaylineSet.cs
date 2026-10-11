using IPTech.SlotEngine.Model.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace IPTech.SlotEngine.Model
{
	[Serializable]
	public class PaylineSet : IPaylineSet
	{
		[SerializeReference]
		private List<IPayline> _payLines;
		
		public List<IPayline> PayLines 
		{ 
			get => _payLines; 
			set => _payLines = value; 
		}

		public PaylineSet() {
			this.PayLines = new List<IPayline>();
		}
	}
}
