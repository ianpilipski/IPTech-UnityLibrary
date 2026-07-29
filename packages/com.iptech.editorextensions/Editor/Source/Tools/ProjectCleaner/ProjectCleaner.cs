using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.PackageManager;
using System.Reflection;
using System.Linq;

namespace IPTech.EditorTools {
    public static class ProjectCleaner {
        public static List<ProjectCleanerItem> ScanProject() {
            var retVal = new List<ProjectCleanerItem>();
            var settings = ProjectCleanerSettings.instance;
            retVal.AddRange(FindEmptyDirectories(settings));
            retVal.AddRange(FindUnusedScripts(settings));
            retVal.AddRange(FindUnreferencedMonoBehaviours(settings));
            return retVal.OrderBy(i => i.Category).ThenBy(i => i.AssetPath).ToList();
        }

        public static List<string> DeleteEmptyDirectories() {
            var retVal = GetEmptyDirectories();
            DeleteDirectories(retVal);
            return retVal;
        }

        public static int DeleteItems(List<ProjectCleanerItem> items) {
            var deletedCount = 0;
            var safeItems = items.Where(i => i.CanDelete).ToList();

            foreach(var item in safeItems) {
                var assetPath = ToAssetPath(item.AssetPath);
                if(string.IsNullOrEmpty(assetPath)) {
                    continue;
                }

                if(item.IsDirectory) {
                    if(AssetDatabase.DeleteAsset(assetPath)) {
                        deletedCount++;
                    }
                } else if(item.AssetPath.EndsWith(".cs", System.StringComparison.OrdinalIgnoreCase)) {
                    if(AssetDatabase.DeleteAsset(assetPath)) {
                        deletedCount++;
                    }
                }
            }

            if(deletedCount > 0) {
                AssetDatabase.Refresh();
            }

            return deletedCount;
        }

        static void DeleteDirectories(List<string> dirs) {
            foreach(var d in dirs) {
                if(AssetDatabase.DeleteAsset(ToAssetPath(d))) {
                    string metaFile = d + ".meta";
                    if(File.Exists(metaFile)) {
                        File.Delete(metaFile);
                    }
                }
            }

            AssetDatabase.Refresh();
        }

        public static List<string> GetEmptyDirectories() {
            var retVal = new List<string>();
            var scanRoots = GetScannableDirectoryRoots();
            if(scanRoots.Count == 0) {
                return retVal;
            }

            foreach(var scanRoot in scanRoots) {
                if(!Directory.Exists(scanRoot)) {
                    continue;
                }

                var directories = Directory.GetDirectories(scanRoot, "*", SearchOption.AllDirectories)
                    .OrderByDescending(d => d.Length)
                    .ToList();

                foreach(var dir in directories) {
                    if(IsDirEmpty(dir, retVal)) {
                        retVal.Add(dir);
                    }
                }
            }

            return retVal
                .Where(d => !scanRoots.Contains(d))
                .Distinct()
                .OrderBy(d => d)
                .ToList();

            bool IsDirEmpty(string dir, List<string> collected) {
                var files = Directory.GetFiles(dir)
                    .Where(f => !f.EndsWith(".meta"))
                    .ToArray();
                if(files.Length > 0) {
                    return false;
                }

                var childDirs = Directory.GetDirectories(dir);
                if(childDirs.Length == 0) {
                    return true;
                }

                return childDirs.All(child => collected.Contains(child));
            }
        }

        public static List<ProjectCleanerItem> FindEmptyDirectories(ProjectCleanerSettings settings = null) {
            var items = new List<ProjectCleanerItem>();
            foreach(var dir in GetEmptyDirectories()) {
                string assetPath = ToAssetPath(dir);
                items.Add(new ProjectCleanerItem {
                    AssetPath = assetPath,
                    DisplayName = Path.GetFileName(dir),
                    Category = "Empty Directory",
                    Reason = "Directory contains no files and no non-empty child folders.",
                    Confidence = "High",
                    CanDelete = true,
                    IsDirectory = true,
                    IsKept = settings != null && settings.IsKept(assetPath)
                });
            }
            return items;
        }

        public static List<ProjectCleanerItem> FindUnusedScripts(ProjectCleanerSettings settings = null) {
            var items = new List<ProjectCleanerItem>();
            var allAssetPaths = GetScannableAssetPaths()
                .Where(path => path.EndsWith(".cs", System.StringComparison.OrdinalIgnoreCase))
                .Where(path => !path.Contains("/Tests/", System.StringComparison.OrdinalIgnoreCase))
                .ToArray();

            var referencedAssetPaths = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
            foreach(var keptPath in GetKeptAssetPaths(settings)) {
                referencedAssetPaths.Add(keptPath);
            }

            var projectAssets = GetScannableAssetPaths()
                .Where(path => !path.EndsWith(".cs", System.StringComparison.OrdinalIgnoreCase))
                .ToArray();

            foreach(var assetPath in projectAssets) {
                var deps = AssetDatabase.GetDependencies(new[] { assetPath }, true);
                foreach(var dep in deps) {
                    if(dep.EndsWith(".cs", System.StringComparison.OrdinalIgnoreCase)) {
                        referencedAssetPaths.Add(dep);
                    }
                }
            }

            // Also scan C# source files for textual references to other classes so
            // code-to-code references (e.g. Editor -> Runtime static class usage)
            // are considered.
            try {
                var scriptTexts = new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase);
                foreach(var scriptPath in allAssetPaths) {
                    try {
                        var fullPath = AssetPathToFullPath(scriptPath);
                        if(!string.IsNullOrEmpty(fullPath) && File.Exists(fullPath)) {
                            scriptTexts[scriptPath] = File.ReadAllText(fullPath);
                        } else {
                            scriptTexts[scriptPath] = string.Empty;
                        }
                    } catch {
                        scriptTexts[scriptPath] = string.Empty;
                    }
                }

                foreach(var scriptPath in allAssetPaths) {
                    var className = Path.GetFileNameWithoutExtension(scriptPath);
                    if(string.IsNullOrEmpty(className)) continue;

                    foreach(var kv in scriptTexts) {
                        if(string.Equals(kv.Key, scriptPath, System.StringComparison.OrdinalIgnoreCase)) continue;
                        var text = kv.Value;
                        if(string.IsNullOrEmpty(text)) continue;

                        if(text.Contains(className)) {
                            referencedAssetPaths.Add(scriptPath);
                            break;
                        }
                    }
                }
            } catch {
                // best-effort; ignore failures
            }

            foreach(var scriptPath in allAssetPaths) {
                if(referencedAssetPaths.Contains(scriptPath) || IsKeptPath(scriptPath, settings)) {
                    continue;
                }

                if(IsMonoBehaviourScript(scriptPath)) {
                    continue;
                }

                // Treat certain editor-entry scripts as referenced roots so they are not
                // reported as unused: EditorWindow subclasses, SettingsProvider subclasses,
                // classes marked with InitializeOnLoad, or methods marked with InitializeOnLoadMethod.
                if(IsEditorRootScript(scriptPath)) {
                    continue;
                }

                var item = new ProjectCleanerItem {
                    AssetPath = scriptPath,
                    DisplayName = Path.GetFileNameWithoutExtension(scriptPath),
                    Category = "Unused C# Script",
                    Reason = scriptPath.Contains("/Resources/", System.StringComparison.OrdinalIgnoreCase)
                        ? "The asset is inside a Resources folder, so it may be loaded dynamically."
                        : "No static references were found in the project dependency graph.",
                    Confidence = scriptPath.Contains("/Resources/", System.StringComparison.OrdinalIgnoreCase) ? "Medium" : "High",
                    CanDelete = !scriptPath.Contains("/Resources/", System.StringComparison.OrdinalIgnoreCase),
                    IsKept = settings != null && settings.IsKept(scriptPath)
                };

                items.Add(item);
            }

            return items;
        }

        public static List<ProjectCleanerItem> FindUnreferencedMonoBehaviours(ProjectCleanerSettings settings = null) {
            var items = new List<ProjectCleanerItem>();
            var allAssetPaths = GetScannableAssetPaths()
                .Where(path => path.EndsWith(".cs", System.StringComparison.OrdinalIgnoreCase))
                .Where(path => !path.Contains("/Tests/", System.StringComparison.OrdinalIgnoreCase))
                .ToArray();

            var referencedAssetPaths = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
            foreach(var keptPath in GetKeptAssetPaths(settings)) {
                referencedAssetPaths.Add(keptPath);
            }

            var projectAssets = GetScannableAssetPaths()
                .Where(path => !path.EndsWith(".cs", System.StringComparison.OrdinalIgnoreCase))
                .ToArray();

            foreach(var assetPath in projectAssets) {
                var deps = AssetDatabase.GetDependencies(new[] { assetPath }, true);
                foreach(var dep in deps) {
                    if(dep.EndsWith(".cs", System.StringComparison.OrdinalIgnoreCase)) {
                        referencedAssetPaths.Add(dep);
                    }
                }
            }

            // Also consider code-to-code references for MonoBehaviours (AddComponent<T>, typeof(T), etc.)
            try {
                var scriptTexts = new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase);
                foreach(var scriptPath in allAssetPaths) {
                    try {
                        var fullPath = AssetPathToFullPath(scriptPath);
                        if(!string.IsNullOrEmpty(fullPath) && File.Exists(fullPath)) {
                            scriptTexts[scriptPath] = File.ReadAllText(fullPath);
                        } else {
                            scriptTexts[scriptPath] = string.Empty;
                        }
                    } catch {
                        scriptTexts[scriptPath] = string.Empty;
                    }
                }

                foreach(var scriptPath in allAssetPaths) {
                    var className = Path.GetFileNameWithoutExtension(scriptPath);
                    if(string.IsNullOrEmpty(className)) continue;

                    foreach(var kv in scriptTexts) {
                        if(string.Equals(kv.Key, scriptPath, System.StringComparison.OrdinalIgnoreCase)) continue;
                        var text = kv.Value;
                        if(string.IsNullOrEmpty(text)) continue;

                        if(text.Contains(className)) {
                            referencedAssetPaths.Add(scriptPath);
                            break;
                        }
                    }
                }
            } catch {
                // ignore failures
            }

            foreach(var scriptPath in allAssetPaths) {
                if(!IsMonoBehaviourScript(scriptPath)) {
                    continue;
                }

                if(referencedAssetPaths.Contains(scriptPath) || IsKeptPath(scriptPath, settings)) {
                    continue;
                }

                bool isResourceAsset = scriptPath.Contains("/Resources/", System.StringComparison.OrdinalIgnoreCase);
                bool isEditorOnly = scriptPath.Contains("/Editor/", System.StringComparison.OrdinalIgnoreCase) || scriptPath.Contains("/Tests/", System.StringComparison.OrdinalIgnoreCase);

                var item = new ProjectCleanerItem {
                    AssetPath = scriptPath,
                    DisplayName = Path.GetFileNameWithoutExtension(scriptPath),
                    Category = "Unreferenced MonoBehaviour",
                    Reason = isResourceAsset
                        ? "The script is inside a Resources folder, so it may be loaded dynamically."
                        : isEditorOnly
                            ? "The script is editor/test-only, so it should be reviewed before deletion."
                            : "No static references were found in the project dependency graph.",
                    Confidence = isResourceAsset || isEditorOnly ? "Medium" : "High",
                    CanDelete = !isResourceAsset && !isEditorOnly,
                    IsKept = settings != null && settings.IsKept(scriptPath)
                };

                items.Add(item);
            }

            return items;
        }

        static bool IsMonoBehaviourScript(string scriptPath) {
            var monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(scriptPath);
            if(monoScript == null) {
                return false;
            }

            var classType = monoScript.GetClass();
            return classType != null && typeof(MonoBehaviour).IsAssignableFrom(classType);
        }

        static bool IsEditorRootScript(string scriptPath) {
            var monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(scriptPath);
            if(monoScript == null) {
                return false;
            }

            var classType = monoScript.GetClass();
            if(classType == null) {
                return false;
            }

            // EditorWindow subclasses are entry points (windows registered via code).
            if(typeof(EditorWindow).IsAssignableFrom(classType)) {
                return true;
            }

            // SettingsProvider subclasses are registered via the Settings window.
            var settingsProviderType = typeof(UnityEditor.SettingsProvider);
            if(settingsProviderType != null && settingsProviderType.IsAssignableFrom(classType)) {
                return true;
            }

            // Classes marked with InitializeOnLoad will have static constructors invoked.
            var initOnLoadAttr = typeof(UnityEditor.InitializeOnLoadAttribute);
            if(classType.IsDefined(initOnLoadAttr, true)) {
                return true;
            }

            // Methods marked with InitializeOnLoadMethod will be invoked on load.
            var initMethodAttr = typeof(UnityEditor.InitializeOnLoadMethodAttribute);
            var methods = classType.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            foreach(var m in methods) {
                if(m.IsDefined(initMethodAttr, true)) {
                    return true;
                }
            }

            return false;
        }

        static IEnumerable<string> GetScannableAssetPaths() {
            return AssetDatabase.GetAllAssetPaths()
                .Where(IsScannableAssetPath)
                .ToArray();
        }

        static HashSet<string> GetScannableDirectoryRoots() {
            var roots = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
            var assetsRoot = Path.GetFullPath(Application.dataPath);
            if(Directory.Exists(assetsRoot)) {
                roots.Add(assetsRoot);
            }

            foreach(var assetPath in GetScannableAssetPaths()) {
                if(IsAssetUnderAssets(assetPath)) {
                    continue;
                }

                var packageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssetPath(assetPath);
                if(packageInfo == null || string.IsNullOrEmpty(packageInfo.resolvedPath)) {
                    continue;
                }

                var resolvedPath = Path.GetFullPath(packageInfo.resolvedPath);
                if(Directory.Exists(resolvedPath)) {
                    roots.Add(resolvedPath);
                }
            }

            return roots;
        }

        static bool IsScannableAssetPath(string assetPath) {
            return IsAssetUnderAssets(assetPath) || IsAssetInEditablePackage(assetPath);
        }

        static IEnumerable<string> GetKeptAssetPaths(ProjectCleanerSettings settings) {
            if(settings == null) {
                return Enumerable.Empty<string>();
            }

            return AssetDatabase.GetAllAssetPaths()
                .Where(path => !string.IsNullOrEmpty(path))
                .Where(path => settings.IsKept(path))
                .Select(NormalizeAssetPath)
                .Where(path => !string.IsNullOrEmpty(path));
        }

        static bool IsKeptPath(string assetPath, ProjectCleanerSettings settings) {
            if(settings == null || string.IsNullOrEmpty(assetPath)) {
                return false;
            }

            return settings.IsKept(NormalizeAssetPath(assetPath));
        }

        static string NormalizeAssetPath(string assetPath) {
            if(string.IsNullOrEmpty(assetPath)) {
                return string.Empty;
            }

            return assetPath.Replace('\\', '/');
        }

        static bool IsAssetUnderAssets(string assetPath) {
            return assetPath.StartsWith("Assets/", System.StringComparison.OrdinalIgnoreCase) || assetPath.StartsWith("assets/", System.StringComparison.OrdinalIgnoreCase);
        }

        static bool IsAssetInEditablePackage(string assetPath) {
            if(string.IsNullOrEmpty(assetPath)) {
                return false;
            }

            if(!assetPath.StartsWith("Packages/", System.StringComparison.OrdinalIgnoreCase)) {
                return false;
            }

            var packageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssetPath(assetPath);
            if(packageInfo == null) {
                return false;
            }

            return packageInfo.source == PackageSource.Local || packageInfo.source == PackageSource.Embedded;
        }

        static string ToAssetPath(string absolutePath) {
            if(string.IsNullOrEmpty(absolutePath)) {
                return string.Empty;
            }

            if(absolutePath.StartsWith("Assets/", System.StringComparison.OrdinalIgnoreCase) ||
               absolutePath.StartsWith("Packages/", System.StringComparison.OrdinalIgnoreCase)) {
                return absolutePath.Replace('\\', '/');
            }

            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            string fullRoot = Path.GetFullPath(projectRoot);
            string fullPath = Path.GetFullPath(absolutePath);
            if(fullPath.StartsWith(fullRoot, System.StringComparison.OrdinalIgnoreCase)) {
                return fullPath.Substring(fullRoot.Length)
                    .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                    .Replace('\\', '/');
            }

            var packageRoot = FindPackageRoot(fullPath);
            if(!string.IsNullOrEmpty(packageRoot)) {
                var packageName = Path.GetFileName(packageRoot);
                if(!string.IsNullOrEmpty(packageName)) {
                    var relativePath = fullPath.Substring(packageRoot.Length)
                        .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                        .Replace('\\', '/');
                    return string.IsNullOrEmpty(relativePath)
                        ? "Packages/" + packageName
                        : "Packages/" + packageName + "/" + relativePath;
                }
            }

            return absolutePath.Replace('\\', '/');
        }

        static string AssetPathToFullPath(string assetPath) {
            if(string.IsNullOrEmpty(assetPath)) return string.Empty;

            if(assetPath.StartsWith("Assets/", System.StringComparison.OrdinalIgnoreCase)) {
                var relative = assetPath.Substring("Assets/".Length);
                return Path.GetFullPath(Path.Combine(Application.dataPath, relative));
            }

            if(assetPath.StartsWith("Packages/", System.StringComparison.OrdinalIgnoreCase)) {
                var segments = assetPath.Split('/');
                if(segments.Length > 1) {
                    var packageRoot = FindPackageRoot(Path.Combine(Application.dataPath, ".."));
                    // Try to find package via PackageInfo
                    var packageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssetPath(assetPath);
                    if(packageInfo != null && !string.IsNullOrEmpty(packageInfo.resolvedPath)) {
                        var packageName = segments[1];
                        var rel = assetPath.Substring(("Packages/" + packageName + "/").Length);
                        return Path.GetFullPath(Path.Combine(packageInfo.resolvedPath, rel));
                    }
                }
            }

            return string.Empty;
        }

        static string FindPackageRoot(string absolutePath) {
            var current = new DirectoryInfo(absolutePath);
            while(current != null) {
                if(File.Exists(Path.Combine(current.FullName, "package.json"))) {
                    return current.FullName;
                }

                current = current.Parent;
            }

            return null;
        }
    }

    public class ProjectCleanerItem {
        public string AssetPath;
        public string DisplayName;
        public string Category;
        public string Reason;
        public string Confidence;
        public bool CanDelete;
        public bool IsDirectory;
        public bool IsKept;
    }
}