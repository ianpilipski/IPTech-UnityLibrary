using IPTech.SlotEngine.Model.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPTech.SlotEngine.Api
{
	public interface ISlotEngineSpinEvaluationResult {
		ISpinResult SpinResult { get; set; }
		IDictionary<IPayline, IPayoutTableEntry> WinningPaylines { get; }
		float TotalPayoutMultiplier { get; }
	}
}
