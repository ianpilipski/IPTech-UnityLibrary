#if !(UNITY_EDITOR || DEVELOPMENT_BUILD || QA_BUILD)
#define CONTAINERDEBUGSERVICE_DISABLED
#endif

#if !CONTAINERDEBUGSERVICE_DISABLED

using System.Collections.Generic;
using UnityEngine.UIElements;

namespace IPTech.DebugConsoleService.InGameConsole {
    public class ListViewOutput {
        private readonly List<string> output;
        public readonly ListView listView;
        public float MaxHeight;
        bool notSelectable;

        public ListViewOutput(ListView listView) {
            this.listView = listView;
            this.output = new();
            
            RegisterEvents();
        }

        
        public void Add(string line) {
            if(listView.virtualizationMethod == CollectionVirtualizationMethod.FixedHeight) {
                var lines = line.Split("\n");
                output.AddRange(lines);
            } else {
                output.Add(line);
            }
            if(MaxHeight>0) {
                var itemSize = listView.fixedItemHeight * output.Count;
                if(itemSize > MaxHeight) {
                    listView.style.height = new StyleLength(new Length(MaxHeight, LengthUnit.Pixel));
                } else {
                    listView.style.height = new StyleLength(new Length(itemSize, LengthUnit.Pixel));
                }
            }
            listView.RefreshItems();
        }

        public void Clear() {
            output.Clear();
        }

        void RegisterEvents() {
            listView.itemsSource = output;

            listView.makeItem = HandleMakeItem;
            listView.bindItem = HandleBindItem;
            listView.unbindItem = HandleUnBindItem;
        }

        private void HandleUnBindItem(VisualElement arg1, int arg2) {
            
        }

        private void HandleBindItem(VisualElement arg1, int arg2) {
            var l = (Label)arg1;
            l.text = output[arg2];
        }

        private VisualElement HandleMakeItem() {
            var l = new Label();
            l.pickingMode = PickingMode.Ignore;
            return l;
        }
    }
}
#endif
