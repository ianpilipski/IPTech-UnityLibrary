using UnityEditor.Build.Reporting;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
#if UNITY_IOS
using System.IO;
using UnityEditor;
using UnityEditor.iOS.Xcode;
#endif

namespace IPTech.BuildTool.Processors
{
    [Tooltip("This will add any additional skadnetwork items")]
    public class IOSSkadNetworkItems : BuildProcessor
    {
        [Serializable]
        public struct SkadIdentifier {
            public string Name;
            public string Identifier;
        }

        const string KEY = "SKAdNetworkItems";
        const string SKADNETWORKIDENTIFIER = "SkAdNetworkIdentifier";

        public List<SkadIdentifier> SkAdNetworkIdentifiers;

        public override void PostProcessBuild(BuildReport report)
        {
#if UNITY_IOS
            if(report.summary.platform == BuildTarget.iOS) {
                string plistPath = report.summary.outputPath + "/Info.plist";

                PlistDocument plist = new PlistDocument();
                plist.ReadFromString(File.ReadAllText(plistPath));

                PlistElementDict rootDict = plist.root;
                
                if(!rootDict.values.ContainsKey(KEY)) {
                    rootDict.CreateArray(KEY);
                }
                PlistElementArray skAdNetworks = rootDict[KEY].AsArray();

                var itemsToAdd = SkAdNetworkIdentifiers.Select(o => o.Identifier).ToList();

                foreach(var item in skAdNetworks.values) {
                    var d = item.AsDict();
                    var idEl = d[SKADNETWORKIDENTIFIER];
                    if(idEl != null) {
                        var id = idEl.AsString();
                        if(!string.IsNullOrWhiteSpace(id)) {
                            if(!itemsToAdd.Contains(id)) {
                                itemsToAdd.Remove(id);
                            }
                        }
                    }
                }

                if(itemsToAdd.Count > 0) {
                    foreach(var item in itemsToAdd) {
                        if(item != null) {
                            var newDict = skAdNetworks.AddDict();
                            newDict.SetString(SKADNETWORKIDENTIFIER, item);
                        }
                    }

                    File.WriteAllText(plistPath, plist.WriteToString()); // Override Info.plist
                }
            }
#endif
        }
    }
}
