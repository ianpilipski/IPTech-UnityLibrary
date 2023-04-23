using UnityEditor;

namespace IPTech.BuildTool {
    public class ConfigModifierBuildAsAppBundle : ConfigModifier {
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
