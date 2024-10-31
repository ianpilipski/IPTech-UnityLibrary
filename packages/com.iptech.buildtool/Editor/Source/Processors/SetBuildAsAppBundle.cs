using UnityEditor;

namespace IPTech.BuildTool.Processors {
    public class SetBuildAsAppBundle : BuildProcessor {
        public bool BuildAsAppBundle;

        bool origValue;

       
        public override void ModifyProject(BuildTarget buildTarget) {
            origValue = EditorUserBuildSettings.buildAppBundle;
            EditorUserBuildSettings.buildAppBundle = BuildAsAppBundle;
        }

        public override void RestoreProject(BuildTarget buildTarget) {
            EditorUserBuildSettings.buildAppBundle = origValue;
        }
    }
}
