using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace IPTech.BuildTool {
    public class ConfigModifierSetiOSTargetSDK : ConfigModifier {
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