using IPTech.SlotEngine.Model.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPTech.SlotEngine.Model
{
	[Serializable]
	public class WildSymbolSet : IWildSymbolSet
	{
		public IList<IWildSymbol> WildSymbols { get; set; }

		public WildSymbolSet() {
			this.WildSymbols = new List<IWildSymbol>();
		}
	}
}
