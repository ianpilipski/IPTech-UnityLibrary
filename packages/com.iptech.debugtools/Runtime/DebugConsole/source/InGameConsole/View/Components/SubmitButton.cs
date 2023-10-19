#if !(UNITY_EDITOR || DEVELOPMENT_BUILD || QA_BUILD)
#define CONTAINERDEBUGSERVICE_DISABLED
#endif

#if !CONTAINERDEBUGSERVICE_DISABLED

using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace IPTech.DebugConsoleService.InGameConsole {
    public class SubmitButton : Button {
        public override void OnSubmit(BaseEventData eventData) {
            onClick.Invoke();
        }
    }
}

#endif
