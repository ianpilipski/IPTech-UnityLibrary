using UnityEngine;
using System.Collections;
using UnityEditor;

public static class GeneratorEditorUtility {
	[MenuItem("GameObject/Edit Mode Utility/Force Refresh All")]
	public static void RefreshAll()
	{
		foreach(Object o in GameObject.FindObjectsOfType(typeof(MonoBehaviour))) {
			RefreshTarget t = o as RefreshTarget;
			if(t != null) {
				t.Refresh(true);
				UnityEditor.EditorUtility.SetDirty(o);
			}
		}
	}
}
