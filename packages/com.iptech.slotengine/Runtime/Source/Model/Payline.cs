using System;
using IPTech.SlotEngine.Model.Api;
using System.Collections.Generic;

namespace IPTech.SlotEngine.Model
{
	[Serializable]
	public class Payline : IPayline
	{
		public IList<IPaylineEntry> PaylineEntries { get; set; }

		public Payline() {
			PaylineEntries = new List<IPaylineEntry>();
		}

	}
}
