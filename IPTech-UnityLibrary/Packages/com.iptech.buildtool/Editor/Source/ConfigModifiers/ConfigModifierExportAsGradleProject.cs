using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace IPTech.BuildTool {
    public class ConfigModifierExportAsGradleProject : ConfigModifier {
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