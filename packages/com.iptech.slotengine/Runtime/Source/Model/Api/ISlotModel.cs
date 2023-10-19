using System.Collections.Generic;

namespace IPTech.SlotEngine.Model.Api
{
	public interface ISlotModel
	{
		IReelSet ReelSet { get; set; }
		IPaylineSet PaylineSet { get; set; }
		IPayoutTable PayoutTable { get; set; }
		IWildSymbolSet WildSymbolSet { get; set; }
	}
}
