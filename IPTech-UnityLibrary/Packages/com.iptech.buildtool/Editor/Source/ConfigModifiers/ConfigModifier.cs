using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IPTech.BuildTool {
    public abstract class ConfigModifier : ScriptableObject {
        public abstract void ModifyProject();
        public abstract void RestoreProject();
    }
}
