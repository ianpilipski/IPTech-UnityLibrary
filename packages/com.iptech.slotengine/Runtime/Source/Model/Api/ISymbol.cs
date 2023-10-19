using System;

namespace IPTech.SlotEngine.Model.Api
{
	public interface ISymbol {
		string ID { get; set; }
		float Weight { get; set; }
	}
}