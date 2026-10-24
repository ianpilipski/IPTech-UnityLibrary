using System.Collections.Generic;
using UnityEditor;

namespace IPTech.BuildTool.Processors {
    public class SetExportAsGradleProject : BuildProcessor {
        public bool ExportAsGradleProject;

        bool origValue;

        public override void ModifyProject(BuildTarget buildTarget, IReadOnlyDictionary<string,string> args) {
            origValue = EditorUserBuildSettings.exportAsGoogleAndroidProject;
            EditorUserBuildSettings.exportAsGoogleAndroidProject = ExportAsGradleProject;
        }

        public override void RestoreProject(BuildTarget buildTarget, IReadOnlyDictionary<string,string> args) {
            EditorUserBuildSettings.exportAsGoogleAndroidProject = origValue;
        }
    }
}
