using System;
using System.Collections.Generic;
using IPTech.SlotEngine.Model.Api;

namespace IPTech.SlotEngine.Model
{
	[Serializable]
	public class PayoutTable : IPayoutTable
	{
		public string ID { get; set; }

		public IList<IPayoutTableEntry> PayoutTableEntries { get; set; }

		public PayoutTable() {
			PayoutTableEntries = new List<IPayoutTableEntry>();
		}
	}
}
