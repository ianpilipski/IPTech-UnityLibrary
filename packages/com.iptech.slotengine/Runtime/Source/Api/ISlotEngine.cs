using UnityEngine;
using System.Collections;
using IPTech.SlotEngine.Model.Api;
using System.Collections.Generic;
using System;

namespace IPTech.SlotEngine.Api
{
	public interface ISlotEngine {
		ISlotModel SlotModel { get; set; }
		ISlotEngineSpinEvaluationResult SpinEvaluationResult { get; }
		void Spin(Action<ISlotEngineSpinEvaluationResult> spinEvaluationResultCallback);
		bool IsSpinning { get; }
	}
}
