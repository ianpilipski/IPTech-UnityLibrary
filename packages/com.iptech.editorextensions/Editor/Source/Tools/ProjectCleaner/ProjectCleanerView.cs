using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace IPTech.EditorTools {
    /// <summary>
    /// Builds the Project Cleaner UI into any root element and binds it to a <see cref="ProjectCleanerSession"/>.
    /// Independent of <see cref="EditorWindow"/> so it can be hosted by the window or by an off-screen test panel.
    /// </summary>
    internal sealed class ProjectCleanerView : IDisposable {
        private const string UiDirectory = "Packages/com.iptech.editorextensions/Editor/Source/Tools/ProjectCleaner/";
        private readonly VisualElement _root;
        private readonly ProjectCleanerSession _session;
        private readonly IProjectCleanerServices _services;
        private readonly MultiColumnListView _table;
        private readonly ScrollView _details;
        private readonly HelpBox _message;
        private readonly Label _summary;
        private readonly ProgressBar _progress;
        private readonly Button _delete;
        private readonly Button _cancel;
        private readonly Button _scanButton;
        private readonly DropdownField _scopeField;
        private readonly DropdownField _sortField;
        private readonly Toggle _scriptAcknowledgement;
        private readonly ProjectCleanerPreview _preview;
        private ProjectCleanerScanner _operation;
        private IDisposable _ticking;
        private bool _syncingSort;

        public ProjectCleanerView(VisualElement root, ProjectCleanerSession session, IProjectCleanerServices services) {
            _root = root ?? throw new ArgumentNullException(nameof(root));
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _session.Normalize();
            root.Clear();
            LoadTree("ProjectCleanerEditor.uxml").CloneTree(root);
            _message = Require<HelpBox>("message");
            _progress = Require<ProgressBar>("progress");
            _summary = Require<Label>("summary");
            _details = Require<ScrollView>("details");
            _delete = Require<Button>("preview-selected");
            _preview = new ProjectCleanerPreview(root, _delete, ConfirmDelete);
            _delete.clicked += PreviewSelected;
            _scanButton = Require<Button>("scan");
            _scanButton.clicked += BeginScan;
            _cancel = Require<Button>("cancel-scan");
            _cancel.clicked += CancelScan;
            _scanButton.SetEnabled(true);
            _cancel.SetEnabled(false);
            Require<Button>("select-visible").clicked += () => { _session.SelectVisible(); UpdateView(); };
            Require<Button>("clear-selection").clicked += () => { _session.ClearSelection(); UpdateView(); };
            Require<Button>("export-csv").clicked += ExportCsv;
            Require<Button>("restore-backup").clicked += Restore;

            var search = Require<ToolbarSearchField>("search");
            search.SetValueWithoutNotify(_session.Search);
            search.RegisterValueChangedCallback(evt => { _session.Search = evt.newValue; UpdateView(); });
            var status = Require<DropdownField>("status");
            status.choices = ProjectCleanerSession.StatusChoices.ToList();
            status.SetValueWithoutNotify(_session.Status);
            status.RegisterValueChangedCallback(evt => { _session.Status = evt.newValue; UpdateView(); });
            _scopeField = Require<DropdownField>("scope");
            _scopeField.RegisterValueChangedCallback(evt => { _session.Scope = evt.newValue; UpdateView(); });
            _sortField = Require<DropdownField>("sort");
            _sortField.choices = ProjectCleanerSession.SortChoices.ToList();
            _sortField.SetValueWithoutNotify(_session.Sort);
            _sortField.RegisterValueChangedCallback(evt => {
                _session.Sort = evt.newValue;
                SyncSortIndicator();
                UpdateView();
            });
            var packages = Require<Toggle>("editable-packages");
            packages.SetValueWithoutNotify(ProjectCleanerSettings.instance.IncludeEditablePackages);
            packages.RegisterValueChangedCallback(evt => {
                ProjectCleanerSettings.instance.IncludeEditablePackages = evt.newValue;
                MarkStale("Cleanup scope changed. Scan again to include empty package folders.");
            });
            var folders = Require<TextField>("protected-folders");
            folders.SetValueWithoutNotify(string.Join("\n", ProjectCleanerSettings.instance.ProtectedFolders));
            Require<Button>("save-protection").clicked += () => {
                try {
                    ProjectCleanerSettings.instance.SetProtectedFolders(folders.value.Split('\n').Select(s => s.Trim()));
                    if(_session.Scan != null) ProjectCleaner.Reclassify(_session.Scan);
                    UpdateView();
                } catch(ArgumentException exception) { ShowError(exception); }
            };
            _table = Require<MultiColumnListView>("results");
            _table.horizontalScrollingEnabled = true;
            _table.itemsSource = _session.Visible;
            var pickColumn = _table.columns["selected"];
            pickColumn.makeCell = () => {
                var toggle = new Toggle();
                toggle.RegisterValueChangedCallback(evt => {
                    if(!(toggle.userData is ProjectCleanerItem item)) return;
                    _session.SetSelected(item, evt.newValue);
                    if(_session.SortColumn == "selected") UpdateView();
                    else UpdateSummary();
                });
                return toggle;
            };
            pickColumn.bindCell = (element, index) => {
                var item = _session.Visible[index];
                var toggle = (Toggle)element;
                toggle.userData = item;
                toggle.SetValueWithoutNotify(_session.IsSelected(item));
                toggle.SetEnabled(_session.CanSelect(item));
            };
            BindColumn("path", i => i.AssetPath);
            BindColumn("type", i => i.Category);
            BindColumn("status", i => i.Status.ToString());
            BindColumn("bytes", i => ProjectCleanerSession.FormatBytes(i.Bytes));
            BindColumn("refs", i => ProjectCleanerSession.ReferrerCount(i).ToString());
            _table.columnSortingChanged += OnColumnSortingChanged;
            SyncSortIndicator();
            _table.selectionChanged += objects => DrawDetails(objects.OfType<ProjectCleanerItem>().FirstOrDefault());
            _scriptAcknowledgement = Require<Toggle>("script-acknowledgement");
            _scriptAcknowledgement.SetValueWithoutNotify(_session.ReviewedScripts);
            _scriptAcknowledgement.RegisterValueChangedCallback(evt => { _session.ReviewedScripts = evt.newValue; UpdateSummary(); });
            var backup = Require<Toggle>("backup");
            backup.SetValueWithoutNotify(_session.Backup);
            backup.RegisterValueChangedCallback(evt => _session.Backup = evt.newValue);
            var destination = Require<TextField>("backup-root");
            destination.SetValueWithoutNotify(_session.BackupDirectory);
            destination.RegisterValueChangedCallback(evt => _session.BackupDirectory = evt.newValue);
            RebuildScopes();
            UpdateView();
            DrawDetails(null);
        }

        public bool IsScanning => _operation != null;

        public void Dispose() {
            StopTicking();
            _operation?.Cancel();
            _operation?.Dispose();
            _operation = null;
            _preview.Dispose();
        }

        public void BeginScan() {
            if(_operation != null) return;
            var blocked = _services.ScanBlockedReason;
            if(blocked != null) {
                MarkStale(blocked);
                return;
            }
            _operation = _services.CreateScanner();
            _session.StartScan(_operation.Result);
            _scanButton.SetEnabled(false);
            _cancel.SetEnabled(true);
            _delete.SetEnabled(false);
            _scriptAcknowledgement.SetValueWithoutNotify(false);
            _message.text = "Scanning. All Unity API work stays on the editor thread; Cancel stops between work units.";
            _ticking = _services.EveryUpdate(Tick);
        }

        public void CancelScan() {
            if(_operation == null) return;
            _operation.Cancel();
            EndScan();
        }

        public void ProjectChanged() {
            CancelScan();
            MarkStale("Project changed. Scan again before deleting.");
        }

        public void PreviewSelected() {
            var selected = _session.SelectedItems();
            if(selected.Count == 0 || _operation != null) return;
            _preview.Show(selected, _session.Backup, _session.BackupDirectory, _session.BlockingReason(selected));
        }

        public void ConfirmDelete() {
            if(!_preview.IsShowing || !_session.CanConfirm(_preview.Items)) return;
            _preview.DisableConfirmation();
            List<CleanerOutcome> outcomes;
            try {
                outcomes = _services.Delete(_preview.Items, _session.ReviewedScripts, _session.Backup, _session.BackupDirectory);
            } catch(Exception exception) {
                _preview.Cancel();
                ShowError(exception);
                return;
            }
            var message = _session.CompleteDeletion(outcomes);
            _preview.Cancel();
            MarkStale(message);
            DrawDetails(_table.selectedItem as ProjectCleanerItem);
            _services.ShowMessage("Cleanup results", ProjectCleanerSession.FormatOutcomes(outcomes));
        }

        public void MarkStale(string message) {
            _session.MarkStale();
            _preview.MarkStale(message);
            _message.text = message;
            _message.messageType = HelpBoxMessageType.Warning;
            UpdateView();
        }

        private void Tick() {
            var watch = Stopwatch.StartNew();
            while(_operation != null && watch.ElapsedMilliseconds < 12) {
                if(!_operation.Step()) { EndScan(); break; }
            }
            if(_session.Scan == null) return;
            _progress.value = _session.Scan.Progress * 100;
            _progress.title = _session.Scan.Stage;
        }

        private void EndScan() {
            StopTicking();
            _operation?.Dispose();
            _operation = null;
            _scanButton.SetEnabled(true);
            _cancel.SetEnabled(false);
            var scan = _session.Scan;
            if(scan != null) {
                _message.text = scan.CanClean ? "Scan complete. Review candidates and their evidence before selecting."
                    : "Deletion disabled. " + (scan.Cancelled ? "Scan cancelled." : string.Join("\n", scan.Errors));
                _message.messageType = scan.CanClean ? HelpBoxMessageType.Info : HelpBoxMessageType.Error;
            }
            RebuildScopes();
            UpdateView();
            DrawDetails(null);
        }

        private void StopTicking() {
            _ticking?.Dispose();
            _ticking = null;
        }

        private static VisualTreeAsset LoadTree(string fileName) {
            var path = UiDirectory + fileName;
            var tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path);
            if(tree == null) throw new InvalidOperationException("Project Cleaner: missing UXML asset at " + path);
            return tree;
        }

        private T Require<T>(string name) where T : VisualElement =>
            _root.Q<T>(name) ?? throw new InvalidOperationException("Project Cleaner: missing UXML element " + name);

        private static Label CreateCellLabel() {
            var label = new Label();
            label.AddToClassList("cell");
            return label;
        }

        private void BindColumn(string name, Func<ProjectCleanerItem, string> value) {
            var column = _table.columns[name];
            column.makeCell = CreateCellLabel;
            column.bindCell = (element, index) => {
                ((Label)element).text = value(_session.Visible[index]);
                element.tooltip = value(_session.Visible[index]);
            };
        }

        private void SyncSortIndicator() {
            _syncingSort = true;
            try {
                _table.sortColumnDescriptions.Clear();
                _table.sortColumnDescriptions.Add(new SortColumnDescription(_session.SortColumn,
                    _session.SortDescending ? SortDirection.Descending : SortDirection.Ascending));
                _sortField.SetValueWithoutNotify(_session.Sort);
            } finally { _syncingSort = false; }
        }

        private void OnColumnSortingChanged() {
            if(_syncingSort) return;
            var sort = _table.sortedColumns.FirstOrDefault();
            _session.SetSort(sort?.column?.name ?? sort?.columnName, sort?.direction == SortDirection.Descending);
            _sortField.SetValueWithoutNotify(_session.Sort);
            UpdateView();
        }

        private void RebuildScopes() {
            _scopeField.choices = _session.Scopes(_services.IsFolder);
            _scopeField.SetValueWithoutNotify(_session.Scope);
        }

        private void UpdateView() {
            var inspected = _table.selectedItem as ProjectCleanerItem;
            _session.RefreshVisible();
            _table.Rebuild();
            var inspectedIndex = inspected == null ? -1 : _session.Visible.IndexOf(inspected);
            _table.SetSelectionWithoutNotify(inspectedIndex < 0 ? Array.Empty<int>() : new[] { inspectedIndex });
            UpdateSummary();
        }

        private void UpdateSummary() {
            _summary.text = _session.Summary();
            _scriptAcknowledgement.SetValueWithoutNotify(_session.ReviewedScripts);
            _delete.SetEnabled(_session.SelectedItems().Count > 0 && _operation == null);
        }

        private void DrawDetails(ProjectCleanerItem item) {
            _details.Clear();
            var scan = _session.Scan;
            if(item == null) {
                _details.Add(new Label("Select a row to inspect its evidence."));
                if(scan != null) foreach(var coverage in scan.Coverage) _details.Add(WrappedLabel(coverage));
                DrawKeepManager();
                return;
            }
            _details.Add(WrappedLabel(item.AssetPath));
            _details.Add(WrappedLabel(item.Reason));
            _details.Add(WrappedLabel("GUID: " + item.Guid + "\n" + item.Confidence));
            _details.Add(DetailButton("Ping asset", () => _services.Ping(item.AssetPath)));
            _details.Add(DetailButton("Open asset", () => _services.Open(item.AssetPath)));
            _details.Add(DetailButton(item.IsKept ? "Remove exact Keep (parent Keep may still apply)" : "Keep asset / folder", () => {
                ProjectCleanerSettings.instance.SetKeep(item.AssetPath, !item.IsKept);
                ProjectCleaner.Reclassify(_session.Scan);
                _session.SetSelected(item, false);
                UpdateView();
                DrawDetails(item);
            }));
            _details.Add(new Label("Referenced by:"));
            foreach(var reference in item.ReferencedBy.OrderBy(r => r.Source)) {
                var source = reference.Source;
                _details.Add(DetailButton(reference.Evidence + ": " + source, () => _services.Ping(source)));
            }
            _details.Add(new Label("Dependencies:"));
            if(scan != null) {
                foreach(var reference in scan.References.Where(r => r.Source == item.AssetPath).OrderBy(r => r.Target)) {
                    var target = reference.Target;
                    _details.Add(DetailButton(reference.Evidence + ": " + target, () => _services.Ping(target)));
                }
                foreach(var coverage in scan.Coverage) _details.Add(WrappedLabel(coverage));
            }
            DrawKeepManager();
        }

        private void DrawKeepManager() {
            var kept = new Foldout { text = "Manage all Keep decisions", value = false };
            foreach(var path in ProjectCleanerSettings.instance.KeptPaths.ToArray()) {
                var captured = path;
                kept.Add(DetailButton("Unkeep: " + path, () => {
                    ProjectCleanerSettings.instance.SetKeep(captured, false);
                    if(_session.Scan != null) ProjectCleaner.Reclassify(_session.Scan);
                    UpdateView();
                    DrawDetails(null);
                }));
            }
            foreach(var guid in ProjectCleanerSettings.instance.UnresolvedKeptGuids.ToArray()) {
                var captured = guid;
                kept.Add(DetailButton("Remove missing Keep GUID: " + guid, () => {
                    ProjectCleanerSettings.instance.RemoveMissingKeep(captured);
                    DrawDetails(null);
                }));
            }
            _details.Add(kept);
        }

        private static Label WrappedLabel(string text) {
            var label = new Label(text);
            label.AddToClassList("wrapped-label");
            return label;
        }

        private static Button DetailButton(string text, Action clicked) => new Button(clicked) { text = text };

        private void Restore() {
            var folder = _services.ChooseFolder("Choose a Project Cleaner backup session",
                _session.BackupDirectory ?? ProjectCleanerCleanup.DefaultBackupDirectory);
            if(string.IsNullOrEmpty(folder)) return;
            try {
                var preview = _services.Restore(folder, false);
                if(preview.Any(o => !o.Success)) {
                    _services.ShowMessage("Restore blocked", ProjectCleanerSession.FormatOutcomes(preview));
                    return;
                }
                if(!_services.Confirm("Restore preview", "Restore these assets without overwriting existing content?\n\n"
                    + string.Join("\n", preview.Select(o => o.AssetPath)), "Restore", "Cancel")) return;
                var outcomes = _services.Restore(folder, true);
                MarkStale("Restore completed. Rescan to refresh references.");
                _services.ShowMessage("Restore results", ProjectCleanerSession.FormatOutcomes(outcomes));
            } catch(Exception exception) { ShowError(exception); }
        }

        private void ExportCsv() {
            var path = _services.ChooseSaveFile("Export visible Project Cleaner results", ProjectCleanerPaths.ProjectRoot, "project-cleaner", "csv");
            if(string.IsNullOrEmpty(path)) return;
            try {
                _services.WriteFile(path, _session.VisibleCsv());
            } catch(IOException exception) { ShowError(exception); }
        }

        private void ShowError(Exception exception) {
            _services.LogException(exception);
            _message.text = exception.Message;
            _message.messageType = HelpBoxMessageType.Error;
        }
    }
}
