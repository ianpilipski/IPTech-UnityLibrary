using UnityEditor.Build.Reporting;
using UnityEngine;
#if UNITY_IOS
using System.IO;
using UnityEditor;
using UnityEditor.iOS.Xcode;
#endif

namespace IPTech.BuildTool.Processors {
    [Tooltip("This will update the Info.plist ITSAppUsesNonExemptEncryption")]
    public class IOSUsesNonExemptEncryption : BuildProcessor {
        public bool UsesNonExemptEncryption;

        public override void PostProcessBuild(BuildReport report) {
#if UNITY_IOS
            if(report.summary.platform == BuildTarget.iOS) {
                string plistPath = report.summary.outputPath + "/Info.plist";

                PlistDocument plist = new PlistDocument();
                plist.ReadFromString(File.ReadAllText(plistPath));

                PlistElementDict rootDict = plist.root;
                rootDict.SetBoolean("ITSAppUsesNonExemptEncryption", UsesNonExemptEncryption);

                File.WriteAllText(plistPath, plist.WriteToString()); // Override Info.plist
            }
#endif
        }
    }
}
