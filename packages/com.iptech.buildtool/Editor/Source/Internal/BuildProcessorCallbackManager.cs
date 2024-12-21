using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
#if UNITY_ANDROID
using UnityEditor.Android;
#endif

namespace IPTech.BuildTool.Internal {
    public class BuildProcessorCallbackManager : IPreprocessBuildWithReport, IPostprocessBuildWithReport
#if UNITY_ANDROID
        ,IPostGenerateGradleAndroidProject
#endif
    {
        public int callbackOrder => 0;

        public void OnPostGenerateGradleAndroidProject(string path) {
            ForEachBuildProcessor(bp => {
                BuildToolLogger.Log($"Calling PostGenerateGradleAndroidProject on {bp.name}");
                bp.PostGenerateGradleAndroidProject(path);
            });
        }

        public void OnPostprocessBuild(BuildReport report) {
            ForEachBuildProcessor(bp => {
                BuildToolLogger.Log($"Calling PostProcessBuild on {bp.name}");
                bp.PostProcessBuild(report);
            });
        }

        public void OnPreprocessBuild(BuildReport report) {
            ForEachBuildProcessor(bp => {
                BuildToolLogger.Log($"Calling PreprocessBuild on {bp.name}");
                bp.PreprocessBuild(report);
            });
        }

        void ForEachBuildProcessor(Action<BuildProcessor> action) {
            try {
                List<BuildProcessor> bps = BuildToolsSettings.instance.BuildProcessors;
                if(PlayerBuildConfig.CurrentlyBuildingConfig != null) {
                    bps = bps.Concat(PlayerBuildConfig.CurrentlyBuildingConfig.BuildProcessors).ToList();
                }

                if(bps != null) {
                    foreach(var pp in bps) {
                        action(pp);
                    }
                }
            } catch(Exception e) {
                if(!(e is BuildFailedException)) {
                    throw new BuildFailedException(e);
                } else {
                    throw;
                }
            }
        }
    }
}
