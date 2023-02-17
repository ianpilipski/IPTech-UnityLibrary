using System;
using System.Collections;
using System.Collections.Generic;
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

            public Scoped() {
                origInst = (CurrentBuildSettings)CurrentBuildSettings._inst.MemberwiseClone();  
            }

            public void Dispose() {
                _inst = origInst;
            }
        }
    }


}
