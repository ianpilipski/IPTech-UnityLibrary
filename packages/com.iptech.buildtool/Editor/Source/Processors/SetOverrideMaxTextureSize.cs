using UnityEditor;

namespace IPTech.BuildTool.Processors {
    public class SetOverrideMaxTextureSize : BuildProcessor {
        public EMaxTextureSize OverrideMaxTextureSize;

        public enum EMaxTextureSize {
            NoOverride = 0,
            Max64 = 64,
            Max128 = 128,
            Max256 = 256,
            Max512 = 512,
            Max1024 = 1024,
            Max2048 = 2048
        }

        int origValue;

        public override void ModifyProject(BuildTarget buildTarget) {
            origValue = EditorUserBuildSettings.overrideMaxTextureSize;
            EditorUserBuildSettings.overrideMaxTextureSize = (int)OverrideMaxTextureSize;
        }

        public override void RestoreProject(BuildTarget buildTarget) {
            EditorUserBuildSettings.overrideMaxTextureSize = origValue;
        }
    }
}
