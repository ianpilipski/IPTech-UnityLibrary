using System.Collections.Generic;

namespace IPTech.SlotEngine.Model.Api
{
	public interface IReel : ICollection<ISymbol>
	{
		string ID { get; set; }
		ISymbol this[int index] { get; }
	}
}