using UnityEditor.Build.Reporting;
using UnityEngine;
#if UNITY_IOS
using System.IO;
using UnityEditor;
using UnityEditor.iOS.Xcode;
#endif

namespace IPTech.BuildTool.Processors {
    [Tooltip("This will update the Info.plist NSUserTrackingUsageDescription")]
    public class IOSNSUserTrackingDescription : BuildProcessor {
        public string NSUserTrackingUsageDescription = "Your data will be used to provide you a better and personalized ad experience.";

        public override void PostProcessBuild(BuildReport report) {
#if UNITY_IOS
            if(report.summary.platform == BuildTarget.iOS) {
                string plistPath = report.summary.outputPath + "/Info.plist";

                PlistDocument plist = new PlistDocument();
                plist.ReadFromString(File.ReadAllText(plistPath));

                PlistElementDict rootDict = plist.root;
                rootDict.SetString("NSUserTrackingUsageDescription", NSUserTrackingUsageDescription);

                File.WriteAllText(plistPath, plist.WriteToString()); // Override Info.plist
            }
#endif
        }
    }
}
