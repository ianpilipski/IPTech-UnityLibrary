using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using System;

namespace IPTech.BuildTool.Processors {
    [Tooltip("Adds shell scripts configured to build the xcode project artifact.")]
    public class IOSBuildScripts : BuildProcessor {
        public List<Config> Builds;
        
        public override void PostProcessBuild(BuildReport report) {

            base.PostProcessBuild(report);
        }

        [Serializable]
        public class Config {
            public string Name;
            public EScheme Scheme;
            public string ProvisioningProfile;
            public string ExportOptionsPlist;
        }

        [Serializable]
        public class Export {
            public string Name;
            public string ProvisionigProfile;
            public string ExportOptionPlist;
        }

        public enum EScheme {
            Release,
            Debug
        }
    }
}
