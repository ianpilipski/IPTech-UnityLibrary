using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IPTech.BuildTool {
    public class ConfigModifiereUsesNonExemptEncryption : ConfigModifier {
        public bool UsesNonExemptEncryption;

        bool origValue;

        public override void ModifyProject() {
            origValue = CurrentBuildSettings.Inst.UsesNonExemptEncryption;
            CurrentBuildSettings.Inst.UsesNonExemptEncryption = UsesNonExemptEncryption;
        }

        public override void RestoreProject() {
            CurrentBuildSettings.Inst.UsesNonExemptEncryption = origValue;
        }
    }
}