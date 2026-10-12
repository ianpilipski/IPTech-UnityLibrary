using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace IPTech.EditorTools {
    public interface IProjectCleanerSettings {
        bool IncludeEditablePackages { get; set; }
        IEnumerable<string> KeptPaths { get; }
        IReadOnlyList<string> ProtectedFolders { get; }
        IEnumerable<string> UnresolvedKeptGuids { get; }
        bool IsKept(string assetPath);
        bool IsProtectedFolder(string path);
        void SetKeep(string assetPath, bool keep);
        void SetProtectedFolders(IEnumerable<string> paths);
        void RemoveMissingKeep(string guid);
        void MigrateKeepPaths();
    }

    [FilePath("ProjectSettings/IPTech/ProjectCleanerSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    public class ProjectCleanerSettings : ScriptableSingleton<ProjectCleanerSettings>, IProjectCleanerSettings {
        [SerializeField] private List<string> _keptPaths = new List<string>();
        [SerializeField] private List<string> _keptGuids = new List<string>();
        [SerializeField] private List<string> _protectedFolders = new List<string>();
        [SerializeField] private bool _includeEditablePackages;

        public bool IncludeEditablePackages {
            get => _includeEditablePackages;
            set { _includeEditablePackages = value; Save(true); }
        }

        public IEnumerable<string> KeptPaths => _keptPaths.Concat(_keptGuids.Select(AssetDatabase.GUIDToAssetPath))
            .Where(p => !string.IsNullOrEmpty(p)).Distinct(StringComparer.Ordinal);
        public IReadOnlyList<string> ProtectedFolders => _protectedFolders;
        public IEnumerable<string> UnresolvedKeptGuids => _keptGuids.Where(g => string.IsNullOrEmpty(AssetDatabase.GUIDToAssetPath(g)));

        public void RemoveMissingKeep(string guid) {
            if(!UnresolvedKeptGuids.Contains(guid)) throw new ArgumentException("Only unresolved Keep GUIDs can be removed here.");
            _keptGuids.Remove(guid);
            Save(true);
        }

        public bool IsKept(string assetPath) {
            if(string.IsNullOrEmpty(assetPath)) return false;
            var path = ProjectCleanerPaths.Normalize(assetPath);
            return KeptPaths.Any(p => ProjectCleanerPaths.Contains(p, path));
        }

        public bool IsProtectedFolder(string path) {
            return _protectedFolders.Any(p => ProjectCleanerPaths.Contains(p, path));
        }

        public void SetKeep(string assetPath, bool keep) {
            if(string.IsNullOrEmpty(assetPath)) throw new ArgumentException("Keep requires an asset path.", nameof(assetPath));
            var path = ProjectCleanerPaths.Normalize(assetPath);
            var guid = AssetDatabase.AssetPathToGUID(path);
            if(keep) {
                if(!string.IsNullOrEmpty(guid)) {
                    if(!_keptGuids.Contains(guid)) _keptGuids.Add(guid);
                    _keptPaths.Remove(path);
                } else if(!_keptPaths.Contains(path)) {
                    _keptPaths.Add(path);
                }
            } else {
                _keptPaths.Remove(path);
                _keptGuids.Remove(guid);
            }
            Save(true);
        }

        public void SetProtectedFolders(IEnumerable<string> paths) {
            var normalized = paths.Select(ProjectCleanerPaths.Normalize).Where(p => !string.IsNullOrWhiteSpace(p)).Distinct().ToList();
            if(normalized.Any(p => !ProjectCleanerPaths.IsLogicalPath(p))) {
                throw new ArgumentException("Protected folders must be Assets or Packages asset paths.");
            }
            _protectedFolders = normalized;
            Save(true);
        }

        public void MigrateKeepPaths() {
            var changed = false;
            foreach(var path in _keptPaths.ToArray()) {
                var guid = AssetDatabase.AssetPathToGUID(path);
                if(string.IsNullOrEmpty(guid)) continue;
                if(!_keptGuids.Contains(guid)) _keptGuids.Add(guid);
                _keptPaths.Remove(path);
                changed = true;
            }
            if(changed) Save(true);
        }
    }
}
