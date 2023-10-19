using IPTech.SlotEngine.Model.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPTech.SlotEngine.Model
{
	[Serializable]
	public class WildSymbol : IWildSymbol
	{
		public string ID { get; set; }
	}
}
