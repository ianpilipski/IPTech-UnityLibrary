using System;
using System.Collections.Generic;
using IPTech.BuildTool.Encryption;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace IPTech.BuildTool
{
    public abstract class BuildProcessor : ScriptableObject {
        public virtual void ModifyProject(BuildTarget buildTarget, IReadOnlyDictionary<string,string> args) { }
        public virtual void RestoreProject(BuildTarget buildTarget, IReadOnlyDictionary<string,string> args) { }
        public virtual BuildPlayerOptions ModifyBuildPlayerOptions(BuildPlayerOptions options, IReadOnlyDictionary<string,string> args) { return options; }

        public virtual void PostProcessBuild(BuildReport report) { }
        public virtual void PreprocessBuild(BuildReport report) { }
        public virtual void PostGenerateGradleAndroidProject(string path) { }

        protected T GetDecryptedItem<T>(EncryptedItem<T>.Reference item) where T : EncryptedItem<T> {
            try {
                return BuildToolsSettings.instance.GetDecryptedItem(item);
            } catch(Exception e) {
                throw new BuildFailedException(e);
            }
        }
    }
}
