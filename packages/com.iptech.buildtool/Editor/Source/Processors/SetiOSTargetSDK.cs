using System.Collections.Generic;
using UnityEditor;

namespace IPTech.BuildTool.Processors {
    public class SetiOSTargetSDK : BuildProcessor {
        public iOSSdkVersion TargetSDK;

        iOSSdkVersion origValue;

        public override void ModifyProject(BuildTarget buildTarget, IReadOnlyDictionary<string,string> args) {
            origValue = PlayerSettings.iOS.sdkVersion;
            PlayerSettings.iOS.sdkVersion = TargetSDK;
        }

        public override void RestoreProject(BuildTarget buildTarget, IReadOnlyDictionary<string,string> args) {
            PlayerSettings.iOS.sdkVersion = origValue;
        }
    }
}
