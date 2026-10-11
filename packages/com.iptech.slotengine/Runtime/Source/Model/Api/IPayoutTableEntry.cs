using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPTech.SlotEngine.Model.Api
{
	public interface IPayoutTableEntry
	{
		string ID { get; set; }
		List<string> SymbolIDList { get; set; }
		float PayoutMultiplier { get; set; }
	}
}
