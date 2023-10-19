using UnityEditor;
using UnityEngine;
using IPTech.SlotEngine.Model.Api;
using IPTech.SlotEngine.Unity.Model.Editor.Api;
using IPTech.SlotEngine.Model;

namespace IPTech.SlotEngine.Unity.Model.Editor
{
	[CustomEditor(typeof(SlotModelScriptableObject))]
	public class SlotModelScriptableObjectEditor : UnityEditor.Editor
	{
		private ISlotModelEditor slotModelEditor { get; set; }

		public SlotModelScriptableObjectEditor() {
			this.slotModelEditor = Context.Inst.SlotModelEditor;
		}

		public override void OnInspectorGUI() {
			base.OnInspectorGUI();

			SlotModelScriptableObject smso = ((SlotModelScriptableObject)target);
			ISlotModel slotModel = smso.slotModel;
			if (slotModel == null) {
				if (GUILayout.Button("Create Model")) {
					slotModel = smso.slotModel = new SlotModel();
				}
			} else {
				if (slotModelEditor.OnInspectorGUI(slotModel)) {
					EditorUtility.SetDirty(smso);
				}
			}
		}
	}
}
