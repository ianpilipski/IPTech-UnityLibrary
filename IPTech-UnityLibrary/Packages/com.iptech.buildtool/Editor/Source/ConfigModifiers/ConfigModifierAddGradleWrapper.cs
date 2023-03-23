using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace IPTech.BuildTool {
    public class ConfigModifierAddGradleWrapper : ConfigModifier {
        public bool AddGradleWrapper;

        bool origValue;

        public override void ModifyProject(BuildTarget buildTarget) {
            origValue = CurrentBuildSettings.Inst.AddGradlewWrapper;
            CurrentBuildSettings.Inst.AddGradlewWrapper = AddGradleWrapper;
        }

        public override void RestoreProject(BuildTarget buildTarget) {
            CurrentBuildSettings.Inst.AddGradlewWrapper = origValue;
        }
    }
}