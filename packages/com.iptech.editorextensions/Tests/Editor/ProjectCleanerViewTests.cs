using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.TestFramework;

namespace IPTech.EditorTools.Tests {
    /// <summary>
    /// Drives <see cref="ProjectCleanerView"/> inside an off-screen UI Toolkit test panel; no EditorWindow is opened.
    /// </summary>
    public class ProjectCleanerViewTests : UITestFixture {
        private ProjectCleanerSession _session;
        private FakeServices _services;
        private ProjectCleanerView _view;
        private string _root;
        private string _backupRoot;

        [SetUp]
        public void SetUp() {
            panelSize = new Vector2(850, 500);
            _services = new FakeServices();
            _root = "Assets/ProjectCleanerViewTests-" + Guid.NewGuid().ToString("N");
            _backupRoot = Path.Combine(ProjectCleanerPaths.ProjectRoot, "Temp", "ProjectCleanerViewTests-" + Guid.NewGuid().ToString("N"));
        }

        [TearDown]
        public void TearDown() {
            _view?.Dispose();
            _view = null;
            if(AssetDatabase.IsValidFolder(_root)) AssetDatabase.DeleteAsset(_root);
            if(Directory.Exists(_backupRoot)) Directory.Delete(_backupRoot, true);
        }

        [Test]
        public void LoadsUxmlControlsAndStylesAndInvokesButtonsOnce() {
            var candidate = ProjectCleanerSessionTests.Item("Assets/T/candidate.asset", CleanerStatus.Candidate);
            CreateView(ProjectCleanerSessionTests.Session(candidate));

            foreach(var name in new[] { "project-cleaner-main", "project-cleaner-preview", "scan-tab-button", "settings-tab-button",
                "scan-tab", "settings-tab", "settings-header", "protected-folders-box", "editable-packages", "protected-folders",
                "add-protected-folder", "save-protection",
                "message", "progress", "summary", "details",
                "scan", "cancel-scan", "select-visible", "clear-selection", "export-csv", "restore-backup", "search", "status",
                "scope", "sort", "split", "results", "script-acknowledgement", "backup", "backup-root", "preview-selected" }) {
                Assert.That(rootVisualElement.Q(name), Is.Not.Null, name);
            }
            Assert.That(rootVisualElement.Q("project-cleaner-main").resolvedStyle.position, Is.EqualTo(Position.Absolute),
                "ProjectCleanerEditor.uss must be applied.");
            Assert.That(rootVisualElement.Q("project-cleaner-preview").resolvedStyle.display, Is.EqualTo(DisplayStyle.None));
            Assert.That(rootVisualElement.Q("scan-tab").resolvedStyle.display, Is.EqualTo(DisplayStyle.Flex));
            Assert.That(rootVisualElement.Q("settings-tab").resolvedStyle.display, Is.EqualTo(DisplayStyle.None));
            var table = rootVisualElement.Q<MultiColumnListView>("results");
            Assert.That(table.sortingMode, Is.EqualTo(ColumnSortingMode.Custom));
            Assert.That(table.columns.All(c => c.sortable), Is.True);
            Assert.That(rootVisualElement.Q<Button>("preview-selected").enabledSelf, Is.False);

            simulate.Click(rootVisualElement.Q<Button>("select-visible"));
            Assert.That(_session.Selected, Is.EqualTo(new[] { candidate.Guid }), "Select visible must run exactly once per click.");
            Assert.That(rootVisualElement.Q<Label>("summary").text, Does.Contain("Selected: 1"));
            Assert.That(rootVisualElement.Q<Button>("preview-selected").enabledSelf, Is.True);

            simulate.Click(rootVisualElement.Q<Button>("export-csv"));
            Assert.That(_services.Written.Count, Is.EqualTo(1));
            Assert.That(_services.Written.Single().Value, Does.Contain("candidate.asset"));

            simulate.Click(rootVisualElement.Q<Button>("clear-selection"));
            Assert.That(_session.Selected, Is.Empty);
            Assert.That(rootVisualElement.Q<Button>("preview-selected").enabledSelf, Is.False);
        }

        [Test]
        public void SettingsTabSeparatesScopeProtectionControlsFromScanFlow() {
            CreateView(ProjectCleanerSessionTests.Session());
            var scanTab = rootVisualElement.Q("scan-tab");
            var settingsTab = rootVisualElement.Q("settings-tab");
            var scanButton = rootVisualElement.Q<ToolbarToggle>("scan-tab-button");
            var settingsButton = rootVisualElement.Q<ToolbarToggle>("settings-tab-button");

            simulate.Click(settingsButton);

            Assert.That(scanTab.resolvedStyle.visibility, Is.EqualTo(Visibility.Hidden));
            Assert.That(settingsTab.resolvedStyle.display, Is.EqualTo(DisplayStyle.Flex));
            Assert.That(settingsButton.value, Is.True);
            Assert.That(scanButton.value, Is.False);
            Assert.That(settingsTab.Q<Label>("settings-header").text, Is.EqualTo("Scope and protection settings (project-local)"));
            Assert.That(settingsTab.Q("protected-folders-box").Q<Button>("add-protected-folder"), Is.Not.Null);
            Assert.That(settingsTab.Q<Toggle>("backup"), Is.Not.Null);
            Assert.That(settingsTab.Q<TextField>("backup-root"), Is.Not.Null);
            Assert.That(scanTab.Q("backup"), Is.Null);
            Assert.That(scanTab.Q("backup-root"), Is.Null);

            simulate.Click(scanButton);

            Assert.That(scanTab.resolvedStyle.display, Is.EqualTo(DisplayStyle.Flex));
            Assert.That(scanTab.resolvedStyle.visibility, Is.EqualTo(Visibility.Visible));
            Assert.That(settingsTab.resolvedStyle.display, Is.EqualTo(DisplayStyle.None));
            Assert.That(scanButton.value, Is.True);
            Assert.That(settingsButton.value, Is.False);
        }

        [Test]
        public void SplitterDefaultsToThreeQuartersOfAvailableWidth() {
            CreateView(ProjectCleanerSessionTests.Session());
            simulate.FrameUpdate();

            var split = rootVisualElement.Q<TwoPaneSplitView>("split");
            Assert.That(split.fixedPane.resolvedStyle.width, Is.EqualTo(split.resolvedStyle.width * 0.75f).Within(1));
            Assert.That(split.Q("unity-dragline-anchor").resolvedStyle.left,
                Is.EqualTo(split.resolvedStyle.width * 0.75f).Within(1), "The divider must align with the fixed pane.");
        }

        [Test]
        public void SettingsVisitAndResultsRefreshPreserveSplitterPosition() {
            CreateView(ProjectCleanerSessionTests.Session());
            var split = rootVisualElement.Q<TwoPaneSplitView>("split");
            split.fixedPaneInitialDimension = 360;
            simulate.FrameUpdate();

            simulate.Click(rootVisualElement.Q<ToolbarToggle>("settings-tab-button"));
            simulate.FrameUpdate();
            simulate.Click(rootVisualElement.Q<ToolbarToggle>("scan-tab-button"));
            simulate.Click(rootVisualElement.Q<Button>("clear-selection"));
            simulate.FrameUpdate();

            Assert.That(split.fixedPane.resolvedStyle.width, Is.EqualTo(360).Within(1));
            Assert.That(split.Q("unity-dragline-anchor").resolvedStyle.left, Is.EqualTo(360).Within(1));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void ScanCompletionOrCancellationAfterSettingsVisitPreservesSplitterPosition(bool cancel) {
            CreateView(ProjectCleanerSessionTests.Session());
            var split = rootVisualElement.Q<TwoPaneSplitView>("split");
            split.fixedPaneInitialDimension = 360;
            simulate.FrameUpdate();
            simulate.Click(rootVisualElement.Q<ToolbarToggle>("settings-tab-button"));
            simulate.Click(rootVisualElement.Q<ToolbarToggle>("scan-tab-button"));
            _services.ScannerPaths = Array.Empty<string>();
            simulate.Click(rootVisualElement.Q<Button>("scan"));
            if(cancel) {
                simulate.Click(rootVisualElement.Q<Button>("cancel-scan"));
            } else {
                for(var tick = 0; tick < 100 && _view.IsScanning; tick++) {
                    foreach(var callback in _services.Ticks.ToArray()) callback();
                }
            }
            simulate.FrameUpdate();

            Assert.That(_view.IsScanning, Is.False);
            Assert.That(_session.Scan.Cancelled, Is.EqualTo(cancel));
            Assert.That(split.fixedPane.resolvedStyle.width, Is.EqualTo(360).Within(1));
            Assert.That(split.Q("unity-dragline-anchor").resolvedStyle.left, Is.EqualTo(360).Within(1));
        }

        [Test]
        public void SettingsControlsReadAndWriteOnlyInjectedSettings() {
            var session = ProjectCleanerSessionTests.Session();
            session.Settings.IncludeEditablePackages.Returns(true);
            session.Settings.ProtectedFolders.Returns(new[] { "Assets/Protected" });
            CreateView(session);
            simulate.Click(rootVisualElement.Q<ToolbarToggle>("settings-tab-button"));
            simulate.FrameUpdate();

            var packages = rootVisualElement.Q<Toggle>("editable-packages");
            Assert.That(packages.value, Is.True);
            packages.value = false;
            session.Settings.Received().IncludeEditablePackages = false;
            Assert.That(_session.Stale, Is.True);
            Assert.That(rootVisualElement.Q<ScrollView>("protected-folders").Query<Label>().ToList()
                .Any(label => label.text == "Assets/Protected"), Is.True);
            simulate.Click(rootVisualElement.Q<Button>("save-protection"));

            session.Settings.Received(1).SetProtectedFolders(Arg.Is<IEnumerable<string>>(paths =>
                paths.SequenceEqual(new[] { "Assets/Protected" })));
        }

        [Test]
        public void ProtectedFolderCanBeAddedWithDirectoryChooserAndRemovedFromList() {
            AssetDatabase.CreateFolder("Assets", Path.GetFileName(_root));
            CreateView(ProjectCleanerSessionTests.Session());
            simulate.Click(rootVisualElement.Q<ToolbarToggle>("settings-tab-button"));
            simulate.FrameUpdate();
            _services.SelectedFolder = ProjectCleanerPaths.FullPath(_root);

            simulate.Click(rootVisualElement.Q<Button>("add-protected-folder"));

            var folders = rootVisualElement.Q<ScrollView>("protected-folders");
            Assert.That(_services.ChooseFolderCalls, Is.EqualTo(1));
            Assert.That(folders.Query<Label>().ToList().Any(label => label.text == _root), Is.True);
            simulate.FrameUpdate();
            simulate.Click(folders.Query<Button>().ToList().Single());

            Assert.That(folders.Query<Label>().ToList().Any(label => label.text == _root), Is.False);
            Assert.That(folders.Query<Label>().ToList().Single().text, Is.EqualTo("No protected folders."));
        }

        [Test]
        public void BlockedScanReportsReasonWithoutStarting() {
            CreateView(ProjectCleanerSessionTests.Session());
            _services.ScanBlockedReason = "Busy compiling.";

            simulate.Click(rootVisualElement.Q<Button>("scan"));

            Assert.That(_view.IsScanning, Is.False);
            Assert.That(_services.Ticks, Is.Empty);
            Assert.That(rootVisualElement.Q<HelpBox>("message").text, Is.EqualTo("Busy compiling."));
            Assert.That(_session.Stale, Is.True);
        }

        [TestCase("selected", false)]
        [TestCase("selected", true)]
        [TestCase("path", false)]
        [TestCase("path", true)]
        [TestCase("type", false)]
        [TestCase("type", true)]
        [TestCase("status", false)]
        [TestCase("status", true)]
        [TestCase("bytes", false)]
        [TestCase("bytes", true)]
        [TestCase("refs", false)]
        [TestCase("refs", true)]
        public void HeaderSortingUpdatesSessionDropdownAndKeepsInspectedRow(string column, bool descending) {
            var (low, high) = ProjectCleanerSessionTests.SortPair();
            var session = ProjectCleanerSessionTests.Session(high, low);
            session.Status = "All";
            session.Selected.Add(high.Guid);
            CreateView(session);
            var table = rootVisualElement.Q<MultiColumnListView>("results");
            table.SetSelection(_session.Visible.IndexOf(high));

            table.sortColumnDescriptions.Clear();
            table.sortColumnDescriptions.Add(new SortColumnDescription(column, descending ? SortDirection.Descending : SortDirection.Ascending));
            simulate.FrameUpdate();

            Assert.That(_session.SortColumn, Is.EqualTo(column));
            Assert.That(_session.SortDescending, Is.EqualTo(descending));
            Assert.That(rootVisualElement.Q<DropdownField>("sort").value, descending ? Does.EndWith(ProjectCleanerSession.Descending)
                : Does.Not.EndWith(ProjectCleanerSession.Descending));
            Assert.That(_session.Visible, Is.EqualTo(descending ? new[] { high, low } : new[] { low, high }));
            Assert.That(table.selectedItem, Is.SameAs(high), "Sorting must keep the inspected row selected.");
        }

        [Test]
        public void ConfirmedDeletionRemovesOnlySuccessfulRowsAndClearsDetails() {
            var deleted = ProjectCleanerSessionTests.Item("Assets/T/DeadBeef.mat", CleanerStatus.Candidate);
            var failed = ProjectCleanerSessionTests.Item("Assets/T/failed.mat", CleanerStatus.Candidate);
            CreateView(ProjectCleanerSessionTests.Session(deleted, failed));
            _services.DeleteResult = new List<CleanerOutcome> {
                new CleanerOutcome { AssetPath = deleted.AssetPath, Success = true, Message = "Deleted." },
                new CleanerOutcome { AssetPath = failed.AssetPath, Success = false, Message = "Unity refused deletion." }
            };
            var table = rootVisualElement.Q<MultiColumnListView>("results");
            table.SetSelection(_session.Visible.IndexOf(deleted));
            var details = rootVisualElement.Q<ScrollView>("details");
            Assert.That(details.Query<Label>().ToList().Any(l => l.text.Contains("DeadBeef.mat")), Is.True);

            simulate.Click(rootVisualElement.Q<Button>("select-visible"));
            ShowPreview();
            simulate.Click(rootVisualElement.Q<Button>("project-cleaner-preview-confirm"));
            WaitForTransition();

            Assert.That(_services.Deleted.Select(i => i.AssetPath), Is.EquivalentTo(new[] { deleted.AssetPath, failed.AssetPath }));
            Assert.That(_session.Scan.Items, Is.EqualTo(new[] { failed }));
            Assert.That(table.itemsSource, Is.EqualTo(new[] { failed }));
            Assert.That(table.selectedItem, Is.Null);
            Assert.That(details.Query<Label>().ToList().Any(l => l.text.Contains("DeadBeef.mat")), Is.False);
            Assert.That(rootVisualElement.Q<Label>("summary").text, Does.Contain("Visible: 1").And.Contain("STALE"));
            Assert.That(rootVisualElement.Q<HelpBox>("message").messageType, Is.EqualTo(HelpBoxMessageType.Warning));
            Assert.That(_services.Messages.Single().title, Is.EqualTo("Cleanup results"));
            Assert.That(_services.Messages.Single().message, Does.Contain("Unity refused deletion."));
            AssertMainPageShowing();
        }

        [Test]
        public void PreviewSlidesAndCancelPreservesSplitterAndSelection() {
            var first = ProjectCleanerSessionTests.Item("Assets/T/first.asset", CleanerStatus.Candidate);
            var second = ProjectCleanerSessionTests.Item("Assets/T/second.asset", CleanerStatus.Candidate);
            CreateView(ProjectCleanerSessionTests.Session(first, second));
            var split = rootVisualElement.Q<TwoPaneSplitView>("split");
            split.fixedPane.style.width = 360;
            simulate.FrameUpdate();
            var table = rootVisualElement.Q<MultiColumnListView>("results");
            table.SetSelection(_session.Visible.IndexOf(second));
            simulate.Click(rootVisualElement.Q<Button>("select-visible"));

            for(var visit = 0; visit < 2; visit++) {
                ShowPreview();
                var main = rootVisualElement.Q("project-cleaner-main");
                var preview = rootVisualElement.Q("project-cleaner-preview");
                Assert.That(main.style.translate.value.x.value, Is.EqualTo(-100));
                Assert.That(preview.style.translate.value.x.value, Is.EqualTo(0));
                Assert.That(main.resolvedStyle.visibility, Is.EqualTo(Visibility.Hidden));
                Assert.That(rootVisualElement.Q<ListView>("project-cleaner-preview-assets").itemsSource.Count, Is.EqualTo(2));

                simulate.Click(rootVisualElement.Q<Button>("project-cleaner-preview-cancel"));
                WaitForTransition();

                AssertMainPageShowing();
                Assert.That(preview.style.translate.value.x.value, Is.EqualTo(100));
                Assert.That(split.fixedPane.resolvedStyle.width, Is.EqualTo(360).Within(1),
                    "Returning from the preview must not reset the splitter position.");
                Assert.That(table.selectedItem, Is.SameAs(second));
                Assert.That(_session.Selected, Is.EquivalentTo(new[] { first.Guid, second.Guid }));
                Assert.That(_services.Deleted, Is.Empty);
            }
        }

        [Test]
        public void BackupAndDeleteButtonRemovesTemporaryMaterial() {
            AssetDatabase.CreateFolder("Assets", Path.GetFileName(_root));
            var path = _root + "/DeadBeef.mat";
            AssetDatabase.CreateAsset(new Material(Shader.Find("Hidden/InternalErrorShader")), path);
            var full = ProjectCleanerPaths.FullPath(path);
            var original = File.ReadAllBytes(full);
            var originalMeta = File.ReadAllBytes(full + ".meta");
            var item = new ProjectCleanerItem {
                AssetPath = path, Guid = AssetDatabase.AssetPathToGUID(path), Category = "Material",
                Status = CleanerStatus.Candidate, CanDelete = true
            };
            var session = ProjectCleanerSessionTests.Session(item);
            session.BackupDirectory = _backupRoot;
            _services.UseRealDelete = true;
            CreateView(session);

            simulate.Click(rootVisualElement.Q<Button>("select-visible"));
            ShowPreview();
            var confirm = rootVisualElement.Q<Button>("project-cleaner-preview-confirm");
            Assert.That(confirm.text, Does.Contain("Back up"));
            simulate.Click(confirm);
            WaitForTransition();

            Assert.That(_services.Outcomes.Single().Success, Is.True, _services.Outcomes.Single().Message);
            Assert.That(File.Exists(full), Is.False);
            Assert.That(File.Exists(full + ".meta"), Is.False);
            Assert.That(_session.Scan.Items, Is.Empty);
            Assert.That(rootVisualElement.Q<MultiColumnListView>("results").itemsSource, Is.Empty);
            AssertMainPageShowing();
            var backup = Directory.GetDirectories(_backupRoot).Single();
            Assert.That(File.ReadAllBytes(Path.Combine(backup, path)), Is.EqualTo(original));
            Assert.That(File.ReadAllBytes(Path.Combine(backup, path + ".meta")), Is.EqualTo(originalMeta));
            var manifest = JsonUtility.FromJson<CleanerBackupManifest>(File.ReadAllText(Path.Combine(backup, "manifest.json")));
            Assert.That(manifest.Entries.Single().Guid, Is.EqualTo(item.Guid));
        }

        private void CreateView(ProjectCleanerSession session) {
            _session = session;
            _view = new ProjectCleanerView(rootVisualElement, session, _services);
            simulate.FrameUpdate();
        }

        private void ShowPreview() {
            simulate.Click(rootVisualElement.Q<Button>("preview-selected"));
            WaitForTransition();
            Assert.That(rootVisualElement.Q("project-cleaner-preview").resolvedStyle.display, Is.EqualTo(DisplayStyle.Flex));
        }

        // The slide uses nested scheduled callbacks, so step several frames rather than one long frame.
        private void WaitForTransition() {
            for(var frame = 0; frame < 25; frame++) simulate.FrameUpdateMs(20);
        }

        private void AssertMainPageShowing() {
            var main = rootVisualElement.Q("project-cleaner-main");
            Assert.That(main.style.translate.value.x.value, Is.EqualTo(0));
            Assert.That(main.resolvedStyle.visibility, Is.EqualTo(Visibility.Visible));
            Assert.That(rootVisualElement.Q("project-cleaner-preview").resolvedStyle.display, Is.EqualTo(DisplayStyle.None));
        }

        private sealed class FakeServices : IProjectCleanerServices {
            public readonly List<(string title, string message)> Messages = new List<(string, string)>();
            public readonly Dictionary<string, string> Written = new Dictionary<string, string>();
            public readonly List<Action> Ticks = new List<Action>();
            public readonly List<ProjectCleanerItem> Deleted = new List<ProjectCleanerItem>();
            public List<CleanerOutcome> DeleteResult = new List<CleanerOutcome>();
            public List<CleanerOutcome> Outcomes;
            public bool UseRealDelete;
            public string SelectedFolder;
            public int ChooseFolderCalls;
            public IEnumerable<string> ScannerPaths;

            public string ScanBlockedReason { get; set; }
            public ProjectCleanerScanner CreateScanner(IProjectCleanerSettings settings) => ScannerPaths == null
                ? throw new NotSupportedException("Set ScannerPaths to exercise a scoped scan.")
                : new ProjectCleanerScanner(settings, ScannerPaths);

            public IDisposable EveryUpdate(Action tick) {
                Ticks.Add(tick);
                return new Disposable(() => Ticks.Remove(tick));
            }

            public bool IsFolder(string assetPath) => AssetDatabase.IsValidFolder(assetPath);

            public List<CleanerOutcome> Delete(IEnumerable<ProjectCleanerItem> items, bool reviewedScripts, bool backup, string backupRoot, IProjectCleanerSettings settings) {
                var list = items.ToList();
                Deleted.AddRange(list);
                Outcomes = UseRealDelete ? ProjectCleanerCleanup.Delete(list, reviewedScripts, backup, backupRoot, settings) : DeleteResult;
                return Outcomes;
            }

            public List<CleanerOutcome> Restore(string session, bool apply, IProjectCleanerSettings settings) => new List<CleanerOutcome>();
            public void ShowMessage(string title, string message) => Messages.Add((title, message));
            public bool Confirm(string title, string message, string ok, string cancel) => false;
            public string ChooseFolder(string title, string directory) {
                ChooseFolderCalls++;
                return SelectedFolder ?? "";
            }
            public string ChooseSaveFile(string title, string directory, string defaultName, string extension) => "/fake/" + defaultName + "." + extension;
            public void WriteFile(string path, string contents) => Written[path] = contents;
            public void Ping(string assetPath) { }
            public void Open(string assetPath) { }
            public void LogException(Exception exception) => throw exception;
        }

        private sealed class Disposable : IDisposable {
            private Action _dispose;
            public Disposable(Action dispose) => _dispose = dispose;
            public void Dispose() { _dispose?.Invoke(); _dispose = null; }
        }
    }
}
