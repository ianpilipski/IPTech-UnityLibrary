using IPTech.SlotEngine.Api;
using IPTech.SlotEngine.Model.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPTech.SlotEngine
{
	public class SlotEngineSimulator : ISlotEngineSimulator
	{
		public ISlotModel SlotModel { get; set; }
		public ISlotEngine SlotEngine { get; set; }

		public IList<ISlotEngineSpinEvaluationResult> SpinEvaluationResults { get; set; }

		public int SpinCount { get; set; }

		public float AveragePayoutMultiplier { get; protected set; }

		public void SimulateSpins() {
			SetupSimulation();
			SimulateEachSpinAndCollectResults();
			CalculateAveragePayoutMultiplier();
		}
		
		private void SetupSimulation() {
			this.SlotEngine.SlotModel = this.SlotModel;
			this.SpinEvaluationResults = new List<ISlotEngineSpinEvaluationResult>();
		}

		private void SimulateEachSpinAndCollectResults() {
			int totalSpins = this.SpinCount;
			while (totalSpins-- > 0) {
				this.SlotEngine.Spin(null);
				while(this.SlotEngine.IsSpinning) {
					System.Threading.Thread.Sleep(1);
				}
				this.SpinEvaluationResults.Add(this.SlotEngine.SpinEvaluationResult);
			}
		}

		private void CalculateAveragePayoutMultiplier() {
			this.AveragePayoutMultiplier = 0;
			foreach (ISlotEngineSpinEvaluationResult spinEvaluationResult in this.SpinEvaluationResults) {
				this.AveragePayoutMultiplier += spinEvaluationResult.TotalPayoutMultiplier;
			}
			this.AveragePayoutMultiplier /= this.SpinCount;
		}
	}
}
