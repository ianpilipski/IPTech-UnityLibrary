using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IPTech.BuildTool {
    public class ConfigModifierAddGradleWrapper : ConfigModifier {
        public bool AddGradleWrapper;

        bool origValue;

        public override void ModifyProject() {
            origValue = CurrentBuildSettings.Inst.AddGradlewWrapper;
            CurrentBuildSettings.Inst.AddGradlewWrapper = AddGradleWrapper;
        }

        public override void RestoreProject() {
            CurrentBuildSettings.Inst.AddGradlewWrapper = origValue;
        }
    }
}