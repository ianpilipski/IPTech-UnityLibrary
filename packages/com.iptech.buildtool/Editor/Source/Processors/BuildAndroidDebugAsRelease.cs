using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace IPTech.BuildTool.Processors {
    [Tooltip("If Development build is set, this will make unity bundle the release instead so it can be uploaded to google play store")]
    public class BuildAndroidDebugAsRelease : BuildProcessor
    {
        public override void PostGenerateGradleAndroidProject(string path) {
            if(EditorUserBuildSettings.buildAppBundle) {
                AndroidTools.MakeBundleDebugBuildBundleReleaseInsead(path);
            }
        }
    }
}
