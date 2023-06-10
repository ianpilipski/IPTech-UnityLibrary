using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IPTech.SlotEngine.Model.Api;

namespace IPTech.SlotEngine.Model
{
	[Serializable]
	public class SlotModel : ISlotModel
	{
		public IPaylineSet PaylineSet { get; set; }

		public IPayoutTable PayoutTable { get; set; }

		public IReelSet ReelSet { get; set; }

		public IWildSymbolSet WildSymbolSet { get; set; }

		public SlotModel() {
			
		}
	}
}
