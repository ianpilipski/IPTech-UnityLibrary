using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPTech.EditorTools {
    /// <summary>
    /// UI-independent Project Cleaner state: filters, sorting, selection, and deletion eligibility.
    /// Serializable so the hosting window can persist user choices across domain reloads.
    /// </summary>
    [Serializable]
    internal sealed class ProjectCleanerSession {
        public const string AllScopes = "All scopes";
        public const string Descending = " descending";
        public static readonly IReadOnlyList<string> StatusChoices =
            new[] { "Candidates", "Review required", "Protected", "Referenced", "Kept", "All" };
        public static readonly IReadOnlyList<string> SortChoices = new[] { "Pick", "Path", "Type", "Status", "Size", "References" }
            .SelectMany(value => new[] { value, value + Descending }).ToArray();
        private static readonly (string Label, string Column)[] SortColumns = {
            ("Pick", "selected"), ("Path", "path"), ("Type", "type"), ("Status", "status"), ("Size", "bytes"), ("References", "refs")
        };

        public string Search = "";
        public string Status = "Candidates";
        public string Scope = AllScopes;
        public string Sort = "Path";
        public List<string> Selected = new List<string>();
        public string BackupDirectory;
        public bool Backup = true;
        public bool ReviewedScripts;

        [NonSerialized] public ProjectCleanerScan Scan;
        [NonSerialized] public bool Stale;
        [NonSerialized] private List<ProjectCleanerItem> _visible;

        public List<ProjectCleanerItem> Visible => _visible ?? (_visible = new List<ProjectCleanerItem>());
        public string SortColumn => SortColumns.FirstOrDefault(s => s.Label == Sort.Replace(Descending, "")).Column ?? "path";
        public bool SortDescending => Sort.EndsWith(Descending, StringComparison.Ordinal);

        public void Normalize() {
            if(!StatusChoices.Contains(Status)) Status = "Candidates";
            if(!SortChoices.Contains(Sort)) Sort = "Path";
            if(string.IsNullOrEmpty(BackupDirectory)) BackupDirectory = ProjectCleanerCleanup.DefaultBackupDirectory;
            if(Selected == null) Selected = new List<string>();
        }

        public void SetSort(string column, bool descending) {
            Sort = SortColumns.FirstOrDefault(s => s.Column == column).Label ?? "Path";
            if(descending) Sort += Descending;
        }

        public void StartScan(ProjectCleanerScan scan) {
            Selected.Clear();
            ReviewedScripts = false;
            Scan = scan;
            Stale = false;
        }

        public void MarkStale() => Stale = true;

        public void RefreshVisible() {
            Visible.Clear();
            if(Scan == null) return;
            IEnumerable<ProjectCleanerItem> items = Scan.Items;
            switch(Status) {
                case "Candidates": items = items.Where(i => i.Status == CleanerStatus.Candidate); break;
                case "Review required": items = items.Where(i => i.Status == CleanerStatus.ReviewRequired); break;
                case "Protected": items = items.Where(i => i.Status == CleanerStatus.Protected); break;
                case "Referenced": items = items.Where(i => i.Status == CleanerStatus.Referenced); break;
                case "Kept": items = items.Where(i => i.IsKept); break;
            }
            if(Scope != AllScopes) items = items.Where(i => ProjectCleanerPaths.Contains(Scope, i.AssetPath));
            if(!string.IsNullOrEmpty(Search)) items = items.Where(i => i.AssetPath.IndexOf(Search, StringComparison.OrdinalIgnoreCase) >= 0
                || i.Category.IndexOf(Search, StringComparison.OrdinalIgnoreCase) >= 0);
            Visible.AddRange(items.OrderBy(i => i, Comparer<ProjectCleanerItem>.Create(Compare)));
        }

        public int Compare(ProjectCleanerItem left, ProjectCleanerItem right) {
            int comparison;
            switch(SortColumn) {
                case "selected": comparison = Selected.Contains(left.Guid).CompareTo(Selected.Contains(right.Guid)); break;
                case "type": comparison = StringComparer.OrdinalIgnoreCase.Compare(left.Category, right.Category); break;
                case "status": comparison = StringComparer.OrdinalIgnoreCase.Compare(left.Status.ToString(), right.Status.ToString()); break;
                case "bytes": comparison = left.Bytes.CompareTo(right.Bytes); break;
                case "refs": comparison = ReferrerCount(left).CompareTo(ReferrerCount(right)); break;
                default: comparison = StringComparer.OrdinalIgnoreCase.Compare(left.AssetPath, right.AssetPath); break;
            }
            if(SortDescending) comparison = -comparison;
            return comparison != 0 ? comparison : StringComparer.Ordinal.Compare(left.AssetPath, right.AssetPath);
        }

        public static int ReferrerCount(ProjectCleanerItem item) => item.ReferencedBy.Select(r => r.Source).Distinct().Count();

        public List<string> Scopes(Func<string, bool> isFolder) {
            var scopes = new List<string> { AllScopes, "Assets" };
            if(Scan != null) {
                scopes.AddRange(Scan.Items.Select(i => i.AssetPath.Split('/'))
                    .Where(parts => parts.Length >= 2)
                    .Select(parts => string.Join("/", parts.Take(2)))
                    .Where(isFolder).Distinct().OrderBy(p => p));
            }
            if(!scopes.Contains(Scope)) Scope = AllScopes;
            return scopes;
        }

        public bool CanSelect(ProjectCleanerItem item) => Scan != null && Scan.CanClean && !Stale && item.CanDelete && !item.IsKept;
        public bool IsSelected(ProjectCleanerItem item) => Selected.Contains(item.Guid);
        public List<ProjectCleanerItem> SelectedItems() =>
            Scan == null ? new List<ProjectCleanerItem>() : Scan.Items.Where(IsSelected).ToList();

        public void SetSelected(ProjectCleanerItem item, bool selected) {
            if(!selected) {
                Selected.Remove(item.Guid);
                return;
            }
            if(Selected.Contains(item.Guid)) return;
            Selected.Add(item.Guid);
            // Every newly selected script must be explicitly acknowledged again.
            if(item.IsScript) ReviewedScripts = false;
        }

        public void SelectVisible() {
            foreach(var item in Visible.Where(CanSelect)) SetSelected(item, true);
        }

        public void ClearSelection() => Selected.Clear();

        public string BlockingReason(IReadOnlyCollection<ProjectCleanerItem> items) {
            var needsScriptReview = items.Any(i => i.IsScript) && !ReviewedScripts;
            if(items.All(CanSelect) && !needsScriptReview) return null;
            if(Stale) return "Results are stale. Cancel and scan again before deleting.";
            return needsScriptReview ? "Cancel and acknowledge review of the selected scripts before deleting."
                : "Selected assets are no longer eligible. Cancel and review a fresh scan before deleting.";
        }

        public bool CanConfirm(IReadOnlyCollection<ProjectCleanerItem> items) => items.Count > 0 && BlockingReason(items) == null;

        /// <summary>Removes successfully deleted items (and their children) and returns the results message.</summary>
        public string CompleteDeletion(IEnumerable<CleanerOutcome> outcomes) {
            var results = outcomes.ToList();
            var deletedPaths = results.Where(o => o.Success).Select(o => o.AssetPath).ToArray();
            Scan?.Items.RemoveAll(item => deletedPaths.Any(path => ProjectCleanerPaths.Contains(path, item.AssetPath)));
            Selected.Clear();
            ReviewedScripts = false;
            MarkStale();
            RefreshVisible();
            return FormatOutcomes(results) + "\nScan again to refresh references before deleting more assets.";
        }

        public string Summary() {
            var selected = SelectedItems();
            return $"Visible: {Visible.Count} ({FormatBytes(Visible.Sum(i => i.Bytes))}) | Selected: {selected.Count} ({FormatBytes(selected.Sum(i => i.Bytes))})"
                + (Stale ? " | STALE - rescan required" : "");
        }

        public string VisibleCsv() {
            var csv = new StringBuilder("Path,GUID,Type,Bytes,Status,Reason,Referrers\n");
            foreach(var item in Visible) {
                csv.AppendLine(string.Join(",", new[] { item.AssetPath, item.Guid, item.Category, item.Bytes.ToString(),
                    item.Status.ToString(), item.Reason, string.Join(";", item.ReferencedBy.Select(r => r.Source).Distinct()) }.Select(CsvField)));
            }
            return csv.ToString();
        }

        public static string FormatOutcomes(IEnumerable<CleanerOutcome> outcomes) =>
            string.Join("\n", outcomes.Select(o => o.AssetPath + ": " + o.Message));

        public static string CsvField(string value) => "\"" + (value ?? "").Replace("\"", "\"\"") + "\"";

        public static string FormatBytes(long bytes) {
            var units = new[] { "B", "KB", "MB", "GB" };
            var amount = (double)bytes;
            var unit = 0;
            while(amount >= 1024 && unit < units.Length - 1) { amount /= 1024; unit++; }
            return amount.ToString("0.##") + " " + units[unit];
        }
    }
}
