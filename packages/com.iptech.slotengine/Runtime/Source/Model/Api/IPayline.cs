
using System.Collections.Generic;

namespace IPTech.SlotEngine.Model.Api
{ 

	public interface IPayline {
		List<IPaylineEntry> PaylineEntries { get; set; }
	}

}
