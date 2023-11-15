using IPTech.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;

namespace IPTech.Model.Strange.Editor
{
	[CustomEditor(typeof(ModelView), true)]
	class ModelViewInspector : UnityEditor.Editor
	{
		public override void OnInspectorGUI() {
			ModelView modelView = (ModelView)target;
			string newModelID = EditorGUILayout.TextField("Model ID", modelView.ModelID.ToString());
			if(newModelID!=modelView.ModelID.ToString()) {
				Undo.RecordObject(modelView, "Changed ModelID");
				modelView.ModelID = RName.Name(newModelID);
			}

			DrawDefaultInspector();
		}
	}
}
