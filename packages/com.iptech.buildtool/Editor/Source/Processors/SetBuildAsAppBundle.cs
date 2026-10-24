using System.Collections.Generic;
using UnityEditor;

namespace IPTech.BuildTool.Processors {
    public class SetBuildAsAppBundle : BuildProcessor {
        public bool BuildAsAppBundle;

        bool origValue;

       
        public override void ModifyProject(BuildTarget buildTarget, IReadOnlyDictionary<string,string> args) {
            origValue = EditorUserBuildSettings.buildAppBundle;
            EditorUserBuildSettings.buildAppBundle = BuildAsAppBundle;
        }

        public override void RestoreProject(BuildTarget buildTarget, IReadOnlyDictionary<string,string> args) {
            EditorUserBuildSettings.buildAppBundle = origValue;
        }
    }
}
