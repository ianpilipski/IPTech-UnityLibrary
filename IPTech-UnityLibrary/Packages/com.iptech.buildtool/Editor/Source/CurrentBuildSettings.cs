using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace IPTech.BuildTool {
    public class CurrentBuildSettings {
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
