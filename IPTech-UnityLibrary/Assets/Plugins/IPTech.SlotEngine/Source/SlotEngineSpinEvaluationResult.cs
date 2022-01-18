using IPTech.SlotEngine.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IPTech.SlotEngine.Model.Api;

namespace IPTech.SlotEngine
{
	class SlotEngineSpinEvaluationResult : ISlotEngineSpinEvaluationResult
	{
		public ISpinResult SpinResult { get; set; }

		public IDictionary<IPayline, IPayoutTableEntry> WinningPaylines { get; protected set; }

		public float TotalPayoutMultiplier {
			get {
				float totalPayoutMultiplier = 0.0f;
				foreach(KeyValuePair<IPayline, IPayoutTableEntry> pair in this.WinningPaylines) {
					totalPayoutMultiplier += pair.Value.PayoutMultiplier;
				}
				return totalPayoutMultiplier;
			}
		}

		public SlotEngineSpinEvaluationResult() {
			this.WinningPaylines = new Dictionary<IPayline, IPayoutTableEntry>();
		}

	
	}
}
