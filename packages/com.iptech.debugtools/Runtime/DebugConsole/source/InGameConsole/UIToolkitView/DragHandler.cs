#if !(DEBUG || QA_BUILD)
#define CONTAINERDEBUGSERVICE_DISABLED
#endif

#if !CONTAINERDEBUGSERVICE_DISABLED

using UnityEngine;
using UnityEngine.UIElements;

namespace IPTech.DebugConsoleService.InGameConsole {
    // Lets a horizontal ScrollView be scrolled by dragging with the mouse. Touch is left to the ScrollView.
    // The pointer is only captured once the drag threshold is passed, so child buttons still receive plain clicks.
    public class DragHandler {
        const float DragThreshold = 8f;

        readonly ScrollView scrollView;
        int pointerId = -1;
        bool isDragging;

        // True while a drag is in progress, so callers can ignore the click a drag would otherwise produce.
        public bool IsDragging => isDragging;
        Vector2 startPosition;
        float startOffset;

        public DragHandler(ScrollView scrollView) {
            this.scrollView = scrollView;
            scrollView.RegisterCallback<PointerDownEvent>(HandlePointerDown, TrickleDown.TrickleDown);
            scrollView.RegisterCallback<PointerMoveEvent>(HandlePointerMove, TrickleDown.TrickleDown);
            scrollView.RegisterCallback<PointerUpEvent>(HandlePointerUp, TrickleDown.TrickleDown);
            scrollView.RegisterCallback<PointerCaptureOutEvent>(HandlePointerCaptureOut);
        }

        private void HandlePointerDown(PointerDownEvent evt) {
            if(evt.pointerType != UnityEngine.UIElements.PointerType.mouse || evt.button != 0 || pointerId >= 0) return;
            pointerId = evt.pointerId;
            isDragging = false;
            startPosition = evt.position;
            startOffset = scrollView.scrollOffset.x;
        }

        private void HandlePointerMove(PointerMoveEvent evt) {
            if(evt.pointerId != pointerId) return;

            var deltaX = evt.position.x - startPosition.x;
            if(!isDragging) {
                if(Mathf.Abs(deltaX) < DragThreshold) return;
                isDragging = true;
                scrollView.CapturePointer(pointerId);
            }

            var max = Mathf.Max(0f, scrollView.contentContainer.resolvedStyle.width - scrollView.contentViewport.resolvedStyle.width);
            var offset = scrollView.scrollOffset;
            offset.x = Mathf.Clamp(startOffset - deltaX, 0f, max);
            scrollView.scrollOffset = offset;
            evt.StopPropagation();
        }

        private void HandlePointerUp(PointerUpEvent evt) {
            if(evt.pointerId != pointerId) return;

            if(isDragging) {
                scrollView.ReleasePointer(pointerId);
                evt.StopPropagation();
            }
            Reset();
        }

        private void HandlePointerCaptureOut(PointerCaptureOutEvent evt) {
            if(evt.target == scrollView && evt.pointerId == pointerId) Reset();
        }

        private void Reset() {
            pointerId = -1;
            isDragging = false;
        }
    }
}
#endif
