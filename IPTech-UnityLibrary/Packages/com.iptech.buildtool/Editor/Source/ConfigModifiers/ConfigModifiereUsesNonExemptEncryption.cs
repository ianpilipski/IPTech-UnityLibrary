using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace IPTech.BuildTool {
    public class ConfigModifiereUsesNonExemptEncryption : ConfigModifier {
        public bool UsesNonExemptEncryption;

        bool origValue;

        public override void ModifyProject(BuildTarget buildTarget) {
            origValue = CurrentBuildSettings.Inst.UsesNonExemptEncryption;
            CurrentBuildSettings.Inst.UsesNonExemptEncryption = UsesNonExemptEncryption;
        }

        public override void RestoreProject(BuildTarget buildTarget) {
            CurrentBuildSettings.Inst.UsesNonExemptEncryption = origValue;
        }
    }
}