using IPTech.SlotEngine.Model.Api;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace IPTech.SlotEngine.Model
{
	[Serializable]
	public class WildSymbolSet : IWildSymbolSet
	{
		[SerializeReference] private List<IWildSymbol> _wildSymbols;

		public List<IWildSymbol> WildSymbols {
			get => _wildSymbols;
			set => _wildSymbols = value;
		}

		public WildSymbolSet() {
			this.WildSymbols = new List<IWildSymbol>();
		}
	}
}
