using UnityEditor.Build.Reporting;
using UnityEngine;
#if UNITY_IOS
using System.IO;
using UnityEditor;
using UnityEditor.iOS.Xcode;
#endif

namespace IPTech.BuildTool.Processors
{
    [Tooltip("This will set NSAppTransportSecurity settings")]
    public class IOSNSAppTransportSecurity : BuildProcessor
    {
        const string NSAPPTRANSPORTSECURITYKEY = "NSAppTransportSecurity";
        
        public bool AllowArbitraryLoads = true;

        public override void PostProcessBuild(BuildReport report)
        {
#if UNITY_IOS
            if(report.summary.platform == BuildTarget.iOS) {
                string plistPath = report.summary.outputPath + "/Info.plist";

                PlistDocument plist = new PlistDocument();
                plist.ReadFromString(File.ReadAllText(plistPath));

                PlistElementDict rootDict = plist.root;
                
                if(!rootDict.values.ContainsKey(NSAPPTRANSPORTSECURITYKEY)) {
                    rootDict.CreateDict(NSAPPTRANSPORTSECURITYKEY);
                }
                PlistElementDict appTransSec = rootDict[NSAPPTRANSPORTSECURITYKEY].AsDict();

                appTransSec.SetBoolean("NSAllowsArbitraryLoads", AllowArbitraryLoads);

                File.WriteAllText(plistPath, plist.WriteToString()); // Override Info.plist
            }
#endif
        }
    }
}
