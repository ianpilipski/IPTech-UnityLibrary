using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IPTech.SlotEngine.Model.Api;

namespace IPTech.SlotEngine.Model
{
	[Serializable]
	public class PayoutTableEntry : IPayoutTableEntry
	{
		public string ID { get; set; }

		public float PayoutMultiplier { get; set; }

		public IList<string> SymbolIDList { get; set; }

		public PayoutTableEntry() {
			this.SymbolIDList = new List<string>();
			this.PayoutMultiplier = 1.0f;
		}
	}
}
