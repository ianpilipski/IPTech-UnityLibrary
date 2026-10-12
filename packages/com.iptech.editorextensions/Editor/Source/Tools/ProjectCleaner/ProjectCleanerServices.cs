using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using Debug = UnityEngine.Debug;

namespace IPTech.EditorTools {
    /// <summary>
    /// Side effects used by <see cref="ProjectCleanerView"/>: dialogs, file pickers, deletion, and editor ticking.
    /// Tests substitute this to drive the view without modal dialogs or an editor window.
    /// </summary>
    internal interface IProjectCleanerServices {
        /// <summary>Null when a scan may start; otherwise the reason it cannot.</summary>
        string ScanBlockedReason { get; }
        ProjectCleanerScanner CreateScanner();
        /// <summary>Invokes <paramref name="tick"/> every editor update until the returned handle is disposed.</summary>
        IDisposable EveryUpdate(Action tick);
        bool IsFolder(string assetPath);
        List<CleanerOutcome> Delete(IEnumerable<ProjectCleanerItem> items, bool reviewedScripts, bool backup, string backupRoot);
        List<CleanerOutcome> Restore(string session, bool apply);
        void ShowMessage(string title, string message);
        bool Confirm(string title, string message, string ok, string cancel);
        string ChooseFolder(string title, string directory);
        string ChooseSaveFile(string title, string directory, string defaultName, string extension);
        void WriteFile(string path, string contents);
        void Ping(string assetPath);
        void Open(string assetPath);
        void LogException(Exception exception);
    }

    internal sealed class EditorProjectCleanerServices : IProjectCleanerServices {
        public static readonly EditorProjectCleanerServices Instance = new EditorProjectCleanerServices();

        public string ScanBlockedReason =>
            EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorApplication.isUpdating
                ? "Stop Play Mode and wait for compilation/imports before scanning." : null;

        public ProjectCleanerScanner CreateScanner() => new ProjectCleanerScanner(ProjectCleanerSettings.instance);

        public IDisposable EveryUpdate(Action tick) {
            EditorApplication.CallbackFunction callback = () => tick();
            EditorApplication.update += callback;
            return new Subscription(() => EditorApplication.update -= callback);
        }

        public bool IsFolder(string assetPath) => AssetDatabase.IsValidFolder(assetPath);

        public List<CleanerOutcome> Delete(IEnumerable<ProjectCleanerItem> items, bool reviewedScripts, bool backup, string backupRoot) =>
            ProjectCleanerCleanup.Delete(items, reviewedScripts, backup, backupRoot);

        public List<CleanerOutcome> Restore(string session, bool apply) => ProjectCleanerCleanup.Restore(session, apply);
        public void ShowMessage(string title, string message) => EditorUtility.DisplayDialog(title, message, "OK");
        public bool Confirm(string title, string message, string ok, string cancel) => EditorUtility.DisplayDialog(title, message, ok, cancel);
        public string ChooseFolder(string title, string directory) => EditorUtility.OpenFolderPanel(title, directory, "");

        public string ChooseSaveFile(string title, string directory, string defaultName, string extension) =>
            EditorUtility.SaveFilePanel(title, directory, defaultName, extension);

        public void WriteFile(string path, string contents) => File.WriteAllText(path, contents, Encoding.UTF8);
        public void Ping(string assetPath) => EditorGUIUtility.PingObject(AssetDatabase.LoadMainAssetAtPath(assetPath));
        public void Open(string assetPath) => AssetDatabase.OpenAsset(AssetDatabase.LoadMainAssetAtPath(assetPath));
        public void LogException(Exception exception) => Debug.LogException(exception);

        private sealed class Subscription : IDisposable {
            private Action _dispose;
            public Subscription(Action dispose) => _dispose = dispose;
            public void Dispose() {
                _dispose?.Invoke();
                _dispose = null;
            }
        }
    }
}
