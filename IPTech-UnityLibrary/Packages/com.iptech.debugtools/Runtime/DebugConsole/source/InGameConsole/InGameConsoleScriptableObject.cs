#if !(UNITY_EDITOR || DEVELOPMENT_BUILD || QA_BUILD)
#define CONTAINERDEBUGSERVICE_DISABLED
#endif

#if !CONTAINERDEBUGSERVICE_DISABLED

using UnityEngine;
using System.Linq;
using System.Collections.Generic;

//[CreateAssetMenu(menuName="InGameConsoleView", fileName="InGameConsoleView")]
public class InGameConsoleScriptableObject : ScriptableObject, ISerializationCallbackReceiver {

	public GameObject InGameConsoleViewPrefab;

	#region ISerializationCallbackReceiver implementation
	public void OnBeforeSerialize() {

	}
	public void OnAfterDeserialize() {

	}
	
	public void OnEnable()
	{
#if UNITY_EDITOR
		if(UnityEditor.BuildPipeline.isBuildingPlayer) {
			List<string> defs = UnityEditor.EditorUserBuildSettings.activeScriptCompilationDefines.ToList();
            if(IsConsoleDisabled(defs)){
				Debug.Log("Removing In Game Console Resource");
				this.InGameConsoleViewPrefab = null;
			}
		}		
#endif
	}
	#endregion

#if UNITY_EDITOR
    private bool IsConsoleDisabled(IList<string> defs) {
        bool qaBuild = defs.Contains("QA_BUILD");
        bool devBuild = defs.Contains("DEVELOPMENT_BUILD") || Debug.isDebugBuild;
        bool consoleDisabled = defs.Contains("CONTAINERDEBUGSERVICE_DISABLED");

        return !(qaBuild || devBuild) || consoleDisabled;
    }
#endif
}
#endif
