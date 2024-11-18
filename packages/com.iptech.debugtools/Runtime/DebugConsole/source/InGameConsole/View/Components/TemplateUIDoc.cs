#if !(UNITY_EDITOR || DEVELOPMENT_BUILD || QA_BUILD)
#define CONTAINERDEBUGSERVICE_DISABLED
#endif

#if !CONTAINERDEBUGSERVICE_DISABLED

using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace IPTech.DebugConsoleService.InGameConsole {
    public class TemplateUIDoc : MonoBehaviour {
        public VisualElement visualElement;
        private UIDocument uiDoc;
        private RectTransform rt;

        void Awake() {
            uiDoc = GetComponent<UIDocument>();
            rt = uiDoc.GetComponentInParent<RectTransform>();
            StartCoroutine(AddVisualElement());
        }

        private void Update() {
            //RectTransformUtility
            if(uiDoc.rootVisualElement != null) {
                //float rectHeight = (rt.anchorMax.y - rt.anchorMin.y) * Screen.height;
                //Vector2 poss = new Vector2(rt.anchorMin.x * Screen.width, rt.anchorMin.y * Screen.height);
                //poss.y += rectHeight;
                var rtPos = rt.position; // this is in pixels already
                rtPos.y = Screen.height - rtPos.y;
                var panel = uiDoc.rootVisualElement.panel;
                var panelPos = RuntimePanelUtils.ScreenToPanel(uiDoc.rootVisualElement.panel, rtPos);
                Debug.Log($"panel pos: {rt.position}, {panelPos}");
                var pos = uiDoc.rootVisualElement.transform.position;
                pos.y = panelPos.y;
                uiDoc.rootVisualElement.transform.position = pos;
            }
        }

        IEnumerator AddVisualElement() {
            while(true) {
                if(visualElement != null) {
                    if(uiDoc.rootVisualElement != null) {
                        uiDoc.rootVisualElement.Add(visualElement);
                        yield break;
                    }
                }
                yield return null;
            }
        }
    }
}
#endif

