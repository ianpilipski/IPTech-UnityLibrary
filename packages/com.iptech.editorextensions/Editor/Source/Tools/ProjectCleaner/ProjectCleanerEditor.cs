using UnityEditor;
using UnityEngine;

namespace IPTech.EditorTools {
    /// <summary>
    /// Window shell for the Project Cleaner. State lives in <see cref="ProjectCleanerSession"/> and
    /// the UI in <see cref="ProjectCleanerView"/>, so both can be tested without opening this window.
    /// </summary>
    public class ProjectCleanerEditor : EditorWindow {
        [SerializeField] private ProjectCleanerSession _session;
        private ProjectCleanerView _view;

        [MenuItem("IPTech/Project/Cleaner ...")]
        [MenuItem("Window/IPTech/Project/Cleaner")]
        public static void MenuOpen() {
            var window = GetWindow<ProjectCleanerEditor>();
            window.titleContent = new GUIContent("Project Cleaner");
            window.minSize = new Vector2(850, 500);
            window.Show();
        }

        [MenuItem("Assets/IPTech/Find References", false, 20)]
        private static void FindReferences() {
            var path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if(string.IsNullOrEmpty(path)) {
                Debug.LogWarning("Project Cleaner: select a project asset to inspect references.");
                return;
            }
            var window = GetWindow<ProjectCleanerEditor>();
            window._session.Search = path;
            window._session.Status = "All";
            window._session.Scope = ProjectCleanerSession.AllScopes;
            window.CreateGUI();
            window._view.BeginScan();
        }

        private void OnEnable() {
            _session = new ProjectCleanerSession(ProjectCleanerSettings.instance, _session);
            EditorApplication.projectChanged += OnProjectChanged;
        }

        private void OnDisable() {
            EditorApplication.projectChanged -= OnProjectChanged;
            _view?.CancelScan();
        }

        private void OnDestroy() {
            _view?.Dispose();
            _view = null;
        }

        public void CreateGUI() {
            _view?.Dispose();
            _view = new ProjectCleanerView(rootVisualElement, _session, EditorProjectCleanerServices.Instance);
        }

        private void OnProjectChanged() => _view?.ProjectChanged();

        public static string CsvField(string value) => ProjectCleanerSession.CsvField(value);
        public static string FormatBytes(long bytes) => ProjectCleanerSession.FormatBytes(bytes);
    }
}
