using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace IPTech.BuildTool {
    public class ConfigModifierApplicationIdentifier : ConfigModifier {
        public string ApplicationIdentifier;

        string origValue;

        void OnEnable() {
            if(string.IsNullOrEmpty(ApplicationIdentifier)) {
                ApplicationIdentifier = PlayerSettings.GetApplicationIdentifier(EditorUserBuildSettings.selectedBuildTargetGroup);
            }
        }

        public override void ModifyProject(BuildTarget buildTarget) {
            origValue = PlayerSettings.GetApplicationIdentifier(EditorUserBuildSettings.selectedBuildTargetGroup);
            PlayerSettings.SetApplicationIdentifier(EditorUserBuildSettings.selectedBuildTargetGroup, ApplicationIdentifier);
        }

        public override void RestoreProject(BuildTarget buildTarget) {
            PlayerSettings.SetApplicationIdentifier(EditorUserBuildSettings.selectedBuildTargetGroup, origValue);
        }
    }
}
