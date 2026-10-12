using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;

namespace IPTech.EditorTools {
    public sealed class CleanerOutcome {
        public string AssetPath;
        public bool Success;
        public string Message;
    }

    [Serializable]
    public sealed class CleanerBackupEntry {
        public string AssetPath;
        public string OriginalFullPath;
        public string Guid;
        public bool IsDirectory;
        public string ContentHash;
        public string MetaHash;
    }

    [Serializable]
    public sealed class CleanerBackupManifest {
        public string ProjectRoot;
        public List<CleanerBackupEntry> Entries = new List<CleanerBackupEntry>();
    }

    public static class ProjectCleanerCleanup {
        public static string DefaultBackupDirectory => Path.Combine(ProjectCleanerPaths.ProjectRoot, "Backups", "ProjectCleaner");

        public static List<CleanerOutcome> Preview(IEnumerable<ProjectCleanerItem> selected, bool reviewedScripts, IProjectCleanerSettings settings) {
            if(settings == null) throw new ArgumentNullException(nameof(settings));
            var requested = selected.ToList();
            var blocked = EditorBlockReason();
            if(blocked != null) return requested.Select(i => Failure(i.AssetPath, blocked)).ToList();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            blocked = EditorBlockReason();
            if(blocked != null) return requested.Select(i => Failure(i.AssetPath, blocked)).ToList();
            var scan = ProjectCleaner.Scan(settings, showProgress: true);
            if(!scan.CanClean) {
                return requested.Select(i => Failure(i.AssetPath, scan.Cancelled ? "Revalidation cancelled; no assets deleted."
                    : "Revalidation failed: " + string.Join("; ", scan.Errors))).ToList();
            }
            var current = scan.Items.ToDictionary(i => i.AssetPath);
            return requested.Select(item => {
                if(!current.TryGetValue(item.AssetPath, out var fresh) || fresh.Guid != item.Guid) {
                    return Failure(item.AssetPath, "Asset moved, removed, or replaced; rescan.");
                }
                if(!fresh.CanDelete || fresh.IsKept || (fresh.IsScript && !reviewedScripts)) {
                    return Failure(item.AssetPath, fresh.IsScript && !reviewedScripts ? "Explicit script review is required." : fresh.Reason);
                }
                try {
                    ProjectCleanerPaths.RejectLinks(ProjectCleanerPaths.FullPath(item.AssetPath));
                    ProjectCleanerPaths.RejectLinks(ProjectCleanerPaths.FullPath(item.AssetPath) + ".meta");
                    if(!AssetDatabase.IsOpenForEdit(item.AssetPath, StatusQueryOptions.ForceUpdate)) {
                        return Failure(item.AssetPath, "Asset is not writable / open for edit.");
                    }
                    return new CleanerOutcome { AssetPath = item.AssetPath, Success = true, Message = "Eligible after current dependency/protection checks." };
                } catch(IOException exception) {
                    return Failure(item.AssetPath, exception.Message);
                }
            }).ToList();
        }

        public static List<CleanerOutcome> Delete(IEnumerable<ProjectCleanerItem> selected, bool reviewedScripts, bool backup, string backupRoot,
            IProjectCleanerSettings settings) {
            var items = selected.GroupBy(i => i.AssetPath).Select(g => g.First()).ToList();
            var outcomes = Preview(items, reviewedScripts, settings);
            if(outcomes.Any(o => !o.Success)) {
                return outcomes.Select(o => o.Success ? Failure(o.AssetPath, "Batch blocked because another selection failed revalidation.") : o).ToList();
            }
            if(items.Count == 0) return outcomes;
            string session = null;
            if(backup) {
                try {
                    session = Backup(items, backupRoot);
                    Debug.Log("Project Cleaner verified backup: " + session);
                } catch(Exception exception) {
                    Debug.LogException(exception);
                    return items.Select(i => Failure(i.AssetPath, "No assets deleted: backup failed. " + exception.Message)).ToList();
                }
            }
            outcomes = Preview(items, reviewedScripts, settings);
            if(outcomes.Any(o => !o.Success)) {
                return outcomes.Select(o => o.Success ? Failure(o.AssetPath, "Batch blocked by final revalidation.") : o).ToList();
            }
            var manifest = session == null ? null : JsonUtility.FromJson<CleanerBackupManifest>(File.ReadAllText(Path.Combine(session, "manifest.json")));
            outcomes.Clear();
            foreach(var item in items.OrderByDescending(i => i.AssetPath.Length)) {
                try {
                    var full = ProjectCleanerPaths.FullPath(item.AssetPath);
                    ProjectCleanerPaths.RejectLinks(full);
                    ProjectCleanerPaths.RejectLinks(full + ".meta");
                    var saved = manifest?.Entries.Single(e => e.AssetPath == item.AssetPath);
                    if(!ProjectCleanerPaths.IsWritableTarget(item.AssetPath, settings.IncludeEditablePackages)
                        || settings.IsKept(item.AssetPath)
                        || settings.IsProtectedFolder(item.AssetPath)) {
                        outcomes.Add(Failure(item.AssetPath, "Scope or Keep protection changed; deletion blocked."));
                    } else if(item.IsDirectory && !IsPhysicallyEmptyTree(full)) {
                        outcomes.Add(Failure(item.AssetPath, "Folder is no longer empty; deletion blocked."));
                    } else if(AssetDatabase.AssetPathToGUID(item.AssetPath) != item.Guid) {
                        outcomes.Add(Failure(item.AssetPath, "Asset identity changed; deletion blocked."));
                    } else if(saved != null && (Hash(full + ".meta") != saved.MetaHash || (!item.IsDirectory && Hash(full) != saved.ContentHash))) {
                        outcomes.Add(Failure(item.AssetPath, "Content changed after backup; deletion blocked."));
                    } else if(AssetDatabase.DeleteAsset(item.AssetPath)) {
                        outcomes.Add(new CleanerOutcome { AssetPath = item.AssetPath, Success = true, Message = session == null ? "Deleted without backup." : "Deleted. Verified backup: " + session });
                    } else {
                        outcomes.Add(Failure(item.AssetPath, "Unity refused deletion."));
                    }
                } catch(Exception exception) {
                    Debug.LogException(exception);
                    outcomes.Add(Failure(item.AssetPath, "Deletion failed: " + exception.Message));
                }
            }
            AssetDatabase.Refresh();
            return outcomes;
        }

        public static bool IsPhysicallyEmptyTree(string full) {
            if(!Directory.Exists(full)) return false;
            ProjectCleanerPaths.RejectLinks(full);
            foreach(var file in Directory.GetFiles(full)) {
                if(!file.EndsWith(".meta", StringComparison.Ordinal) || !Directory.Exists(file.Substring(0, file.Length - 5))) return false;
            }
            return Directory.GetDirectories(full).All(IsPhysicallyEmptyTree);
        }

        public static string Backup(IEnumerable<ProjectCleanerItem> selected, string backupRoot) {
            if(string.IsNullOrWhiteSpace(backupRoot)) throw new ArgumentException("A backup directory is required.");
            backupRoot = Path.GetFullPath(backupRoot);
            ProjectCleanerPaths.RejectLinks(backupRoot);
            var scanRoots = new List<string> { Application.dataPath };
            scanRoots.AddRange(UnityEditor.PackageManager.PackageInfo.GetAllRegisteredPackages().Select(p => p.resolvedPath));
            if(scanRoots.Any(root => ProjectCleanerPaths.Contains(ProjectCleanerPaths.Normalize(Path.GetFullPath(root)), ProjectCleanerPaths.Normalize(backupRoot)))) {
                throw new IOException("Backup must be outside Assets and all scanned package roots.");
            }
            var session = Path.Combine(backupRoot, DateTime.UtcNow.ToString("yyyyMMdd-HHmmss") + "-" + System.Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(session);
            var manifest = new CleanerBackupManifest { ProjectRoot = ProjectCleanerPaths.ProjectRoot };
            var entries = new Dictionary<string, ProjectCleanerItem>();
            foreach(var item in selected) {
                entries[item.AssetPath] = item;
                if(!item.IsDirectory) continue;
                var full = ProjectCleanerPaths.FullPath(item.AssetPath);
                if(!IsPhysicallyEmptyTree(full)) throw new IOException("Cannot back up a non-empty cleanup folder: " + item.AssetPath);
                foreach(var child in Directory.GetDirectories(full, "*", SearchOption.AllDirectories)) {
                    var path = item.AssetPath + "/" + ProjectCleanerPaths.Normalize(Path.GetRelativePath(full, child));
                    entries[path] = new ProjectCleanerItem { AssetPath = path, Guid = AssetDatabase.AssetPathToGUID(path), IsDirectory = true };
                }
            }
            foreach(var item in entries.Values) {
                var full = ProjectCleanerPaths.FullPath(item.AssetPath);
                ProjectCleanerPaths.RejectLinks(full);
                ProjectCleanerPaths.RejectLinks(full + ".meta");
                var destination = Path.Combine(session, item.AssetPath);
                Directory.CreateDirectory(Path.GetDirectoryName(destination));
                var entry = new CleanerBackupEntry {
                    AssetPath = item.AssetPath, OriginalFullPath = full, Guid = item.Guid, IsDirectory = item.IsDirectory
                };
                if(!item.IsDirectory) {
                    entry.ContentHash = Hash(full);
                    File.Copy(full, destination, false);
                    if(Hash(destination) != entry.ContentHash || Hash(full) != entry.ContentHash) throw new IOException("Asset backup verification failed: " + item.AssetPath);
                }
                entry.MetaHash = Hash(full + ".meta");
                File.Copy(full + ".meta", destination + ".meta", false);
                if(Hash(destination + ".meta") != entry.MetaHash || Hash(full + ".meta") != entry.MetaHash) throw new IOException("Metadata backup verification failed: " + item.AssetPath);
                manifest.Entries.Add(entry);
            }
            var manifestPath = Path.Combine(session, "manifest.json");
            File.WriteAllText(manifestPath, JsonUtility.ToJson(manifest, true));
            if(JsonUtility.FromJson<CleanerBackupManifest>(File.ReadAllText(manifestPath)).Entries.Count != manifest.Entries.Count) {
                throw new IOException("Backup manifest verification failed.");
            }
            return session;
        }

        public static List<CleanerOutcome> Restore(string session, bool apply, IProjectCleanerSettings settings) {
            if(settings == null) throw new ArgumentNullException(nameof(settings));
            var blocked = EditorBlockReason(false);
            if(blocked != null) throw new InvalidOperationException(blocked);
            session = Path.GetFullPath(session);
            ProjectCleanerPaths.RejectLinks(session);
            var manifest = JsonUtility.FromJson<CleanerBackupManifest>(File.ReadAllText(Path.Combine(session, "manifest.json")));
            if(manifest == null || manifest.ProjectRoot != ProjectCleanerPaths.ProjectRoot || manifest.Entries == null) {
                throw new IOException("Backup belongs to another project or has an invalid manifest.");
            }
            var outcomes = new List<CleanerOutcome>();
            foreach(var entry in manifest.Entries) {
                if(!ProjectCleanerPaths.IsWritableTarget(entry.AssetPath, settings.IncludeEditablePackages)) {
                    outcomes.Add(Failure(entry.AssetPath, "Restore target is out of scope. Enable editable packages if appropriate."));
                    continue;
                }
                var full = ProjectCleanerPaths.FullPath(entry.AssetPath);
                ProjectCleanerPaths.RejectLinks(full);
                ProjectCleanerPaths.RejectLinks(full + ".meta");
                var source = Path.Combine(session, entry.AssetPath);
                ProjectCleanerPaths.RejectLinks(source);
                ProjectCleanerPaths.RejectLinks(source + ".meta");
                if(full != entry.OriginalFullPath || File.Exists(full) || File.Exists(full + ".meta") || Directory.Exists(full)) {
                    outcomes.Add(Failure(entry.AssetPath, "Restore collision or package location change. No existing content will be overwritten."));
                } else if(Hash(source + ".meta") != entry.MetaHash || (!entry.IsDirectory && Hash(source) != entry.ContentHash)) {
                    outcomes.Add(Failure(entry.AssetPath, "Backup integrity verification failed."));
                } else {
                    outcomes.Add(new CleanerOutcome { AssetPath = entry.AssetPath, Success = true, Message = "Ready to restore." });
                }
            }
            if(!apply) return outcomes;
            if(outcomes.Any(o => !o.Success)) {
                return outcomes.Select(o => o.Success ? Failure(o.AssetPath, "Restore batch blocked; no assets restored.") : o).ToList();
            }
            foreach(var entry in manifest.Entries.OrderBy(e => e.AssetPath.Length)) {
                var full = ProjectCleanerPaths.FullPath(entry.AssetPath);
                var source = Path.Combine(session, entry.AssetPath);
                Directory.CreateDirectory(Path.GetDirectoryName(full));
                if(entry.IsDirectory) Directory.CreateDirectory(full);
                else File.Copy(source, full, false);
                File.Copy(source + ".meta", full + ".meta", false);
                if(Hash(full + ".meta") != entry.MetaHash || (!entry.IsDirectory && Hash(full) != entry.ContentHash)) {
                    throw new IOException("Restored content did not verify: " + entry.AssetPath);
                }
            }
            AssetDatabase.Refresh();
            foreach(var entry in manifest.Entries) {
                if(AssetDatabase.AssetPathToGUID(entry.AssetPath) != entry.Guid) throw new IOException("Restored GUID mismatch: " + entry.AssetPath);
            }
            foreach(var outcome in outcomes) outcome.Message = "Restored and verified.";
            return outcomes;
        }

        private static string Hash(string path) {
            using(var algorithm = SHA256.Create())
            using(var stream = File.OpenRead(path)) {
                return BitConverter.ToString(algorithm.ComputeHash(stream)).Replace("-", "");
            }
        }

        private static string EditorBlockReason(bool deleting = true) {
            if(EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) {
                return "Stop Play Mode and wait for compilation/imports before cleanup or restore.";
            }
            if(EditorUtility.scriptCompilationFailed) return "Fix script compilation errors before cleanup or restore.";
            if(!deleting) return null;
            for(var index = 0; index < UnityEngine.SceneManagement.SceneManager.sceneCount; index++) {
                if(UnityEngine.SceneManagement.SceneManager.GetSceneAt(index).isDirty) {
                    return "Save or discard unsaved scene changes before cleanup or restore. Unsaved references are not part of the disk dependency graph.";
                }
            }
            var dirty = Resources.FindObjectsOfTypeAll<UnityEngine.Object>().Where(obj => AssetDatabase.Contains(obj)
                && AssetDatabase.IsNativeAsset(obj) && ProjectCleanerPaths.IsLogicalPath(AssetDatabase.GetAssetPath(obj)) && EditorUtility.IsDirty(obj))
                .Select(AssetDatabase.GetAssetPath).Distinct().ToArray();
            if(dirty.Length > 0) {
                return "Save or discard unsaved asset changes before cleanup: " + string.Join(", ", dirty);
            }
            return null;
        }

        private static CleanerOutcome Failure(string path, string message) {
            Debug.LogWarning("Project Cleaner: " + path + ": " + message);
            return new CleanerOutcome { AssetPath = path, Message = message };
        }
    }
}
