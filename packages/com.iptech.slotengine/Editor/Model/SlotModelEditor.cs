using UnityEngine;
using UnityEditor;
using IPTech.SlotEngine.Unity.Model.Editor.Api;
using IPTech.SlotEngine.Model.Api;
using IPTech.SlotEngine.Model;
using IPTech.SlotEngine.Api;

namespace IPTech.SlotEngine.Unity.Model.Editor
{
	public class SlotModelEditor : ISlotModelEditor {
		public IReelSetEditor reelSetEditor => Context.Inst.reelSetEditor;
		public IPaylineSetEditor paylineSetEditor => Context.Inst.paylineSetEditor;
		public IPayoutTableEditor payoutTableEditor => Context.Inst.payoutTableEditor;
		public IWildSymbolSetEditor wildSymbolSetEditor => Context.Inst.wildSymbolSetEditor;
		public ISlotEngine slotEngine => Context.Inst.slotEngine;
		public ISlotEngineSimulator slotEngineSimulator => Context.Inst.slotEngineSimulator;

		private bool foldoutReelSetEditor;
		private bool foldoutPaylineSetEditor;
		private bool foldoutPayoutTableEditor;
		private bool foldoutWildSymbolSetEditor;
		private int simulationSpinCount;
		private ISlotModel slotModel;

		public SlotModelEditor() {
			this.simulationSpinCount = 1000;
		}

		public bool OnInspectorGUI(object targetObject) {
			return OnInspectorGUI((ISlotModel)targetObject);
		}

		private bool OnInspectorGUI(ISlotModel slotModel) {
			this.slotModel = slotModel;

			bool modified = false;

			EditorGUILayout.LabelField("SlotModel");
			if (slotModel.ReelSet == null) {
				if (GUILayout.Button("Add Reelset")) {
					slotModel.ReelSet = new ReelSet();
					modified = true;
				}
			} else {
				foldoutReelSetEditor = EditorGUILayout.Foldout(foldoutReelSetEditor, "Reel Set");
				if (foldoutReelSetEditor) {
					modified = reelSetEditor.OnInspectorGUI(slotModel.ReelSet) || modified;
				}
			}

			if (slotModel.PaylineSet == null) {
				if (GUILayout.Button("Add Payline Set")) {
					slotModel.PaylineSet = new PaylineSet();
					modified = true;
				}
			} else {
				this.foldoutPaylineSetEditor = EditorGUILayout.Foldout(this.foldoutPaylineSetEditor, "Pay Lines");
				if (this.foldoutPaylineSetEditor) {
					modified = this.paylineSetEditor.OnInspectorGUI(slotModel.PaylineSet) || modified;
				}
			}

			if (slotModel.PayoutTable == null) {
				if (GUILayout.Button("Add Payout Table")) {
					slotModel.PayoutTable = new PayoutTable();
					modified = true;
				}
			} else {
				this.foldoutPayoutTableEditor = EditorGUILayout.Foldout(this.foldoutPayoutTableEditor, "Pay Tables");
				if (this.foldoutPayoutTableEditor) {
					modified = this.payoutTableEditor.OnInspectorGUI(slotModel.PayoutTable) || modified;
				}
			}

			if (slotModel.WildSymbolSet == null) {
				if (GUILayout.Button("Add Wild Symbol Set")) {
					slotModel.WildSymbolSet = new WildSymbolSet();
					modified = true;
				}
			} else {
				this.foldoutWildSymbolSetEditor = EditorGUILayout.Foldout(this.foldoutWildSymbolSetEditor, "Wild Symbols");
				if (this.foldoutWildSymbolSetEditor) {
					modified = this.wildSymbolSetEditor.OnInspectorGUI(slotModel.WildSymbolSet) || modified;
				}
			}

			if (this.slotEngine != null && this.slotEngineSimulator != null) {
				EditorGUILayout.LabelField("Simulator");
				this.simulationSpinCount = EditorGUILayout.IntField("Number Of Spins", this.simulationSpinCount);
				if (GUILayout.Button("Simulate")) {
					RunSimulationWithCurrentSettings();
				}

				EditorGUILayout.IntField("Simulated Spins", this.slotEngineSimulator.SpinCount);
				EditorGUILayout.FloatField("Return To Player", this.slotEngineSimulator.AveragePayoutMultiplier);
			}

			GUILayout.FlexibleSpace();


			return modified;
		}

		private void RunSimulationWithCurrentSettings() {
			this.slotEngineSimulator.SlotEngine = this.slotEngine;
			this.slotEngineSimulator.SlotModel = this.slotModel;
			this.slotEngineSimulator.SpinCount = this.simulationSpinCount;
			this.slotEngineSimulator.SimulateSpins();
		}
	}
}
