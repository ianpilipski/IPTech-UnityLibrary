using UnityEditor.Build.Reporting;
using System.IO;

#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif

namespace IPTech.BuildTool.Processors {
    public class IOSBitCodeEnabled : BuildProcessor {
        public bool EnableBitCode;

        public override void PostProcessBuild(BuildReport report) {
            if(report.summary.platform == UnityEditor.BuildTarget.iOS) {
#if UNITY_IOS
                string projPath = Path.Combine(report.summary.outputPath, "/Unity-iPhone.xcodeproj/project.pbxproj");
                var enabledString = EnableBitCode ? "YES" : "NO";

                var pbxProject = new PBXProject();
                pbxProject.ReadFromFile(projPath);

                // Main
                string target = pbxProject.GetUnityMainTargetGuid();
                pbxProject.SetBuildProperty(target, "ENABLE_BITCODE", enabledString);


                // Unity Tests
                target = pbxProject.TargetGuidByName(PBXProject.GetUnityTestTargetName());
                pbxProject.SetBuildProperty(target, "ENABLE_BITCODE", enabledString);


                // Unity Framework
                target = pbxProject.GetUnityFrameworkTargetGuid();
                pbxProject.SetBuildProperty(target, "ENABLE_BITCODE", enabledString);


                pbxProject.WriteToFile(projPath);
#endif
            }
        }
    }
}
