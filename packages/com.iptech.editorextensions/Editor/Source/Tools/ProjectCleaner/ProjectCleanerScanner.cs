using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace IPTech.EditorTools {
    public sealed class ProjectCleanerScanner : IDisposable {
        private static readonly Regex AssetGuid = new Regex(@"m_AssetGUID:\s*([0-9a-fA-F]{32})", RegexOptions.Compiled);
        private static readonly Regex Guid = new Regex(@"guid:\s*([0-9a-fA-F]{32})", RegexOptions.Compiled);
        private static readonly Regex Identifier = new Regex(@"@?\b[A-Za-z_][A-Za-z_0-9]*\b", RegexOptions.Compiled);
        private static readonly Regex Declaration = new Regex(@"\b(?:class|struct|interface|enum|record)\s+([A-Za-z_][A-Za-z_0-9]*)", RegexOptions.Compiled);
        private static readonly Regex NonCode = new Regex("//[^\\r\\n]*|/\\*[\\s\\S]*?\\*/|@\"(?:\"\"|[^\"])*\"|\"(?:\\\\.|[^\"\\\\])*\"|'(?:\\\\.|[^'\\\\])*'", RegexOptions.Compiled);
        private static readonly string[] ProtectedExtensions = {
            ".asmdef", ".asmref", ".dll", ".rsp", ".json", ".xml", ".csv", ".txt", ".md", ".cginc", ".hlsl", ".spriteatlas", ".uxml", ".uss"
        };
        private readonly IProjectCleanerSettings _settings;
        private readonly IEnumerator<bool> _steps;
        private readonly Dictionary<string, ProjectCleanerItem> _items = new Dictionary<string, ProjectCleanerItem>();
        private readonly Dictionary<string, string> _scriptCode = new Dictionary<string, string>();
        private readonly HashSet<string> _edges = new HashSet<string>();
        private readonly Dictionary<string, string> _roots = new Dictionary<string, string>();
        private string _currentPath;
        private readonly IEnumerable<string> _sources;

        public ProjectCleanerScan Result { get; } = new ProjectCleanerScan();

        public ProjectCleanerScanner(IProjectCleanerSettings settings, IEnumerable<string> sources = null) {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _sources = sources;
            Result.IsScoped = sources != null;
            _steps = Run().GetEnumerator();
        }

        public bool Step() {
            if(Result.Complete || Result.Cancelled) return false;
            try {
                if(_steps.MoveNext()) return true;
                Result.Complete = true;
                Result.Progress = 1;
                Result.Stage = "Complete";
                Dispose();
                return false;
            } catch(Exception exception) {
                var error = "Scan failed at " + _currentPath + ": " + exception.Message;
                Result.Errors.Add(error);
                Result.Stage = "Failed";
                Debug.LogException(exception);
                Dispose();
                return false;
            }
        }

        public void Cancel() {
            Result.Cancelled = true;
            Result.Stage = "Cancelled - deletion disabled";
            Dispose();
        }

        public void Dispose() => _steps.Dispose();

        private IEnumerable<bool> Run() {
            var paths = (_sources ?? AssetDatabase.GetAllAssetPaths()).Where(ProjectCleanerPaths.IsLogicalPath).Distinct().ToArray();
            var addressables = new AddressablesDetector();
            Result.Coverage.Add(addressables.Available ? "Addressables registration and serialized AssetReferences enabled." : "Addressables not installed; registration detection not applicable.");
            Result.Coverage.Add("Direct Unity dependencies, supplemental TerrainData references, and lexical C# identifiers. Runtime string loads/reflection are not proven.");
            foreach(var scene in EditorBuildSettings.scenes) _roots[scene.path] = "Build-list scene.";
            foreach(var field in typeof(NamedBuildTarget).GetFields(BindingFlags.Public | BindingFlags.Static)) {
                if(field.Name == "Unknown" || field.FieldType != typeof(NamedBuildTarget)) continue;
                var target = (NamedBuildTarget)field.GetValue(null);
                foreach(var icon in PlayerSettings.GetIcons(target, IconKind.Any)) {
                    if(icon != null) _roots[AssetDatabase.GetAssetPath(icon)] = "PlayerSettings icon.";
                }
            }
            Result.Stage = "Indexing assets";
            for(var index = 0; index < paths.Length; index++) {
                _currentPath = paths[index];
                var fullPath = ProjectCleanerPaths.FullPath(_currentPath);
                var directory = AssetDatabase.IsValidFolder(_currentPath);
                var type = AssetDatabase.GetMainAssetTypeAtPath(_currentPath);
                var script = string.Equals(Path.GetExtension(_currentPath), ".cs", StringComparison.OrdinalIgnoreCase);
                var item = new ProjectCleanerItem {
                    Guid = AssetDatabase.AssetPathToGUID(_currentPath),
                    AssetPath = _currentPath,
                    DisplayName = Path.GetFileName(_currentPath),
                    Category = directory ? "Folder" : script ? "C# Script" : type?.Name ?? "Unknown",
                    IsDirectory = directory,
                    IsScript = script,
                    Bytes = !directory && File.Exists(fullPath) ? new FileInfo(fullPath).Length : 0
                };
                if(type == null || (!directory && type == typeof(DefaultAsset))) _roots[_currentPath] = "Unknown or unsupported asset type.";
                if(type?.FullName == "UnityEditor.Build.Profile.BuildProfile") _roots[_currentPath] = "Unity Build Profile.";
                if(addressables.IsRegistered(item.Guid)) _roots[_currentPath] = "Registered Addressable (including folder-entry membership).";
                if(script) {
                    var monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(_currentPath);
                    var classType = monoScript != null ? monoScript.GetClass() : null;
                    if(classType != null && typeof(MonoBehaviour).IsAssignableFrom(classType)) item.Category = "MonoBehaviour";
                    var source = File.ReadAllText(fullPath);
                    _scriptCode[_currentPath] = source;
                    if(IsEntryPoint(classType, source)) _roots[_currentPath] = "Editor / Unity entry point or ScriptableObject type.";
                }
                _items.Add(_currentPath, item);
                Result.Items.Add(item);
                Result.Progress = paths.Length == 0 ? 0 : 0.25f * (index + 1) / paths.Length;
                yield return true;
            }

            Result.Stage = "Inspecting ProjectSettings roots";
            foreach(var settingsFile in Directory.GetFiles(Path.Combine(ProjectCleanerPaths.ProjectRoot, "ProjectSettings"), "*.asset", SearchOption.AllDirectories)) {
                _currentPath = ProjectCleanerPaths.Normalize(Path.GetRelativePath(ProjectCleanerPaths.ProjectRoot, settingsFile));
                foreach(Match match in Guid.Matches(File.ReadAllText(settingsFile))) {
                    var target = AssetDatabase.GUIDToAssetPath(match.Groups[1].Value);
                    if(!_items.ContainsKey(target)) continue;
                    _roots[target] = "Referenced by " + _currentPath + ".";
                    AddReference(_currentPath, target, CleanerEvidence.ProjectSettings);
                }
                yield return true;
            }
            Result.Coverage.Add("Serialized ProjectSettings GUID roots (including graphics/quality, cursors and splash settings).");

            Result.Stage = "Mapping direct dependencies";
            for(var index = 0; index < paths.Length; index++) {
                _currentPath = paths[index];
                if(!_items[_currentPath].IsDirectory) {
                    Result.DependencyCalls++;
                    foreach(var target in AssetDatabase.GetDependencies(_currentPath, false)) {
                        AddReference(_currentPath, target, CleanerEvidence.UnityDependency);
                    }
                    if(IsSerializedAsset(_currentPath)) ScanSerializedAsset(_currentPath);
                }
                Result.Progress = 0.25f + 0.5f * (index + 1) / Math.Max(1, paths.Length);
                yield return true;
            }

            Result.Stage = "Inspecting C# identifiers";
            var declarations = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
            foreach(var script in _scriptCode) {
                foreach(var name in GetDeclaredTypeNames(script.Value)) {
                    if(!declarations.TryGetValue(name, out var declaringPaths)) {
                        declaringPaths = new HashSet<string>();
                        declarations.Add(name, declaringPaths);
                    }
                    declaringPaths.Add(script.Key);
                }
            }
            var processed = 0;
            foreach(var script in _scriptCode) {
                _currentPath = script.Key;
                foreach(var identifier in GetIdentifierNames(script.Value)) {
                    if(!declarations.TryGetValue(identifier, out var targets)) continue;
                    foreach(var target in targets) AddReference(script.Key, target, CleanerEvidence.SourceIdentifier);
                }
                Result.Progress = 0.75f + 0.15f * ++processed / Math.Max(1, _scriptCode.Count);
                yield return true;
            }

            Result.Stage = "Checking folders";
            var empty = new HashSet<string>();
            foreach(var item in Result.Items.Where(i => i.IsDirectory).OrderByDescending(i => i.AssetPath.Length)) {
                _currentPath = item.AssetPath;
                if(ProjectCleanerPaths.IsWritableTarget(item.AssetPath, _settings.IncludeEditablePackages)) {
                    var full = ProjectCleanerPaths.FullPath(item.AssetPath);
                    ProjectCleanerPaths.RejectLinks(full);
                    var children = Directory.GetDirectories(full);
                    var allChildrenEmpty = children.All(child => empty.Contains(ProjectCleanerPaths.Normalize(child)));
                    var files = Directory.GetFiles(full);
                    var onlyChildMetadata = files.All(file => file.EndsWith(".meta", StringComparison.Ordinal)
                        && Directory.Exists(file.Substring(0, file.Length - 5)));
                    if(allChildrenEmpty && onlyChildMetadata) {
                        item.Category = "Empty Directory";
                        empty.Add(ProjectCleanerPaths.Normalize(full));
                    }
                }
                yield return true;
            }
            foreach(var root in _roots) {
                if(_items.TryGetValue(root.Key, out var item)) {
                    item.IsRoot = true;
                    item.ProtectionReason = root.Value;
                }
            }
            _settings.MigrateKeepPaths();
            Classify(Result, _settings);
        }

        private static bool IsSerializedAsset(string path) {
            var extension = Path.GetExtension(path);
            return extension == ".asset" || extension == ".prefab" || extension == ".unity";
        }

        public static IEnumerable<string> GetDeclaredTypeNames(string source) {
            return Declaration.Matches(NonCode.Replace(source, " ")).Cast<Match>().Select(m => m.Groups[1].Value).Distinct();
        }

        public static IEnumerable<string> GetIdentifierNames(string source) {
            return Identifier.Matches(NonCode.Replace(source, " ")).Cast<Match>().Select(m => m.Value.TrimStart('@')).Distinct();
        }

        private void AddReference(string source, string target, CleanerEvidence evidence) {
            if(source == target || !_items.ContainsKey(target)) return;
            var key = source + "\n" + target + "\n" + evidence;
            if(!_edges.Add(key)) return;
            var reference = new CleanerReference { Source = source, Target = target, Evidence = evidence };
            Result.References.Add(reference);
            _items[target].ReferencedBy.Add(reference);
        }

        private void ScanSerializedAsset(string path) {
            var full = ProjectCleanerPaths.FullPath(path);
            using(var reader = new StreamReader(full)) {
                var prefix = reader.ReadLine();
                if(prefix != null && prefix.StartsWith("%YAML", StringComparison.Ordinal)) {
                    var text = reader.ReadToEnd();
                    foreach(Match match in AssetGuid.Matches(text)) {
                        AddReference(path, AssetDatabase.GUIDToAssetPath(match.Groups[1].Value), CleanerEvidence.AssetReference);
                    }
                    foreach(Match match in Guid.Matches(text)) {
                        var target = AssetDatabase.GUIDToAssetPath(match.Groups[1].Value);
                        if(_items.TryGetValue(target, out var item) && item.Category == nameof(TerrainData)) {
                            AddReference(path, target, CleanerEvidence.Terrain);
                        }
                    }
                    return;
                }
            }
            if(path.EndsWith(".unity", StringComparison.Ordinal)) {
                var scene = EditorSceneManager.OpenPreviewScene(path);
                try {
                    foreach(var root in scene.GetRootGameObjects()) InspectHierarchy(path, root);
                } finally {
                    EditorSceneManager.ClosePreviewScene(scene);
                }
            } else if(path.EndsWith(".prefab", StringComparison.Ordinal)) {
                var root = PrefabUtility.LoadPrefabContents(path);
                try { InspectHierarchy(path, root); }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            } else {
                foreach(var obj in AssetDatabase.LoadAllAssetsAtPath(path)) InspectObject(path, obj);
            }
        }

        private void InspectHierarchy(string path, GameObject root) {
            foreach(var component in root.GetComponentsInChildren<Component>(true)) {
                if(component == null) throw new InvalidOperationException("Missing script in " + path + "; binary detector coverage is incomplete.");
                InspectObject(path, component);
            }
        }

        private void InspectObject(string path, Object obj) {
            if(obj == null) return;
            using(var serialized = new SerializedObject(obj)) {
                var property = serialized.GetIterator();
                while(property.Next(true)) {
                    if(property.propertyType == SerializedPropertyType.String && property.name == "m_AssetGUID") {
                        AddReference(path, AssetDatabase.GUIDToAssetPath(property.stringValue), CleanerEvidence.AssetReference);
                    } else if(property.propertyType == SerializedPropertyType.ObjectReference && property.objectReferenceValue is TerrainData terrain) {
                        AddReference(path, AssetDatabase.GetAssetPath(terrain), CleanerEvidence.Terrain);
                    }
                }
            }
        }

        private static bool IsEntryPoint(Type type, string source) {
            if(type != null && (typeof(EditorWindow).IsAssignableFrom(type) || typeof(SettingsProvider).IsAssignableFrom(type)
                || typeof(ScriptableObject).IsAssignableFrom(type))) return true;
            return new[] { "MenuItem", "InitializeOnLoad", "RuntimeInitializeOnLoadMethod", "SettingsProvider", "CustomEditor",
                "CustomPropertyDrawer", "AssetPostprocessor", "ScriptedImporter", "DidReloadScripts", "CreateAssetMenu",
                "IPreprocessBuild", "IPostprocessBuild", "OnOpenAsset" }.Any(source.Contains);
        }

        public static void Classify(ProjectCleanerScan scan, IProjectCleanerSettings settings) {
            var protectedPaths = new HashSet<string>();
            var kept = settings.KeptPaths.ToArray();
            var incoming = scan.References.GroupBy(r => r.Target).ToDictionary(g => g.Key, g => g.ToList());
            foreach(var item in scan.Items) {
                item.ReferencedBy.Clear();
                if(incoming.TryGetValue(item.AssetPath, out var references)) item.ReferencedBy.AddRange(references);
                item.IsKept = kept.Any(path => ProjectCleanerPaths.Contains(path, item.AssetPath));
                if(item.IsKept || IsConventionProtected(item.AssetPath) || settings.IsProtectedFolder(item.AssetPath)
                    || item.IsRoot) protectedPaths.Add(item.AssetPath);
            }
            var outgoing = scan.References.GroupBy(r => r.Source).ToDictionary(g => g.Key, g => g.Select(r => r.Target).Distinct());
            var pending = new Queue<string>(protectedPaths);
            while(pending.Count > 0) {
                if(!outgoing.TryGetValue(pending.Dequeue(), out var targets)) continue;
                foreach(var target in targets) if(protectedPaths.Add(target)) pending.Enqueue(target);
            }
            foreach(var item in scan.Items) {
                var rootReason = item.IsRoot;
                item.CanDelete = false;
                if(rootReason || protectedPaths.Contains(item.AssetPath) || !ProjectCleanerPaths.IsWritableTarget(item.AssetPath, settings.IncludeEditablePackages)
                    || (item.IsDirectory && item.Category != "Empty Directory")) {
                    item.Status = CleanerStatus.Protected;
                    item.Reason = rootReason ? item.ProtectionReason ?? "Project root / entry point."
                        : item.IsKept ? "Kept explicitly or by a parent folder."
                        : IsConventionProtected(item.AssetPath) ? "Protected path / file convention."
                        : settings.IsProtectedFolder(item.AssetPath) ? "Protected by a configured folder rule."
                        : !ProjectCleanerPaths.IsWritableTarget(item.AssetPath, settings.IncludeEditablePackages) ? "Root or out-of-scope / read-only package."
                        : item.IsDirectory && item.Category != "Empty Directory" ? "Folder is not empty."
                        : "Dependency of a protected asset.";
                } else if(item.ReferencedBy.Count > 0) {
                    item.Status = CleanerStatus.Referenced;
                    item.Reason = "Detected references: " + item.ReferencedBy.Select(r => r.Source).Distinct().Count();
                } else {
                    item.Status = item.IsScript ? CleanerStatus.ReviewRequired : CleanerStatus.Candidate;
                    item.Reason = item.IsScript ? "No detected references. Code liveness is not proven; explicit review is required."
                        : "No detected direct references. Runtime use is still possible; review before deleting.";
                    item.CanDelete = scan.Errors.Count == 0;
                }
                item.Confidence = item.IsScript ? "Review required" : "Static evidence only";
            }
            foreach(var folder in scan.Items.Where(i => i.IsDirectory && i.CanDelete)) {
                if(scan.Items.Any(i => i != folder && ProjectCleanerPaths.Contains(folder.AssetPath, i.AssetPath) && !i.CanDelete)) {
                    folder.CanDelete = false;
                    folder.Status = CleanerStatus.Protected;
                    folder.Reason = "Contains a protected child folder.";
                }
            }
        }

        private static bool IsConventionProtected(string path) {
            return path.Split('/').Any(segment => new[] { "Resources", "Editor", "Editor Default Resources", "ThirdParty", "Tests", "StreamingAssets" }
                    .Contains(segment, StringComparer.OrdinalIgnoreCase))
                || ProtectedExtensions.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase);
        }

        private sealed class AddressablesDetector {
            private readonly Object _settings;
            private readonly MethodInfo _find;
            public bool Available { get; }

            public AddressablesDetector() {
                var installed = UnityEditor.PackageManager.PackageInfo.GetAllRegisteredPackages().Any(p => p.name == "com.unity.addressables");
                if(!installed) return;
                Available = true;
                var type = TypeCache.GetTypesDerivedFrom<ScriptableObject>()
                    .FirstOrDefault(t => t.FullName == "UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject");
                if(type == null) throw new InvalidOperationException("Addressables is installed but its editor API could not be loaded.");
                _settings = type.GetProperty("Settings", BindingFlags.Public | BindingFlags.Static)?.GetValue(null) as Object;
                if(_settings == null) return;
                _find = _settings.GetType().GetMethod("FindAssetEntry", new[] { typeof(string), typeof(bool) });
                if(_find == null) throw new InvalidOperationException("Unsupported Addressables FindAssetEntry API.");
            }

            public bool IsRegistered(string guid) {
                return _settings != null && !string.IsNullOrEmpty(guid) && _find.Invoke(_settings, new object[] { guid, true }) != null;
            }
        }

#if UNITY_EDITOR
        internal SpyImp Spy => new SpyImp(this);

        internal sealed class SpyImp {
            private readonly ProjectCleanerScanner _scanner;

            public SpyImp(ProjectCleanerScanner scanner) {
                _scanner = scanner;
            }

            public void InspectHierarchy(string path, GameObject root) => _scanner.InspectHierarchy(path, root);
        }
#endif
    }
}
