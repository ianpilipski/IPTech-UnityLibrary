#if !(UNITY_EDITOR || DEVELOPMENT_BUILD || QA_BUILD)
#define CONTAINERDEBUGSERVICE_DISABLED
#endif

#if !CONTAINERDEBUGSERVICE_DISABLED

using System;
using UnityEngine;
using UnityEngine.UI;

namespace IPTech.DebugConsoleService.InGameConsole
{
    public class ScrollbackBuffer {
        private LinkedItem scrollBackTextHead;
        private LinkedItem scrollBackTextTail;
        private int scrollBackCount = 0;
        private int maxScrollbackCount = 1000;

        public ScrollbackBuffer(int maxScrollbackBufferLines) {
            this.maxScrollbackCount = maxScrollbackBufferLines;
        }

        public void AppendScrollBackText(Text newText) {
            LinkedItem li = new LinkedItem() { 
                textItem = newText,
                nextTextItem = this.scrollBackTextHead
            };
            if(this.scrollBackTextHead!=null) {
                this.scrollBackTextHead.prevTextItem = li;
            }
            this.scrollBackTextHead = li;
            if(this.scrollBackTextTail==null) {
                this.scrollBackTextTail = li;
            }
            if(++scrollBackCount>this.maxScrollbackCount) {
                RemoveScrollBackTailItem();
            }
        }

        private void RemoveScrollBackTailItem() {
            LinkedItem li = this.scrollBackTextTail;
            if(li!=null) {
                this.scrollBackTextTail = li.prevTextItem;
                UnityEngine.Object.Destroy(li.textItem.gameObject);
                scrollBackCount--;
            }
        }

        private class LinkedItem {
            public Text textItem;
            public LinkedItem nextTextItem;
            public LinkedItem prevTextItem;
        }
    }
}

#endif

