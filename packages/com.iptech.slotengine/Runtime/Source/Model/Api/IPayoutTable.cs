using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPTech.SlotEngine.Model.Api
{
	public interface IPayoutTable
	{
		string ID { get; set; }
		IList<IPayoutTableEntry> PayoutTableEntries { get; set; }
	}
}
