using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPTech.SlotEngine.Model.Api
{
	public interface IWildSymbolSet
	{
		IList<IWildSymbol> WildSymbols { get; set; }
	}
}
