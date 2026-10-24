using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;

namespace IPTech.BuildTool.Processors {
    public class SetApplicationIdentifier : BuildProcessor {
        public string ApplicationIdentifier;

        string origValue;

        void OnEnable() {
            if(string.IsNullOrEmpty(ApplicationIdentifier)) {
                var namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
                ApplicationIdentifier = PlayerSettings.GetApplicationIdentifier(namedBuildTarget);
            }
        }

        public override void ModifyProject(BuildTarget buildTarget, IReadOnlyDictionary<string,string> args) {
            var namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            origValue = PlayerSettings.GetApplicationIdentifier(namedBuildTarget);
            PlayerSettings.SetApplicationIdentifier(namedBuildTarget, ApplicationIdentifier);
        }

        public override void RestoreProject(BuildTarget buildTarget, IReadOnlyDictionary<string,string> args) {
            var namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            PlayerSettings.SetApplicationIdentifier(namedBuildTarget, origValue);
        }
    }
}
