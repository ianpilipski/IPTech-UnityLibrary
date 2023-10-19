#if !(UNITY_EDITOR || DEVELOPMENT_BUILD || QA_BUILD)
#define CONTAINERDEBUGSERVICE_DISABLED
#endif

#if !CONTAINERDEBUGSERVICE_DISABLED

using UnityEngine;
using System.Collections;
using UnityEngine.Events;

namespace IPTech.DebugConsoleService {

    public class AnimationEvents : MonoBehaviour {

    	public UnityEvent OnAnimationEnd;

        public void TriggerAnimationEnd() {
            if(this.OnAnimationEnd!=null) {
                this.OnAnimationEnd.Invoke();
            }
        }
    }
}

#endif