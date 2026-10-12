using System;
using System.IO;
using UnityEditor.PackageManager;
using UnityEngine;

namespace IPTech.EditorTools {
    public static class ProjectCleanerPaths {
        public static string ProjectRoot => Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        public static string Normalize(string path) => (path ?? "").Replace('\\', '/').TrimEnd('/');

        public static bool Contains(string root, string path) {
            root = Normalize(root);
            path = Normalize(path);
            return !string.IsNullOrEmpty(root) && (path == root || path.StartsWith(root + "/", StringComparison.Ordinal));
        }

        public static bool IsLogicalPath(string path) {
            return !string.IsNullOrEmpty(path) && (Contains("Assets", path) || Contains("Packages", path))
                && !path.Split('/').AnySegmentIsTraversal();
        }

        public static string FullPath(string assetPath) {
            if(!IsLogicalPath(assetPath)) throw new ArgumentException("Invalid asset path: " + assetPath);
            if(Contains("Assets", assetPath)) return Path.GetFullPath(Path.Combine(ProjectRoot, assetPath));
            var package = PackageInfo.FindForAssetPath(assetPath);
            if(package == null) throw new InvalidOperationException("Cannot resolve package: " + assetPath);
            var root = "Packages/" + package.name;
            if(!Contains(root, assetPath)) throw new InvalidOperationException("Package identity mismatch: " + assetPath);
            var relative = assetPath.Length == root.Length ? "" : assetPath.Substring(root.Length + 1);
            return Path.GetFullPath(Path.Combine(package.resolvedPath, relative));
        }

        public static bool IsWritableTarget(string assetPath, bool includePackages) {
            if(!IsLogicalPath(assetPath) || assetPath == "Assets" || assetPath == "Packages") return false;
            if(Contains("Assets", assetPath)) return true;
            var package = PackageInfo.FindForAssetPath(assetPath);
            return includePackages && package != null && assetPath != "Packages/" + package.name
                && (package.source == PackageSource.Local || package.source == PackageSource.Embedded);
        }

        public static void RejectLinks(string fullPath) {
            var current = Path.GetFullPath(fullPath);
            while(!string.IsNullOrEmpty(current)) {
                if((File.Exists(current) || Directory.Exists(current)) && (File.GetAttributes(current) & FileAttributes.ReparsePoint) != 0) {
                    throw new IOException("Symbolic links are not supported for cleanup: " + current);
                }
                current = Path.GetDirectoryName(current);
            }
        }

        private static bool AnySegmentIsTraversal(this string[] segments) {
            foreach(var segment in segments) {
                if(segment == "." || segment == ".." || segment.Length == 0) return true;
            }
            return false;
        }
    }
}
