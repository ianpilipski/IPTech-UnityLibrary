using IPTech.SlotEngine.Model.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPTech.SlotEngine.Model
{
	[Serializable]
	public class PaylineEntry : IPaylineEntry
	{
		public int ReelColumn { get; set; }

		public int ReelRow { get; set; }

		public PaylineEntry() { }
	}
}
