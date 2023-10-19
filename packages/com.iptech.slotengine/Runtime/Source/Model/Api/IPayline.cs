
using System.Collections.Generic;

namespace IPTech.SlotEngine.Model.Api
{ 

	public interface IPayline {
		IList<IPaylineEntry> PaylineEntries { get; set; }
	}

}
