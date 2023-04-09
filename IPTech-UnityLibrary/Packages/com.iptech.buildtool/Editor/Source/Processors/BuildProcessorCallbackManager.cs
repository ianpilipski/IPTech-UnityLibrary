using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
#if UNITY_ANDROID
using UnityEditor.Android;
#endif

namespace IPTech.BuildTool {
    public class BuildProcessorCallbackManager : IPreprocessBuildWithReport, IPostprocessBuildWithReport
#if UNITY_ANDROID
        ,IPostGenerateGradleAndroidProject
#endif
    {
        public int callbackOrder => 0;

        public void OnPostGenerateGradleAndroidProject(string path) {
            ForEachBuildProcessor(bp => bp.PostGenerateGradleAndroidProject(path));
        }

        public void OnPostprocessBuild(BuildReport report) {
            ForEachBuildProcessor(bp => bp.PostProcessBuild(report));
        }

        public void OnPreprocessBuild(BuildReport report) {
            ForEachBuildProcessor(bp => bp.PreprocessBuild(report));
        }

        void ForEachBuildProcessor(Action<BuildProcessor> action) {
            List<BuildProcessor> bps = BuildToolsSettings.instance.BuildProcessors;
            if(PlayerBuildConfig.CurrentlyBuildingConfig != null) {
                bps = bps.Concat(PlayerBuildConfig.CurrentlyBuildingConfig.BuildProcessors).ToList();
            } 

            if(bps != null) {
                foreach(var pp in bps) {
                    action(pp);
                }
            }
        }
    }
}
