using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPTech.SlotEngine.Model.Api
{
	public interface IWildSymbolSet
	{
		List<IWildSymbol> WildSymbols { get; set; }
	}
}
