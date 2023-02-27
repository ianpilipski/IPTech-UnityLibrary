using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace IPTech.BuildTool {
    public class CurrentBuildSettings {
        static CurrentBuildSettings _inst;

        public bool UsesNonExemptEncryption;
        public bool AddGradlewWrapper;

        public static CurrentBuildSettings Inst {
            get {
                if(_inst == null) {
                    _inst = new CurrentBuildSettings() {
                        UsesNonExemptEncryption = BuildToolsSettings.Inst.UsesNonExemptEncryption,
                        AddGradlewWrapper = BuildToolsSettings.Inst.AddGradleWrapper
                    };
                }
                return _inst;
            }
        }

        public class Scoped : IDisposable {
            readonly string buildNumber;
            readonly int bundleVersionCode;
            
            public Scoped() {
                buildNumber = PlayerSettings.iOS.buildNumber;
                bundleVersionCode = PlayerSettings.Android.bundleVersionCode;
            }

            public void Dispose() {
                PlayerSettings.iOS.buildNumber = buildNumber;
                PlayerSettings.Android.bundleVersionCode = bundleVersionCode;
                
            }
        }
    }


}
