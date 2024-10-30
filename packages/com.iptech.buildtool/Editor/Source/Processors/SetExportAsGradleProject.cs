using UnityEditor;

namespace IPTech.BuildTool.Processors {
    public class SetExportAsGradleProject : BuildProcessor {
        public bool ExportAsGradleProject;

        bool origValue;

        public override void ModifyProject(BuildTarget buildTarget) {
            origValue = EditorUserBuildSettings.exportAsGoogleAndroidProject;
            EditorUserBuildSettings.exportAsGoogleAndroidProject = ExportAsGradleProject;
        }

        public override void RestoreProject(BuildTarget buildTarget) {
            EditorUserBuildSettings.exportAsGoogleAndroidProject = origValue;
        }
    }
}
