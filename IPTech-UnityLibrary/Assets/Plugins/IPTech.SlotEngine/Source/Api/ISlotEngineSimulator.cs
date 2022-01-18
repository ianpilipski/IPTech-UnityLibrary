using IPTech.SlotEngine.Model.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPTech.SlotEngine.Api
{
	public interface ISlotEngineSimulator
	{
		ISlotEngine SlotEngine { get; set; }
		ISlotModel SlotModel { get; set; }
		int SpinCount { get; set; }
		IList<ISlotEngineSpinEvaluationResult> SpinEvaluationResults { get; }
		float AveragePayoutMultiplier { get; }
		void SimulateSpins();
	}
}
