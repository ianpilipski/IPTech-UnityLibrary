using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.iOS.Xcode;
using UnityEngine;

namespace IPTech.AgeVerification.iOS.Editor
{
    public class AgeRangePostProcessBuild : IPostprocessBuildWithReport
    {
        public int callbackOrder => 1000000;

        public void OnPostprocessBuild(BuildReport report)
        {
            ConditionallyPostProcessBuild(report);
        }

        private static void ConditionallyPostProcessBuild(BuildReport report)
        {
            if(AgeRangeSettings.instance.DisablePostProcessor) return;
            if (report.summary.platform != UnityEditor.BuildTarget.iOS) return;
            Debug.Log("[AgeRange] Adding Declared Age Range capability to Xcode project.");
            AddDeclaredAgeRangeCapabilityToProjectAtDir(report.summary.outputPath);
        }

        public static void AddDeclaredAgeRangeCapabilityToProjectAtDir(string projectDir)
        {
            var project = new PBXProject();
            var pbxPath = PBXProject.GetPBXProjectPath(projectDir);
            project.ReadFromFile(pbxPath);
            var xCodeTarget = project.GetUnityMainTargetGuid();

            string entitlementsFileName = project.GetBuildPropertyForAnyConfig(xCodeTarget, "CODE_SIGN_ENTITLEMENTS");
            if (string.IsNullOrWhiteSpace(entitlementsFileName))
            {
                entitlementsFileName = "Entitlements.entitlements";
            }
            var capManager = new ProjectCapabilityManager(
                pbxPath,
                entitlementsFileName,
                "Unity-iPhone");

            capManager.AddDeclaredAgeRangeCapability();

            capManager.WriteToFile();
        }
    }
}

