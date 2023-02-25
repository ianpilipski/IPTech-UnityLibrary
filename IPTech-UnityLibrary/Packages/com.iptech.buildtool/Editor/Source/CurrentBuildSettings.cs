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
            readonly CurrentBuildSettings origInst;
            readonly bool exportAsGoogleAndroidProject;
            readonly string buildNumber;
            readonly int bundleVersionCode;
            readonly string applicationIdentifier;

            public Scoped() {
                if(_inst==null) {
                    _ = Inst;
                }
                origInst = (CurrentBuildSettings)_inst.MemberwiseClone();

                exportAsGoogleAndroidProject = EditorUserBuildSettings.exportAsGoogleAndroidProject;
                buildNumber = PlayerSettings.iOS.buildNumber;
                bundleVersionCode = PlayerSettings.Android.bundleVersionCode;
                applicationIdentifier = PlayerSettings.GetApplicationIdentifier(EditorUserBuildSettings.selectedBuildTargetGroup);
            }

            public void Dispose() {
                _inst = origInst;

                EditorUserBuildSettings.exportAsGoogleAndroidProject = exportAsGoogleAndroidProject;
                PlayerSettings.iOS.buildNumber = buildNumber;
                PlayerSettings.Android.bundleVersionCode = bundleVersionCode;
                PlayerSettings.SetApplicationIdentifier(EditorUserBuildSettings.selectedBuildTargetGroup, applicationIdentifier);
            }
        }
    }


}
