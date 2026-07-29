using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

namespace IPTech.EditorTools
{
	public class ProjectCleanerEditor : EditorWindow
	{
		private List<ProjectCleanerItem> _scanResults;
		private Vector2 _leftScrollPosition;
		private Vector2 _rightScrollPosition;
		private string _selectedGroupPath;
		private readonly Dictionary<string, List<ProjectCleanerItem>> _groupedItems = new Dictionary<string, List<ProjectCleanerItem>>();
		private readonly Dictionary<string, bool> _foldoutStates = new Dictionary<string, bool>();

		[MenuItem("IPTech/Project/Cleaner ...")]
		[MenuItem("Window/IPTech/Project/Cleaner")]
		static void MenuOpen() {
			var win = EditorWindow.GetWindow<ProjectCleanerEditor>();
			win.titleContent = new GUIContent("Project Cleaner");
			win.Show();
		}

		private void OnEnable() {
			RefreshResults();
		}

		private void OnGUI() {
			using(new EditorGUILayout.VerticalScope(EditorStyles.helpBox)) {
				DrawToolbar();
				DrawResults();
			}
		}

		private void DrawToolbar() {
			using(new EditorGUILayout.HorizontalScope(EditorStyles.toolbar)) {
				if(GUILayout.Button("Refresh", EditorStyles.toolbarButton)) {
					RefreshResults();
				}
				if(GUILayout.Button("Delete Safe", EditorStyles.toolbarButton)) {
					DeleteSafeItems();
				}
				if(GUILayout.Button("Delete Empty Dirs", EditorStyles.toolbarButton)) {
					DeleteEmptyDirectories();
				}
			}
		}

		private void DrawResults() {
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Project Cleaner Results", EditorStyles.boldLabel);
			EditorGUILayout.HelpBox("This first pass reports empty folders, unused C# scripts, and MonoBehaviours that are not referenced by the project dependency graph. Items are grouped by their Assets folder and by package identity.", MessageType.Info);

			if(_scanResults == null || _scanResults.Count == 0) {
				EditorGUILayout.HelpBox("No cleanup candidates were found.", MessageType.None);
				return;
			}

			BuildGroupIndex();
			using(new EditorGUILayout.HorizontalScope()) {
				DrawTreeView();
				DrawDetailsPane();
			}
		}

		private void BuildGroupIndex() {
			_groupedItems.Clear();
			var activeItems = _scanResults.Where(i => !i.IsKept).ToList();
			foreach(var item in activeItems) {
				var groupName = GetGroupName(item.AssetPath);
				if(!_groupedItems.ContainsKey(groupName)) {
					_groupedItems[groupName] = new List<ProjectCleanerItem>();
				}
				_groupedItems[groupName].Add(item);
			}

			var keptItems = _scanResults.Where(i => i.IsKept).ToList();
			foreach(var item in keptItems) {
				var groupName = GetGroupName(item.AssetPath);
				if(!_groupedItems.ContainsKey(groupName)) {
					_groupedItems[groupName] = new List<ProjectCleanerItem>();
				}
			}

			if(string.IsNullOrEmpty(_selectedGroupPath) && _groupedItems.Count > 0) {
				_selectedGroupPath = _groupedItems.Keys.OrderBy(k => k).First();
			}
		}

		private void DrawTreeView() {
			using(new EditorGUILayout.ScrollViewScope(_leftScrollPosition, GUILayout.Width(240F), GUILayout.ExpandHeight(true))) {
				EditorGUILayout.LabelField("Groups", EditorStyles.boldLabel);
				foreach(var group in _groupedItems.Keys.OrderBy(k => k)) {
					var isSelected = string.Equals(group, _selectedGroupPath, System.StringComparison.Ordinal);
					using(new EditorGUILayout.HorizontalScope()) {
						GUI.backgroundColor = isSelected ? Color.cyan : Color.white;
						if(GUILayout.Button(group, EditorStyles.miniButton, GUILayout.Width(220F))) {
							_selectedGroupPath = group;
						}
						GUI.backgroundColor = Color.white;
					}
				}
			}
		}

		private void DrawDetailsPane() {
			using(var scroll = new EditorGUILayout.ScrollViewScope(_rightScrollPosition, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true))) {
				_rightScrollPosition = scroll.scrollPosition;
				if(string.IsNullOrEmpty(_selectedGroupPath) || !_groupedItems.ContainsKey(_selectedGroupPath)) {
					EditorGUILayout.HelpBox("Select a group from the left to inspect its items.", MessageType.Info);
					return;
				}

				EditorGUILayout.LabelField(_selectedGroupPath, EditorStyles.boldLabel);
				var activeItems = _groupedItems[_selectedGroupPath]
					.Where(i => !i.IsKept)
					.OrderBy(i => i.DisplayName)
					.ToList();
				var keptItems = _scanResults
					.Where(i => i.IsKept && GetGroupName(i.AssetPath) == _selectedGroupPath)
					.OrderBy(i => i.DisplayName)
					.ToList();

				foreach(var item in activeItems) {
					DrawItem(item);
				}

				if(keptItems.Count > 0) {
					var foldoutKey = _selectedGroupPath + "_kept";
					if(!_foldoutStates.ContainsKey(foldoutKey)) {
						_foldoutStates[foldoutKey] = false;
					}
					_foldoutStates[foldoutKey] = EditorGUILayout.Foldout(_foldoutStates[foldoutKey], "Kept Items");
					if(_foldoutStates[foldoutKey]) {
						foreach(var item in keptItems) {
							DrawItem(item);
						}
					}
				}
			}
		}

		private void DrawItem(ProjectCleanerItem item) {
			using(new EditorGUILayout.VerticalScope(EditorStyles.helpBox)) {
				using(new EditorGUILayout.HorizontalScope()) {
					EditorGUILayout.LabelField(item.Category, EditorStyles.boldLabel);
					EditorGUILayout.LabelField(item.Confidence, EditorStyles.miniLabel);
				}

				EditorGUILayout.LabelField(item.DisplayName, EditorStyles.miniBoldLabel);
				EditorGUILayout.LabelField(item.AssetPath, EditorStyles.wordWrappedMiniLabel);
				EditorGUILayout.LabelField(item.Reason, EditorStyles.wordWrappedMiniLabel);

				using(new EditorGUILayout.HorizontalScope()) {
					EditorGUILayout.LabelField(item.IsKept ? "Decision: Keep" : (item.CanDelete ? "Decision: Delete" : "Decision: Review first"), EditorStyles.miniLabel);
					if(GUILayout.Button(item.IsKept ? "Keep" : "Keep", GUILayout.Width(80F))) {
						SetKeep(item, true);
					}
					if(item.CanDelete && GUILayout.Button("Delete", GUILayout.Width(80F))) {
						DeleteSingleItem(item);
					}
				}
			}
		}

		private string GetGroupName(string assetPath) {
			if(string.IsNullOrEmpty(assetPath)) {
				return "(no path)";
			}

			var normalizedPath = assetPath.Replace('\\', '/');
			if(normalizedPath.StartsWith("Assets/", System.StringComparison.OrdinalIgnoreCase) || normalizedPath.Equals("Assets", System.StringComparison.OrdinalIgnoreCase)) {
				return "Assets";
			}

			if(normalizedPath.StartsWith("Packages/", System.StringComparison.OrdinalIgnoreCase)) {
				var segments = normalizedPath.Split('/');
				if(segments.Length > 1) {
					return segments[1];
				}
			}

			if(normalizedPath.Contains("/Packages/", System.StringComparison.OrdinalIgnoreCase)) {
				var start = normalizedPath.IndexOf("/Packages/", System.StringComparison.OrdinalIgnoreCase);
				if(start >= 0) {
					var segments = normalizedPath.Substring(start + 1).Split('/');
					if(segments.Length > 1) {
						return segments[1];
					}
				}
			}

			return "(project assets)";
		}

		private void RefreshResults() {
			_scanResults = ProjectCleaner.ScanProject();
		}

		private void DeleteSafeItems() {
			if(!EditorUtility.DisplayDialog("Delete Safe Items", "Delete the items marked as safe to delete?", "Delete", "Cancel")) {
				return;
			}

			int deletedCount = ProjectCleaner.DeleteItems(_scanResults);
			if(deletedCount > 0) {
				EditorUtility.DisplayDialog("Cleanup Complete", "Deleted " + deletedCount + " item(s).", "OK");
			}

			RefreshResults();
		}

		private void DeleteSingleItem(ProjectCleanerItem item) {
			if(!EditorUtility.DisplayDialog("Delete Item", "Delete " + item.AssetPath + "?", "Delete", "Cancel")) {
				return;
			}

			int deletedCount = ProjectCleaner.DeleteItems(new List<ProjectCleanerItem> { item });
			if(deletedCount > 0) {
				EditorUtility.DisplayDialog("Cleanup Complete", "Deleted " + item.AssetPath + ".", "OK");
			}

			RefreshResults();
		}

		private void SetKeep(ProjectCleanerItem item, bool keep) {
			var settings = ProjectCleanerSettings.instance;
			settings.SetKeep(item.AssetPath, keep);
			item.IsKept = keep;
			RefreshResults();
			Repaint();
		}

		private void DeleteEmptyDirectories() {
			var directories = ProjectCleaner.DeleteEmptyDirectories();
			if(directories.Count > 0) {
				EditorUtility.DisplayDialog("Cleanup Complete", "Deleted " + directories.Count + " empty directory(ies).", "OK");
			}
			RefreshResults();
		}
	}
}
