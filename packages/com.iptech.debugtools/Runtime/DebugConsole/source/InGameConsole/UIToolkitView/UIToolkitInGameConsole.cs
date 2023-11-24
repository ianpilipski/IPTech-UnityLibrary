#if !(UNITY_EDITOR || DEVELOPMENT_BUILD || QA_BUILD)
#define CONTAINERDEBUGSERVICE_DISABLED
#endif

#if !CONTAINERDEBUGSERVICE_DISABLED

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace IPTech.DebugConsoleService.InGameConsole {
    [RequireComponent(typeof(UIDocument))]
    public class UIToolkitInGameConsole : MonoBehaviour, IInGameDebugConsoleView
    {
        const int MINIMIZEDHEIGHT = 110;

        UIDocument doc;
        List<CategoryData> categories = new();

        ListViewOutput listViewOutput;
        ListViewOutput listViewNotify;

        ScrollView navButtonsScrollView;
        VisualElement panelAliasButtons;
        ScrollView topWindowScrollView;
        VisualElement topWindowConsolePanel;
        VisualElement handle;
        Label handleLabel;
        VisualElement root;
        VisualElement main;
        TextField textFieldCommand;
        Button buttonGo;

        private bool dragging;
        DateTime dragStartTime;
        Vector2 startMousePos;
        Vector2 startPanelPos;
        float startHandlePosX;
        float lastOpenHeight;

        DragHandler refToDragHandler;
        bool initCalled;
        IEnumerable<InGameDebugConsoleView.CommandData> deferredCommands;
        private Task hideNotifyTask;

        public event Action<string> OnExecuteCommand;
        public event Action OnWantsUpdatedCommands;

        private void Awake() {
            doc = GetComponent<UIDocument>();
            DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
            Init();    
        }

        void Init() {
            root = doc.rootVisualElement;
            main = root.Q<VisualElement>("sub");
            handle = main.Q<VisualElement>("handle");
            handleLabel = handle.Q<Label>("labelHandle");
            textFieldCommand = main.Q<TextField>("textFieldCommand");
            buttonGo = main.Q<Button>("buttonGo");
            buttonGo.clicked += () => {
                OnExecuteCommand?.Invoke(textFieldCommand.value);
            };

            listViewOutput = new ListViewOutput(main.Q<ListView>("listViewOutput"));
            listViewNotify = new ListViewOutput(main.Q<ListView>("listViewNotify"));
            listViewNotify.MaxHeight = 640;

            topWindowConsolePanel = main.Q<VisualElement>("consolePanel");
            topWindowScrollView = main.Q<ScrollView>("scrollViewTopWindow");

            var navButtons = main.Q<VisualElement>("navButtons");
            navButtonsScrollView = navButtons.Q<ScrollView>();
            var buttonConsole = main.Q<Button>("buttonConsole");
            refToDragHandler = new DragHandler(buttonConsole, HandleConsoleButtonClicked, HandleCategoryButtonDragged);

            panelAliasButtons = main.Q<VisualElement>("panelAliasButtons");

            MinimizeConsole();
            lastOpenHeight = 320;

            ShowConsoleView(true);
            HideNotifications();
            RegisterHandleDragCallbacks();
            initCalled = true;

            if(deferredCommands!=null) {
                UpdateButtons(deferredCommands);
            }
        }

        void RegisterHandleDragCallbacks() {
            handle.RegisterCallback<MouseDownEvent>(HandleMouseDown);
            handle.RegisterCallback<MouseUpEvent>(HandleMouseUp);
            handle.RegisterCallback<MouseMoveEvent>(HandleMouseMove);
        }

        private void HandleMouseMove(MouseMoveEvent evt) {
            if(dragging) {
                var panelSizePix = RuntimePanelUtils.ScreenToPanel(root.panel, new Vector2(Screen.width, Screen.height));
                
                var delta = (evt.mousePosition - startMousePos);
                var left = startHandlePosX + delta.x;
                var width = handle.resolvedStyle.width;
                if(left < 0) left = 0;
                if(left > panelSizePix.x - width) left = panelSizePix.x - width;
                handle.style.left = new StyleLength(new Length(left, LengthUnit.Pixel));
                
                var pp = startPanelPos;
                pp.y += delta.y;
                SetMainHeight((640 - pp.y));
            }
        }

        void SetMainHeight(float heightInPixels) {
            var pos = 640 - heightInPixels;
            if(pos > 600) pos = 600;
            if(pos < 0) pos = 0;

            if(heightInPixels < MINIMIZEDHEIGHT) heightInPixels = MINIMIZEDHEIGHT;
            if(heightInPixels > 640) heightInPixels = 640;

            var pp = main.transform.position;
            pp.y = pos;
            main.transform.position = pp;

            main.style.height = new StyleLength(new Length(heightInPixels, LengthUnit.Pixel));
        }

        private void HandleMouseUp(MouseUpEvent evt) {
            if(dragging) {
                if((DateTime.Now - dragStartTime).TotalSeconds < 0.25f) {
                    if(main.resolvedStyle.height <= MINIMIZEDHEIGHT + 50) {
                        RestoreConsole();
                    } else {
                        MinimizeConsole();
                    }
                }
                handle.ReleaseMouse();
                evt.StopPropagation();
            }
            dragging = false;
        }

        private void HandleMouseDown(MouseDownEvent evt) {
            if(!dragging) {
                dragging = true;
                dragStartTime = DateTime.Now;
                startMousePos = evt.mousePosition;
                startPanelPos = main.transform.position;
                startHandlePosX = handle.resolvedStyle.left;
                handle.CaptureMouse();
                evt.StopPropagation();
            }
        }

        private void HandleConsoleButtonClicked() {
            ShowConsoleView(true);
        }

        void ShowConsoleView(bool show) {
            topWindowScrollView.style.display = show ? DisplayStyle.None : DisplayStyle.Flex;
            topWindowConsolePanel.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
            if(show) {
                handleLabel.text = "Console";
            }
        }

        public void UpdateButtons(IEnumerable<InGameDebugConsoleView.CommandData> commands) {
            if(initCalled) {
                RemoveMissingCommands(commands);
                AddMissingCommands(commands);
            } else {
                deferredCommands = commands;
            }
        }

        void RemoveMissingCommands(IEnumerable<InGameDebugConsoleView.CommandData> commands) {
            for(int ii=categories.Count-1;ii>=0;ii--) {
                var cat = categories[ii];
                var cc = commands.Where(c => c.Category == cat.name);
                if(cc == null || cc.Count() == 0) {
                    RemoveCategory(cat);
                } else {
                    for(int i=cat.aliasButtons.Count-1;i>=0;i--) {
                        var ab = cat.aliasButtons[i];
                        if(!cc.Any(cmd => ab.name == cmd.ShortName && ab.command == cmd.Command)) {
                            RemoveAliasButton(cat, ab);
                        }
                    }
                }
            }

            void RemoveCategory(CategoryData cat) {
                foreach(var ab in cat.aliasButtons) {
                    RemoveAliasButton(cat, ab);
                }
                cat.Destroy();
                categories.Remove(cat);
            }

            void RemoveAliasButton(CategoryData cat, AliasButtonData ab) {
                ab.Destroy();
                cat.aliasButtons.Remove(ab);
            }
        }

        void AddMissingCommands(IEnumerable<InGameDebugConsoleView.CommandData> commands) {
            foreach(var c in commands) {
                var cat = GetOrCreateCategory(c.Category);
                if(!cat.aliasButtons.Any(ab => ab.name == c.ShortName && ab.command == c.Command)) {
                    var ab = new AliasButtonData(c, HandleAliasClicked);
                    if(c.visualElementFactory == null) {
                        panelAliasButtons.Add(ab.visualElement);
                    } else {
                        panelAliasButtons.parent.Add(ab.visualElement);
                    }
                    cat.aliasButtons.Add(ab);
                }
            }
        }

        private void HandleAliasClicked(AliasButtonData obj) {
            OnExecuteCommand?.Invoke($"\"{obj.command}\"");
        }

        CategoryData GetOrCreateCategory(string category) {
            var cat = categories.FirstOrDefault(c => c.name == category);
            if(cat==null) {
                cat = new CategoryData(category, HandleCategoryClicked, HandleCategoryDragged);
                navButtonsScrollView.Add(cat.button);
                categories.Add(cat);
            }
            return cat;
        }

        private void HandleCategoryDragged(CategoryData arg1, MouseMoveEvent arg2) {
            HandleCategoryButtonDragged(arg2);
        }

        void HandleCategoryButtonDragged(MouseMoveEvent arg2) {
            var delta = arg2.mouseDelta;
            delta.y = 0;
            delta.x = -delta.x;
            var newOffset = navButtonsScrollView.scrollOffset + delta;
            if(newOffset.x < 0) {
                newOffset.x = 0;
            }
            var max = navButtonsScrollView.contentContainer.resolvedStyle.width - navButtonsScrollView.contentRect.width;
            if(newOffset.x > max) {
                newOffset.x = max;
            }
            navButtonsScrollView.scrollOffset = newOffset;
        }

        private void HandleCategoryClicked(CategoryData obj) {
            ShowConsoleView(false);

            foreach(var cat in categories) {
                var vis = cat == obj;
                if(vis) {
                    handleLabel.text = cat.name;
                }
                foreach(var ab in cat.aliasButtons) {
                    ab.visualElement.style.display = vis ? DisplayStyle.Flex : DisplayStyle.None;
                }
            }
        }

        public void MinimizeConsole() {
            lastOpenHeight = main.resolvedStyle.height;
            SetMainHeight(0);
        }

        public void RestoreConsole() {
            OnWantsUpdatedCommands?.Invoke();
            if(lastOpenHeight < 200) {
                lastOpenHeight = 200;
            }
            SetMainHeight(lastOpenHeight);
        }

        void IInGameDebugConsoleView.Output(string msg) {
            if(listViewOutput != null) {
                listViewOutput.Add(msg);
            }
        }

        void IInGameDebugConsoleView.Notify(string msg) {
            if(listViewNotify != null) {
                listViewNotify.Add(msg);
                listViewNotify.listView.parent.style.opacity = 1;
                listViewNotify.listView.parent.style.display = DisplayStyle.Flex;
                hideNotifyTask = DelayTask(5);
                HideNotifyAfterDelay(hideNotifyTask);
            }
        }

        async Task DelayTask(float seconds) {
            DateTime timeout = DateTime.Now.AddSeconds(seconds);
            while(true) {
                await Task.Yield();
                if(DateTime.Now > timeout) {
                    return;            
                }
            }
        }

        async void HideNotifyAfterDelay(Task task) {
            await task;
            if(task == hideNotifyTask) {
                listViewNotify.listView.parent.style.opacity = 0;
                await DelayTask(2);
                if(task == hideNotifyTask) {
                    HideNotifications();
                }
            }
        }

        void HideNotifications() {
            listViewNotify.Clear();
            listViewNotify.listView.parent.style.display = DisplayStyle.None;
        }

        class CategoryData {
            public string name;
            public Button button;
            public List<AliasButtonData> aliasButtons;
            public Action<CategoryData> clickHandler;
            public Action<CategoryData, MouseMoveEvent> dragHandler;

            DragHandler dragHandlerObj;

            public CategoryData(string name, Action<CategoryData> clickHandler, Action<CategoryData, MouseMoveEvent> dragHandler) {
                this.name = name;
                button = new Button(HandleClick);
                button.text = name;
                aliasButtons = new();
                this.clickHandler = clickHandler;
                this.dragHandler = dragHandler;

                dragHandlerObj = new DragHandler(button, HandleClick, HandleMouseMove);
            }

            void HandleMouseMove(MouseMoveEvent evt) {
                dragHandler?.Invoke(this, evt);
            }

            void HandleClick() {
                clickHandler(this);
            }

            public void Destroy() {
                foreach(var ab in aliasButtons) {
                    ab.Destroy();
                }
                button.RemoveFromHierarchy();
                clickHandler = null;
                dragHandler = null;
            }
        }

        class AliasButtonData {
            public readonly VisualElement visualElement;
            public string command;
            public string name;
            public Action<AliasButtonData> clickHandler;

            public AliasButtonData(InGameDebugConsoleView.CommandData cmd, Action<AliasButtonData> clickHandler) {
                this.name = cmd.ShortName;
                this.command = cmd.Command;
                this.clickHandler = clickHandler;
                if(cmd.visualElementFactory != null) {
                    visualElement = new VisualElement();
                    visualElement.style.width = new StyleLength(new Length(100, LengthUnit.Percent));
                    visualElement.Add(cmd.visualElementFactory());
                } else {
                    var button = new Button(HandleClick);
                    button.text = name;
                    visualElement = button;
                }
            }

            private void HandleClick() {
                clickHandler(this);
            }

            public void Destroy() {
                visualElement.RemoveFromHierarchy();
                clickHandler = null;
            }
        }
    }
}
#endif

