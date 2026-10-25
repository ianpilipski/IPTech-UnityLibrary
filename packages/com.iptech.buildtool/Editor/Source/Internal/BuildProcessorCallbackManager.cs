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

        // we need to cache these so they can persist data they may keep during the build
        // otherwise unity will re-create them after the domain load
        static List<BuildProcessor> cachedGlobalBuildProcessors;

        // these are listed in call order

        public void OnPreprocessBuild(BuildReport report) {
            CacheGlobalBuildProcessors();
            ForEachBuildProcessor(bp => {
                BuildToolLogger.Log($"Calling PreprocessBuild on {bp.name}");
                bp.PreprocessBuild(report);
            });
        }

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
            ClearCachedBuildProcessors();
        }

        // end of call order

        static void CacheGlobalBuildProcessors()
        {
            if(PlayerBuildConfig.CurrentlyBuildingConfig != null) return;
            if(BuildToolsSettings.instance.BuildProcessors == null) return;
            foreach(var bp in BuildToolsSettings.instance.BuildProcessors)
            {
                if(bp != null) {
                    var inst = (BuildProcessor)ScriptableObject.Instantiate(bp);
                    cachedGlobalBuildProcessors ??= new List<BuildProcessor>();
                    cachedGlobalBuildProcessors.Add(inst);
                }
            }
        }

        static void ClearCachedBuildProcessors()
        {
            if(cachedGlobalBuildProcessors != null) {
                foreach(var bp in cachedGlobalBuildProcessors) {
                    ScriptableObject.DestroyImmediate(bp);
                }
                cachedGlobalBuildProcessors.Clear();
            }
        }

        void ForEachBuildProcessor(Action<BuildProcessor> action) {
            try {
                var bps = (PlayerBuildConfig.CurrentlyBuildingConfig != null) 
                    ? PlayerBuildConfig.CurrentlyBuildingConfig.cachedBuildProcessors
                    : cachedGlobalBuildProcessors;

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
