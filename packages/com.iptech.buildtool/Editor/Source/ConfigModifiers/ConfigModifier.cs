using System;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace IPTech.BuildTool {
    using Encryption;

    public abstract class ConfigModifier : ScriptableObject {
        public abstract void ModifyProject(BuildTarget buildTarget);
        public abstract void RestoreProject(BuildTarget buildTarget);
        public virtual BuildPlayerOptions ModifyBuildPlayerOptions(BuildPlayerOptions options) { return options; }

        protected T GetDecryptedItem<T>(EncryptedItem<T>.Reference item) where T : EncryptedItem<T> {
            try {
                return BuildToolsSettings.instance.GetDecryptedItem(item);
            } catch(Exception e) {
                throw new BuildFailedException(e);
            }
        }
    }
}
