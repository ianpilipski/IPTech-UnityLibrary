using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace IPTech.BuildTool {
    public class ConfigModifierSetiOSTargetSDK : ConfigModifier {
        public iOSSdkVersion TargetSDK;

        iOSSdkVersion origValue;

        public override void ModifyProject() {
            origValue = PlayerSettings.iOS.sdkVersion;
            PlayerSettings.iOS.sdkVersion = TargetSDK;
        }

        public override void RestoreProject() {
            PlayerSettings.iOS.sdkVersion = origValue;
        }
    }
}