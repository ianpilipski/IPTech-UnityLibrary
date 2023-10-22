using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;

namespace UnityEditor.IPTech.UnityServices {
    public static class InstallIronSourceMods
    {
        [InitializeOnLoadMethod]
        public static void InitializeOnLoad() {
            if(!IsIronSourceInIPTechAssembly()) {
                if(IsIronSourceInstalledInMainAssembly()) {
                    Debug.Log("it's installed but not in my assembly");
                    AssetDatabase.ImportPackage("Packages/com.iptech.unityservices.advertising/Editor/iptech-ironsource-mods.unitypackage", true);
                } else {
                    Debug.LogError("you need to install the ironsource sdk to it's default location");
                }
            } else {
#if !IPTECH_IRONSOURCE_INSTALLED
                string[] defines;
                var namedBuildTarget = Build.NamedBuildTarget.FromBuildTargetGroup(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget));
                PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget, out defines);
                if(!defines.Any(d => d == "IPTECH_IRONSOURCE_INSTALLED")) {
                    var newDefines = defines.ToList();
                    newDefines.Add("IPTECH_IRONSOURCE_INSTALLED");
                    PlayerSettings.SetScriptingDefineSymbols(namedBuildTarget, newDefines.ToArray());
                }
#endif
            }
        }

        static bool IsIronSourceInstalledInMainAssembly() {
            var t = Type.GetType("IronSource, Assembly-CSharp");
            return t!=null;
        }

        static bool IsIronSourceInIPTechAssembly() {
            Type t = Type.GetType("IronSource, IPTech.IronSource");
            return t!=null;
        }
    }
}
