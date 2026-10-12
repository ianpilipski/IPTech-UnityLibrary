using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace IPTech.EditorTools {
    internal sealed class ProjectCleanerPreview : IDisposable {
        private readonly VisualElement _root;
        private readonly VisualElement _mainPage;
        private readonly VisualElement _page;
        private readonly Label _summary;
        private readonly HelpBox _message;
        private readonly ListView _list;
        private readonly Button _cancel;
        private readonly Button _confirm;
        private readonly Button _returnFocus;
        private readonly Action _confirmDelete;
        private int _transitionVersion;
        private List<ProjectCleanerItem> _items = new List<ProjectCleanerItem>();

        public IReadOnlyList<ProjectCleanerItem> Items => _items;
        public bool IsShowing { get; private set; }

        public ProjectCleanerPreview(VisualElement root, Button returnFocus, Action confirmDelete) {
            _root = root;
            _returnFocus = returnFocus;
            _confirmDelete = confirmDelete;
            _mainPage = Require<VisualElement>("project-cleaner-main");
            _page = Require<VisualElement>("project-cleaner-preview");
            _summary = Require<Label>("preview-summary");
            _message = Require<HelpBox>("preview-message");
            _list = Require<ListView>("project-cleaner-preview-assets");
            _cancel = Require<Button>("project-cleaner-preview-cancel");
            _confirm = Require<Button>("project-cleaner-preview-confirm");
            _list.makeItem = () => {
                var label = new Label();
                label.AddToClassList("cell");
                return label;
            };
            _list.bindItem = (element, index) => {
                var item = _items[index];
                ((Label)element).text = item.AssetPath + "  (" + ProjectCleanerSession.FormatBytes(item.Bytes) + ")";
                element.tooltip = item.AssetPath;
            };
            _cancel.clicked += Cancel;
            _confirm.clicked += _confirmDelete;
        }

        private T Require<T>(string name) where T : VisualElement =>
            _root.Q<T>(name) ?? throw new InvalidOperationException("Project Cleaner: missing UXML element " + name);

        public void Show(IEnumerable<ProjectCleanerItem> selected, bool backup, string backupDirectory, string blockingReason) {
            _items = selected.ToList();
            _summary.text = $"Delete {_items.Count} assets ({ProjectCleanerSession.FormatBytes(_items.Sum(i => i.Bytes))})?";
            _message.text = (backup ? "Verified backup root: " + backupDirectory
                    : "NO BACKUP: recovery will depend on your version control or other backups.")
                + "\nDeletion cannot be undone in Unity. Dependencies and protections will be checked again before deleting.";
            _message.messageType = backup ? HelpBoxMessageType.Info : HelpBoxMessageType.Warning;
            _list.itemsSource = _items;
            _list.Rebuild();
            _confirm.text = backup ? "Back up and delete" : "Delete without backup";
            _confirm.SetEnabled(blockingReason == null);
            if(blockingReason != null) {
                _message.text = blockingReason;
                _message.messageType = HelpBoxMessageType.Warning;
            }
            Transition(true);
        }

        public void Cancel() => Transition(false);
        public void DisableConfirmation() => _confirm.SetEnabled(false);

        public void MarkStale(string message) {
            DisableConfirmation();
            if(!IsShowing) return;
            _message.text = message + "\nCancel to return to the main UI and scan again.";
            _message.messageType = HelpBoxMessageType.Warning;
        }

        private void Transition(bool show) {
            IsShowing = show;
            var version = ++_transitionVersion;
            _mainPage.style.display = DisplayStyle.Flex;
            _mainPage.style.visibility = Visibility.Visible;
            _page.style.display = DisplayStyle.Flex;
            _mainPage.SetEnabled(!show);
            _page.SetEnabled(show);
            _root.schedule.Execute(() => {
                if(version != _transitionVersion) return;
                _mainPage.style.translate = new Translate(Length.Percent(show ? -100 : 0), 0);
                _page.style.translate = new Translate(Length.Percent(show ? 0 : 100), 0);
                _root.schedule.Execute(() => {
                    if(version != _transitionVersion) return;
                    if(show) {
                        // Keep layout intact: display:none resets TwoPaneSplitView's fixed pane width.
                        _mainPage.style.visibility = Visibility.Hidden;
                        _cancel.Focus();
                    } else {
                        _page.style.display = DisplayStyle.None;
                        _returnFocus.Focus();
                    }
                }).StartingIn(240);
            }).StartingIn(16);
        }

        public void Dispose() {
            _transitionVersion++;
            IsShowing = false;
            _cancel.clicked -= Cancel;
            _confirm.clicked -= _confirmDelete;
        }
    }
}
