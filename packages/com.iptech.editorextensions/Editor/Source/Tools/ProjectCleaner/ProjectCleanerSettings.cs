using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace IPTech.EditorTools {
    [FilePath("ProjectSettings/IPTech/ProjectCleanerSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    public class ProjectCleanerSettings : ScriptableSingleton<ProjectCleanerSettings> {
        [SerializeField] private List<string> _keptPaths = new List<string>();

        public bool IsKept(string assetPath) {
            if(string.IsNullOrEmpty(assetPath)) {
                return false;
            }

            return _keptPaths.Contains(assetPath.Replace('\\', '/'));
        }

        public void SetKeep(string assetPath, bool keep) {
            if(string.IsNullOrEmpty(assetPath)) {
                return;
            }

            string normalizedPath = assetPath.Replace('\\', '/');
            if(keep) {
                if(!_keptPaths.Contains(normalizedPath)) {
                    _keptPaths.Add(normalizedPath);
                }
            } else {
                _keptPaths.Remove(normalizedPath);
            }

            Save(true);
        }

    }
}
