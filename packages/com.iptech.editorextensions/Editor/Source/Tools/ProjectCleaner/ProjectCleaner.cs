using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace IPTech.EditorTools {
    public enum CleanerStatus { Referenced, Candidate, ReviewRequired, Protected }
    public enum CleanerEvidence { UnityDependency, AssetReference, Terrain, SourceIdentifier, ProjectSettings }

    public sealed class CleanerReference {
        public string Source;
        public string Target;
        public CleanerEvidence Evidence;
    }

    public class ProjectCleanerItem {
        public string Guid;
        public string AssetPath;
        public string DisplayName;
        public string Category;
        public string Reason;
        public string Confidence;
        public bool CanDelete;
        public bool IsDirectory;
        public bool IsKept;
        public bool IsScript;
        public bool IsRoot;
        public string ProtectionReason;
        public long Bytes;
        public CleanerStatus Status;
        public readonly List<CleanerReference> ReferencedBy = new List<CleanerReference>();
    }

    public sealed class ProjectCleanerScan {
        public readonly List<ProjectCleanerItem> Items = new List<ProjectCleanerItem>();
        public readonly List<CleanerReference> References = new List<CleanerReference>();
        public readonly List<string> Errors = new List<string>();
        public readonly List<string> Coverage = new List<string>();
        public bool Complete;
        public bool Cancelled;
        public bool IsScoped;
        public string Stage;
        public float Progress;
        public int DependencyCalls;
        public bool CanClean => Complete && !Cancelled && !IsScoped && Errors.Count == 0;
    }

    public static class ProjectCleaner {
        public static ProjectCleanerScan Scan(ProjectCleanerSettings settings = null, bool showProgress = false) {
            using(var operation = new ProjectCleanerScanner(settings ?? ProjectCleanerSettings.instance)) {
                try {
                    var nextProgress = 0d;
                    while(operation.Step()) {
                        if(!showProgress || EditorApplication.timeSinceStartup < nextProgress) continue;
                        nextProgress = EditorApplication.timeSinceStartup + 0.1;
                        if(EditorUtility.DisplayCancelableProgressBar("Project Cleaner - revalidating", operation.Result.Stage, operation.Result.Progress)) {
                            operation.Cancel();
                            break;
                        }
                    }
                    return operation.Result;
                } finally {
                    if(showProgress) EditorUtility.ClearProgressBar();
                }
            }
        }

        public static List<ProjectCleanerItem> ScanProject() {
            var scan = Scan();
            if(!scan.CanClean) {
                UnityEngine.Debug.LogError("Project Cleaner scan is incomplete: " + string.Join("\n", scan.Errors));
            }
            return scan.Items.Where(i => i.Status != CleanerStatus.Referenced).ToList();
        }

        public static List<ProjectCleanerItem> FindEmptyDirectories(ProjectCleanerSettings settings = null) {
            return Scan(settings).Items.Where(i => i.IsDirectory && i.Category == "Empty Directory").ToList();
        }

        public static List<ProjectCleanerItem> FindUnusedScripts(ProjectCleanerSettings settings = null) {
            return Scan(settings).Items.Where(i => i.Category == "C# Script" && i.Status != CleanerStatus.Referenced).ToList();
        }

        public static List<ProjectCleanerItem> FindUnreferencedMonoBehaviours(ProjectCleanerSettings settings = null) {
            return Scan(settings).Items.Where(i => i.Category == "MonoBehaviour" && i.Status != CleanerStatus.Referenced).ToList();
        }

        public static List<string> GetEmptyDirectories() {
            return FindEmptyDirectories().Where(i => i.CanDelete).Select(i => i.AssetPath).ToList();
        }

        public static List<string> DeleteEmptyDirectories() {
            var items = FindEmptyDirectories();
            return ProjectCleanerCleanup.Delete(items, false, true, ProjectCleanerCleanup.DefaultBackupDirectory)
                .Where(o => o.Success).Select(o => o.AssetPath).ToList();
        }

        public static int DeleteItems(List<ProjectCleanerItem> items) {
            return ProjectCleanerCleanup.Delete(items, false, true, ProjectCleanerCleanup.DefaultBackupDirectory)
                .Count(o => o.Success);
        }

        public static void Reclassify(ProjectCleanerScan scan, ProjectCleanerSettings settings = null) {
            ProjectCleanerScanner.Classify(scan, settings ?? ProjectCleanerSettings.instance);
        }
    }
}
