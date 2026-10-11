using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IPTech.SlotEngine.Model.Api;
using UnityEngine;

namespace IPTech.SlotEngine.Model
{
	[Serializable]
	public class SlotModel : ISlotModel
	{
		[SerializeReference] private IPaylineSet _paylineSet;
		[SerializeReference] private IReelSet _reelSet;
		[SerializeReference] private IPayoutTable _payoutTable;
		[SerializeReference] private IWildSymbolSet _wildSymbolSet;
		
		public IPaylineSet PaylineSet {
			get => _paylineSet;
			set => _paylineSet = value;
		}

		public IPayoutTable PayoutTable {
			get => _payoutTable;
			set => _payoutTable = value;
		}

		public IReelSet ReelSet {
			get => _reelSet;
			set => _reelSet = value;
		}

		public IWildSymbolSet WildSymbolSet {
			get => _wildSymbolSet;
			set => _wildSymbolSet = value;
		}

		public SlotModel() {
			
		}
	}
}
