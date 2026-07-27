
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace IPTech.AgeVerification.Debugging
{
    public class MockPopupBuilder
    {
        private Stack<StackNode> _parentStack = new Stack<StackNode>();
        
        class StackNode
        {
            public VisualElement Parent;
            public NodeType NodeType;

            public void Add(VisualElement element)
            {
                Parent.Add(element);
            }
        }

        enum NodeType
        {
            None,
            Section,
            Column,
            Group,
            ScrollView,
            FullScreenOverlay,
            DialogBox
        }

        public VisualElement Root { get; }

        public MockPopupBuilder()
        {
            Root = new VisualElement();
            _parentStack.Push(new StackNode()
            {
                NodeType = NodeType.None,
                Parent = Root
            });
        }

        public VisualElement BeginFullScreenOverlay()
        {
            var overlay = new VisualElement()
            {
                name = "full-screen-overlay"
            };
            overlay.AddToClassList("mockui-full-screen-overlay");
            overlay.AddToClassList("mockui-dialog-overlay");
            _parentStack.Peek().Add(overlay);
            PushStackNode(overlay, NodeType.FullScreenOverlay);
            return overlay;
        }

        public void EndFullScreenOverlay()
        {
            if(_parentStack.Peek().NodeType != NodeType.FullScreenOverlay)
            {
                Debug.LogError("Attempted to end full screen overlay on a non-overlay node.");
                return;
            }

            _parentStack.Pop();
        }

        public VisualElement BeginDialogBox(string title)
        {
            var dialogBox = new VisualElement()
            {
                name = "dialog-box"
            };
            dialogBox.AddToClassList("mockui-dialog-box");
            dialogBox.Add(new Label(title));
            _parentStack.Peek().Add(dialogBox);
            PushStackNode(dialogBox, NodeType.DialogBox);
            return dialogBox;
        }

        public void EndDialogBox()
        {
            if(_parentStack.Peek().NodeType != NodeType.DialogBox)
            {
                Debug.LogError("Attempted to end dialog box on a non-dialog box node.");
                return;
            }

            _parentStack.Pop();
        }

        public VisualElement BeginSection(string sectionName)
        {
            var section = new VisualElement()
            {
                name = CreateSectionName("section", sectionName)
            };
            section.AddToClassList("mockui-section");
            section.Add(new Label(sectionName));
            
            _parentStack.Peek().Add(section);
            PushStackNode(section, NodeType.Section);
            return section;
        }

        private void PushStackNode(VisualElement element, NodeType nodeType)
        {
            _parentStack.Push(new StackNode()
            {
                NodeType = nodeType,
                Parent = element
            });
        }

        public void EndSection()
        {
            if(_parentStack.Peek().NodeType != NodeType.Section)
            {
                Debug.LogError("Attempted to end section on a non-section node.");
                return;
            }

            _parentStack.Pop();
        }

        public void BeginColumns(string columnsName)
        {
            var columns = new VisualElement()
            {
                name = CreateSectionName("columns", columnsName)
            };
            columns.AddToClassList("mockui-columns");
            _parentStack.Peek().Add(columns);
            PushStackNode(columns, NodeType.Column);
        }

        public void EndColumns()
        {
            if(_parentStack.Peek().NodeType != NodeType.Column)
            {
                Debug.LogError("Attempted to end columns on a non-column node.");
                return;
            }

            _parentStack.Pop();
        }

        public VisualElement BeginGroup(string groupName)
        {
            var group = new VisualElement()
            {
                name = CreateSectionName("group", groupName)
            };
            group.AddToClassList("mockui-group");
            _parentStack.Peek().Add(group);
            PushStackNode(group, NodeType.Group);
            return group;
        }

        public void EndGroup()
        {
            if(_parentStack.Peek().NodeType != NodeType.Group)
            {
                Debug.LogError("Attempted to end group on a non-group node.");
                return;
            }

            _parentStack.Pop();
        }

        public void BeginScrollView()
        {
            var scrollView = new VisualElement()
            {
                name = "scroll-view"
            };
            scrollView.AddToClassList("mockui-scroll-view");
            _parentStack.Peek().Add(scrollView);
            PushStackNode(scrollView, NodeType.ScrollView);
        }

        public void EndScrollView()
        {
            if(_parentStack.Peek().NodeType != NodeType.ScrollView)
            {
                Debug.LogError("Attempted to end scroll view on a non-scroll view node.");
                return;
            }

            _parentStack.Pop();
        }

        public void AddProperty(VisualElement property)
        {
            property.AddToClassList("mockui-property-field");
            _parentStack.Peek().Add(property);
        }

        public Label AddLabelProperty(string label)
        {
            var labelElement = new Label(label);
            AddProperty(labelElement);
            return labelElement;
        }

        public EnumField AddEnumProperty(string label, System.Enum value)
        {
            var enumField = new EnumField(label, value);
            AddProperty(enumField);
            return enumField;
        }

        public Toggle AddToggleProperty(string label, bool value)
        {
            var toggle = new Toggle(label) { value = value };
            AddProperty(toggle);
            return toggle;
        }

        public IntegerField AddIntegerProperty(string label, int value)
        {
            var integerField = new IntegerField(label) { value = value };
            AddProperty(integerField);
            return integerField;
        }

        public TextField AddTextFieldProperty(string label, string value)
        {
            var textField = new TextField(label) { value = value };
            AddProperty(textField);
            return textField;
        }

        public Button AddButton(string text, System.Action clicked)
        {
            var button = new Button() { text = text };
            button.clicked += clicked;
            _parentStack.Peek().Add(button);
            return button;
        }

        public void AddVisualElement(VisualElement element)
        {
            _parentStack.Peek().Add(element);
        }

        private string CreateSectionName(string prefix, string sectionName)
        {
            if(string.IsNullOrWhiteSpace(sectionName))
            {
                return $"{prefix}";
            }

            sectionName = sectionName.ToLower().Replace(" ", "-");

            return $"{prefix}-{sectionName}";
        }
    }
}