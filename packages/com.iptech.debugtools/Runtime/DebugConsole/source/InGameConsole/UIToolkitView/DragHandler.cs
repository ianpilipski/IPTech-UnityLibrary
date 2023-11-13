#if !(UNITY_EDITOR || DEVELOPMENT_BUILD || QA_BUILD)
#define CONTAINERDEBUGSERVICE_DISABLED
#endif

#if !CONTAINERDEBUGSERVICE_DISABLED

using System;
using UnityEngine.UIElements;

namespace IPTech.DebugConsoleService.InGameConsole {
    public class DragHandler {
        Action clickHandler;
        Action<MouseMoveEvent> dragHandler;

        VisualElement elem;
        bool isDragging;
        DateTime startDragTime;

        public DragHandler(VisualElement elem, Action clickHandler, Action<MouseMoveEvent> dragHandler) {
            this.elem = elem;
            this.clickHandler = clickHandler;
            this.dragHandler = dragHandler;
            elem.RegisterCallback<MouseMoveEvent>(HandleMouseMove, TrickleDown.TrickleDown);
            elem.RegisterCallback<MouseDownEvent>(HandleMouseDown, TrickleDown.TrickleDown);
            elem.RegisterCallback<MouseUpEvent>(HandleMouseUp, TrickleDown.TrickleDown);
        }

        private void HandleMouseUp(MouseUpEvent evt) {
            if(isDragging) {
                isDragging = false;
                evt.StopImmediatePropagation();
                elem.ReleaseMouse();

                if((DateTime.Now - startDragTime).TotalSeconds < 0.25) {
                    clickHandler?.Invoke();
                }
            }
        }

        private void HandleMouseDown(MouseDownEvent evt) {
            if(!isDragging) {
                isDragging = true;
                evt.StopImmediatePropagation();
                elem.CaptureMouse();
                startDragTime = DateTime.Now;
            }
        }

        private void HandleMouseMove(MouseMoveEvent evt) {
            if(isDragging) {
                dragHandler?.Invoke(evt);
            }
        }
    }
}
#endif

