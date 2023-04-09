using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace IPTech.BuildTool {
    public abstract class BuildProcessor : ScriptableObject {
        public virtual void PostProcessBuild(BuildReport report) { }
        public virtual void PreprocessBuild(BuildReport report) { }
        public virtual void PostGenerateGradleAndroidProject(string path) { }
    }
}
