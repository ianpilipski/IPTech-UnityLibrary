using UnityEngine;
using System.Collections;
using UnityEditor;

public static class GeneratorEditorUtility {
	[MenuItem("GameObject/Edit Mode Utility/Force Refresh All")]
	public static void RefreshAll()
	{
		#if UNITY_6000_4_OR_NEWER
		Object[] allObjects = UnityEngine.Object.FindObjectsByType<MonoBehaviour>();
		#elif UNITY_6000_0_OR_NEWER
		Object[] allObjects = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
		#else
		Object[] allObjects = UnityEngine.Object.FindObjectsOfType(typeof(MonoBehaviour));
		#endif
		
		foreach(Object o in allObjects) {
			RefreshTarget t = o as RefreshTarget;
			if(t != null) {
				t.Refresh(true);
				UnityEditor.EditorUtility.SetDirty(o);
			}
		}
	}
}
