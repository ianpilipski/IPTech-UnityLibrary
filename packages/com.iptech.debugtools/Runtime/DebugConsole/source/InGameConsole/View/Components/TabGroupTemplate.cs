#if !(UNITY_EDITOR || DEVELOPMENT_BUILD || QA_BUILD)
#define CONTAINERDEBUGSERVICE_DISABLED
#endif

#if !CONTAINERDEBUGSERVICE_DISABLED

using UnityEngine;
using UnityEngine.UIElements;

namespace IPTech.DebugConsoleService.InGameConsole {
    public class TabGroupTemplate : MonoBehaviour {
        public UnityEngine.UI.Button templatButton;
        public UIDocument templateUIDocument;
    }
}
#endif

