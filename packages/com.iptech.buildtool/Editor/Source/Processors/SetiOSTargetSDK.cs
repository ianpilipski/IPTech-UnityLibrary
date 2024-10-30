using UnityEditor;

namespace IPTech.BuildTool.Processors {
    public class SetiOSTargetSDK : BuildProcessor {
        public iOSSdkVersion TargetSDK;

        iOSSdkVersion origValue;

        public override void ModifyProject(BuildTarget buildTarget) {
            origValue = PlayerSettings.iOS.sdkVersion;
            PlayerSettings.iOS.sdkVersion = TargetSDK;
        }

        public override void RestoreProject(BuildTarget buildTarget) {
            PlayerSettings.iOS.sdkVersion = origValue;
        }
    }
}
