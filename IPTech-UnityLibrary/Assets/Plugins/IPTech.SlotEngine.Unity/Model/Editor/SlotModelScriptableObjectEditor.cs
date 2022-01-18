using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using IPTech.SlotEngine.Unity;
using IPTech.Unity.Utils.Editor;
using IPTech.SlotEngine.Model.Api;
using IPTech.SlotEngine.Unity.Model.Editor.Api;
using IPTech.SlotEngine.Model;

namespace IPTech.SlotEngine.Unity.Model.Editor
{
	[CustomEditor(typeof(SlotModelScriptableObject))]
	public class SlotModelScriptableObjectEditor : UnityEditor.Editor
	{
		private const string SLOTMODEL_ASSETNAME = "SlotModel";

		private SlotModelEditorContext slotModelEditorContext;

		[MenuItem("IPTech/Create Slot Model")]
		public static void CreateSlotModelScriptableObjectMenuItem() {
			ScriptableObjectFactory sof = new ScriptableObjectFactory();
			SlotModelScriptableObject obj = (SlotModelScriptableObject)sof.Create<SlotModelScriptableObject>();
			sof.Save(obj, SLOTMODEL_ASSETNAME);
		}

		private ISlotModelEditor slotModelEditor { get; set; }
		
		public SlotModelScriptableObjectEditor() {
			this.slotModelEditorContext = new SlotModelEditorContext();
			this.slotModelEditor = this.slotModelEditorContext.GetSlotModelEditor();
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
