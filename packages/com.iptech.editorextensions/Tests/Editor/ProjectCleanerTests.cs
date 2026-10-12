using System;
using System.IO;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace IPTech.EditorTools.Tests {
    public class ProjectCleanerTests {
        private string _root;
        private string _backupRoot;
        private IProjectCleanerSettings _settings;

        [SetUp]
        public void SetUp() {
            _root = "Assets/ProjectCleanerTests-" + Guid.NewGuid().ToString("N");
            AssetDatabase.CreateFolder("Assets", Path.GetFileName(_root));
            _backupRoot = Path.Combine(ProjectCleanerPaths.ProjectRoot, "Temp", "ProjectCleanerTests-" + Guid.NewGuid().ToString("N"));
            _settings = ProjectCleanerSessionTests.CreateSettings();
        }

        [TearDown]
        public void TearDown() {
            AssetDatabase.DeleteAsset(_root);
            if(Directory.Exists(_backupRoot)) Directory.Delete(_backupRoot, true);
        }

        [TestCase("Assets/Foo", "Assets/Foo", true)]
        [TestCase("Assets/Foo", "Assets/Foo/Bar", true)]
        [TestCase("Assets/Foo", "Assets/Foobar", false)]
        [TestCase("Packages/com.example", "Packages/com.example.other/file", false)]
        public void ContainmentRequiresAPathBoundary(string root, string path, bool expected) {
            Assert.That(ProjectCleanerPaths.Contains(root, path), Is.EqualTo(expected));
        }

        [TestCase("Assets/../outside")]
        [TestCase("Packages/com.example/../../outside")]
        [TestCase("/tmp/outside")]
        public void TraversalAndAbsolutePathsAreRejected(string path) {
            Assert.That(ProjectCleanerPaths.IsLogicalPath(path), Is.False);
            Assert.Throws<ArgumentException>(() => ProjectCleanerPaths.FullPath(path));
        }

        [Test]
        public void RegisteredPackageResolvesUsingManifestIdentity() {
            const string path = "Packages/com.iptech.editorextensions/package.json";
            var package = UnityEditor.PackageManager.PackageInfo.FindForAssetPath(path);
            Assert.That(ProjectCleanerPaths.FullPath(path), Is.EqualTo(Path.Combine(package.resolvedPath, "package.json")));
            Assert.That(ProjectCleanerPaths.IsWritableTarget("Packages/com.iptech.editorextensions", true), Is.False);
            Assert.That(ProjectCleanerPaths.IsWritableTarget(path, false), Is.False);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void GraphDetectsDirectAssetReferencesAndTerrainWithoutSelfEdges(bool textSerialization) {
            var target = CreateFixture("target");
            var reference = CreateFixture("reference");
            var terrain = new TerrainData();
            AssetDatabase.CreateAsset(terrain, _root + "/terrain.asset");
            reference.SetAssetGuid(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(target)));
            reference.DirectReference = terrain;
            reference.Terrain = terrain;
            EditorUtility.SetDirty(reference);
            AssetDatabase.SaveAssetIfDirty(reference);
            var referencePath = AssetDatabase.GetAssetPath(reference);
            var copy = Object.Instantiate(reference);
            try {
                UnityEditorInternal.InternalEditorUtility.SaveToSerializedFileAndForget(new Object[] { copy }, referencePath, textSerialization);
            } finally { Object.DestroyImmediate(copy); }
            AssetDatabase.ImportAsset(referencePath, ImportAssetOptions.ForceSynchronousImport);
            var paths = new[] { AssetDatabase.GetAssetPath(target), referencePath, AssetDatabase.GetAssetPath(terrain) };
            using(var scanner = new ProjectCleanerScanner(_settings, paths)) {
                while(scanner.Step()) { }
                var scan = scanner.Result;
                Assert.That(scan.Errors, Is.Empty);
                Assert.That(scan.Complete, Is.True);
                Assert.That(scan.CanClean, Is.False, "Scoped test/reference scans must not authorize deletion.");
                Assert.That(scan.DependencyCalls, Is.EqualTo(paths.Length));
                Assert.That(scan.References.Any(r => r.Source == paths[1] && r.Target == paths[0] && r.Evidence == CleanerEvidence.AssetReference), Is.True);
                Assert.That(scan.References.Any(r => r.Source == paths[1] && r.Target == paths[2] && r.Evidence == CleanerEvidence.Terrain), Is.True);
                Assert.That(scan.References.Any(r => r.Source == r.Target), Is.False);
                Assert.That(scan.References.Select(r => r.Source + r.Target + r.Evidence).Distinct().Count(), Is.EqualTo(scan.References.Count));
            }
        }

        [Test]
        public void ClassificationUsesInjectedKeepPathsAfterAssetMoves() {
            var asset = CreateFixture("kept");
            var path = AssetDatabase.GetAssetPath(asset);
            var moved = _root + "/moved.asset";
            Assert.That(AssetDatabase.MoveAsset(path, moved), Is.Empty);
            _settings.KeptPaths.Returns(new[] { moved });
            var item = Item(moved);
            var scan = new ProjectCleanerScan();
            scan.Items.Add(item);
            ProjectCleaner.Reclassify(scan, _settings);
            Assert.That(item.IsKept, Is.True);
            _settings.KeptPaths.Returns(Array.Empty<string>());
            ProjectCleaner.Reclassify(scan, _settings);
            Assert.That(item.IsKept, Is.False);
        }

        [Test]
        public void KeptFolderProtectsChildrenDependenciesAndEmptyFolders() {
            var child = Item(_root + "/child.asset");
            var dependency = Item("Assets/unreferenced-target.asset");
            var folder = Item(_root);
            folder.IsDirectory = true;
            folder.Category = "Empty Directory";
            var scan = new ProjectCleanerScan();
            scan.Items.AddRange(new[] { child, dependency, folder });
            scan.References.Add(new CleanerReference { Source = child.AssetPath, Target = dependency.AssetPath });
            _settings.KeptPaths.Returns(new[] { _root });
            ProjectCleaner.Reclassify(scan, _settings);
            Assert.That(scan.Items.All(i => i.Status == CleanerStatus.Protected && !i.CanDelete), Is.True);
            Assert.That(child.IsKept, Is.True);
            Assert.That(scan.Items.Contains(child), Is.True);
        }

        [Test]
        public void ReclassifyDoesNotTurnProtectedDependencyIntoPermanentRoot() {
            var root = Item(_root + "/root.asset");
            var target = Item(_root + "/target.asset");
            var scan = new ProjectCleanerScan();
            scan.Items.AddRange(new[] { root, target });
            scan.References.Add(new CleanerReference { Source = root.AssetPath, Target = target.AssetPath });
            _settings.KeptPaths.Returns(new[] { root.AssetPath });
            ProjectCleaner.Reclassify(scan, _settings);
            Assert.That(target.Status, Is.EqualTo(CleanerStatus.Protected));
            _settings.KeptPaths.Returns(Array.Empty<string>());
            ProjectCleaner.Reclassify(scan, _settings);
            Assert.That(target.Status, Is.EqualTo(CleanerStatus.Referenced));
            Assert.That(target.IsRoot, Is.False);
        }

        [Test]
        public void ScriptsRequireReviewAndScanFailuresDisableEligibility() {
            var script = Item(_root + "/Unused.cs");
            script.IsScript = true;
            var scan = new ProjectCleanerScan();
            scan.Items.Add(script);
            ProjectCleaner.Reclassify(scan, _settings);
            Assert.That(script.Status, Is.EqualTo(CleanerStatus.ReviewRequired));
            Assert.That(script.Confidence, Is.EqualTo("Review required"));
            scan.Errors.Add("Unreadable source");
            ProjectCleaner.Reclassify(scan, _settings);
            Assert.That(script.CanDelete, Is.False);
            Assert.That(scan.CanClean, Is.False);
        }

        [Test]
        public void CancelledScanCannotAuthorizeCleanup() {
            using(var scanner = new ProjectCleanerScanner(_settings)) {
                scanner.Cancel();
                Assert.That(scanner.Step(), Is.False);
                Assert.That(scanner.Result.Cancelled, Is.True);
                Assert.That(scanner.Result.CanClean, Is.False);
            }
        }

        [Test]
        public void BackupAndRestorePreserveContentMetadataAndGuid() {
            var asset = CreateFixture("restore");
            var path = AssetDatabase.GetAssetPath(asset);
            var guid = AssetDatabase.AssetPathToGUID(path);
            var bytes = File.ReadAllBytes(ProjectCleanerPaths.FullPath(path));
            var meta = File.ReadAllBytes(ProjectCleanerPaths.FullPath(path) + ".meta");
            var item = Item(path);
            var session = ProjectCleanerCleanup.Backup(new[] { item }, _backupRoot);
            Assert.That(ProjectCleanerCleanup.Restore(session, false, _settings).All(o => !o.Success), Is.True, "Existing assets are collisions.");
            Assert.That(AssetDatabase.DeleteAsset(path), Is.True);
            Assert.That(ProjectCleanerCleanup.Restore(session, true, _settings).All(o => o.Success), Is.True);
            Assert.That(File.ReadAllBytes(ProjectCleanerPaths.FullPath(path)), Is.EqualTo(bytes));
            Assert.That(File.ReadAllBytes(ProjectCleanerPaths.FullPath(path) + ".meta"), Is.EqualTo(meta));
            Assert.That(AssetDatabase.AssetPathToGUID(path), Is.EqualTo(guid));
        }

        [Test]
        public void BackupInsideAssetsIsRejectedAndOriginalSurvives() {
            var asset = CreateFixture("no-delete");
            var item = Item(AssetDatabase.GetAssetPath(asset));
            Assert.Throws<IOException>(() => ProjectCleanerCleanup.Backup(new[] { item }, ProjectCleanerPaths.FullPath(_root) + "/backup"));
            Assert.That(AssetDatabase.LoadMainAssetAtPath(item.AssetPath), Is.Not.Null);
        }

        [Test]
        public void CorruptedBackupCannotBeRestored() {
            var asset = CreateFixture("corrupt");
            var path = AssetDatabase.GetAssetPath(asset);
            var session = ProjectCleanerCleanup.Backup(new[] { Item(path) }, _backupRoot);
            AssetDatabase.DeleteAsset(path);
            File.AppendAllText(Path.Combine(session, path), "corruption");
            Assert.That(ProjectCleanerCleanup.Restore(session, true, _settings).Single().Success, Is.False);
            Assert.That(File.Exists(ProjectCleanerPaths.FullPath(path)), Is.False);
        }

        [Test]
        public void EmptyTreeRejectsRealFilesAndOrphanMetadata() {
            var full = ProjectCleanerPaths.FullPath(_root);
            Assert.That(ProjectCleanerCleanup.IsPhysicallyEmptyTree(full), Is.True);
            File.WriteAllText(Path.Combine(full, "orphan.meta"), "orphan");
            Assert.That(ProjectCleanerCleanup.IsPhysicallyEmptyTree(full), Is.False);
        }

        [Test]
        public void EmptyFolderBackupRestoresDescendantMetadata() {
            var parent = _root + "/Parent";
            var child = parent + "/Child";
            AssetDatabase.CreateFolder(_root, "Parent");
            AssetDatabase.CreateFolder(parent, "Child");
            var parentGuid = AssetDatabase.AssetPathToGUID(parent);
            var childGuid = AssetDatabase.AssetPathToGUID(child);
            var item = Item(parent);
            item.IsDirectory = true;
            var session = ProjectCleanerCleanup.Backup(new[] { item }, _backupRoot);
            AssetDatabase.DeleteAsset(parent);
            Assert.That(ProjectCleanerCleanup.Restore(session, true, _settings).All(o => o.Success), Is.True);
            Assert.That(AssetDatabase.AssetPathToGUID(parent), Is.EqualTo(parentGuid));
            Assert.That(AssetDatabase.AssetPathToGUID(child), Is.EqualTo(childGuid));
        }

        [Test]
        public void PrefabComponentAssetReferenceIsDetectedAndPreviewSceneIsClosed() {
            var target = CreateFixture("prefab-target");
            var targetPath = AssetDatabase.GetAssetPath(target);
            var prefabPath = _root + "/fixture.prefab";
            var scene = UnityEditor.SceneManagement.EditorSceneManager.NewPreviewScene();
            try {
                var root = EditorUtility.CreateGameObjectWithHideFlags("ProjectCleanerTestFixture", HideFlags.HideAndDontSave);
                UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(root, scene);
                root.hideFlags = HideFlags.None;
                root.AddComponent<CleanerReferenceComponent>().SetAssetGuid(AssetDatabase.AssetPathToGUID(targetPath));
                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                AssetDatabase.ImportAsset(prefabPath, ImportAssetOptions.ForceSynchronousImport);
            } finally {
                UnityEditor.SceneManagement.EditorSceneManager.ClosePreviewScene(scene);
            }
            var count = UnityEngine.SceneManagement.SceneManager.sceneCount;
            using(var scanner = new ProjectCleanerScanner(_settings, new[] { targetPath, prefabPath })) {
                while(scanner.Step()) { }
                Assert.That(scanner.Result.Errors, Is.Empty);
                Assert.That(scanner.Result.References.Any(r => r.Source == prefabPath && r.Target == targetPath && r.Evidence == CleanerEvidence.AssetReference), Is.True);
                var previewRoot = PrefabUtility.LoadPrefabContents(prefabPath);
                try {
                    using(var inspection = new ProjectCleanerScanner(_settings, new[] { targetPath })) {
                        while(inspection.Step()) { }
                        inspection.Spy.InspectHierarchy(prefabPath, previewRoot);
                        Assert.That(inspection.Result.References.Any(r => r.Source == prefabPath && r.Target == targetPath && r.Evidence == CleanerEvidence.AssetReference), Is.True);
                    }
                } finally { PrefabUtility.UnloadPrefabContents(previewRoot); }
            }
            Assert.That(UnityEngine.SceneManagement.SceneManager.sceneCount, Is.EqualTo(count));
        }

        [Test]
        public void ReportsQuoteCsvAndShowSourceBytes() {
            Assert.That(ProjectCleanerEditor.CsvField("a,\"b\"\nc"), Is.EqualTo("\"a,\"\"b\"\"\nc\""));
            Assert.That(ProjectCleanerEditor.FormatBytes(2048), Is.EqualTo("2 KB"));
        }

        [Test]
        public void LexicalEvidenceUsesDeclaredTypesAndIgnoresOrdinaryCommentsAndStrings() {
            var source = "class NotTheFilename {} interface IThing {} // class CommentOnly {}\n"
                + "class Second { void Method() { AddComponent<NotTheFilename>(); var t = typeof(IThing); } "
                + "string text = \"StringOnly\"; string verbatim = @\"AlsoStringOnly\"; }";
            Assert.That(ProjectCleanerScanner.GetDeclaredTypeNames(source), Is.EquivalentTo(new[] { "NotTheFilename", "IThing", "Second" }));
            var identifiers = ProjectCleanerScanner.GetIdentifierNames(source).ToArray();
            Assert.That(identifiers, Does.Contain("NotTheFilename").And.Contain("IThing"));
            Assert.That(identifiers, Does.Not.Contain("CommentOnly").And.Not.Contain("StringOnly").And.Not.Contain("AlsoStringOnly"));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void AddressablesRegistrationProtectsEntriesAndFolderMembers(bool folderEntry) {
            var defaultType = TypeCache.GetTypesDerivedFrom<ScriptableObject>()
                .FirstOrDefault(t => t.FullName == "UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject");
            if(defaultType == null) Assert.Ignore("Addressables is not installed.");
            var settings = defaultType.GetProperty("Settings").GetValue(null) as Object;
            if(settings == null) Assert.Ignore("No Addressables settings configured.");
            var group = settings.GetType().GetProperty("DefaultGroup").GetValue(settings) as Object;
            if(group == null) Assert.Ignore("No Addressables default group configured.");
            var target = CreateFixture("addressable");
            var targetPath = AssetDatabase.GetAssetPath(target);
            var entryGuid = AssetDatabase.AssetPathToGUID(folderEntry ? _root : targetPath);
            var settingsDirty = EditorUtility.IsDirty(settings);
            var groupDirty = EditorUtility.IsDirty(group);
            try {
                var create = settings.GetType().GetMethods().Single(m => m.Name == "CreateOrMoveEntry" && m.GetParameters().Length == 4);
                create.Invoke(settings, new object[] { entryGuid, group, false, false });
                using(var scanner = new ProjectCleanerScanner(_settings, new[] { _root, targetPath })) {
                    while(scanner.Step()) { }
                    Assert.That(scanner.Result.Errors, Is.Empty);
                    var item = scanner.Result.Items.Single(i => i.AssetPath == targetPath);
                    Assert.That(item.IsRoot, Is.True);
                    Assert.That(item.Status, Is.EqualTo(CleanerStatus.Protected));
                    Assert.That(item.ProtectionReason, Does.Contain("Addressable"));
                }
            } finally {
                settings.GetType().GetMethod("RemoveAssetEntry", new[] { typeof(string), typeof(bool) }).Invoke(settings, new object[] { entryGuid, false });
                // Only the temporary entry is changed; never save or clear pre-existing user edits.
                if(!groupDirty) EditorUtility.ClearDirty(group);
                if(!settingsDirty) EditorUtility.ClearDirty(settings);
            }
        }

        [Test]
        public void PreviewBindsSelectionSnapshotAndRefreshesMessagesAndRows() {
            var tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                "Packages/com.iptech.editorextensions/Editor/Source/Tools/ProjectCleaner/ProjectCleanerEditor.uxml");
            var root = tree.CloneTree();
            using(var preview = new ProjectCleanerPreview(root, root.Q<Button>("preview-selected"), () => { })) {
                var first = Item(_root + "/first.asset");
                first.Bytes = 2048;
                var selected = new System.Collections.Generic.List<ProjectCleanerItem> { first };
                preview.Show(selected, true, _backupRoot, null);
                selected.Clear();
                Assert.That(preview.IsShowing, Is.True);
                Assert.That(preview.Items, Is.EqualTo(new[] { first }));
                var list = root.Q<ListView>("project-cleaner-preview-assets");
                var row = (Label)list.makeItem();
                list.bindItem(row, 0);
                Assert.That(row.text, Is.EqualTo(first.AssetPath + "  (2 KB)"));
                Assert.That(row.tooltip, Is.EqualTo(first.AssetPath));
                Assert.That(row.ClassListContains("cell"), Is.True);
                Assert.That(root.Q<Label>("preview-summary").text, Is.EqualTo("Delete 1 assets (2 KB)?"));
                Assert.That(root.Q<HelpBox>("preview-message").text, Does.Contain(_backupRoot));
                Assert.That(root.Q<Button>("project-cleaner-preview-confirm").enabledSelf, Is.True);
                preview.MarkStale("Changed");
                Assert.That(root.Q<Button>("project-cleaner-preview-confirm").enabledSelf, Is.False);
                Assert.That(root.Q<HelpBox>("preview-message").text, Does.StartWith("Changed"));
                preview.Cancel();
                Assert.That(preview.IsShowing, Is.False);
                var second = Item(_root + "/second.asset");
                preview.Show(new[] { second }, false, _backupRoot, "Review required");
                list.bindItem(row, 0);
                Assert.That(preview.Items, Is.EqualTo(new[] { second }));
                Assert.That(row.tooltip, Is.EqualTo(second.AssetPath));
                Assert.That(root.Q<HelpBox>("preview-message").text, Is.EqualTo("Review required"));
                Assert.That(root.Q<HelpBox>("preview-message").messageType, Is.EqualTo(HelpBoxMessageType.Warning));
                Assert.That(root.Q<Button>("project-cleaner-preview-confirm").text, Is.EqualTo("Delete without backup"));
                Assert.That(root.Q<Button>("project-cleaner-preview-confirm").enabledSelf, Is.False);
            }
        }

        private CleanerReferenceFixture CreateFixture(string name) {
            var asset = ScriptableObject.CreateInstance<CleanerReferenceFixture>();
            AssetDatabase.CreateAsset(asset, _root + "/" + name + ".asset");
            return asset;
        }

        private static ProjectCleanerItem Item(string path) {
            return new ProjectCleanerItem {
                AssetPath = path, Guid = AssetDatabase.AssetPathToGUID(path), Category = "Test"
            };
        }
    }
}
