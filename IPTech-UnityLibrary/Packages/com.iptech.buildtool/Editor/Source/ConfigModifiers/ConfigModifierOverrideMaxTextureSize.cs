using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace IPTech.BuildTool {
    public class ConfigModifierOverideMaxTextureSize : ConfigModifier {
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

        public override void ModifyProject() {
            origValue = EditorUserBuildSettings.overrideMaxTextureSize;
            EditorUserBuildSettings.overrideMaxTextureSize = (int)OverrideMaxTextureSize;
        }

        public override void RestoreProject() {
            EditorUserBuildSettings.overrideMaxTextureSize = origValue;
        }
    }
}