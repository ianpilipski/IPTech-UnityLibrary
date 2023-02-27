using UnityEngine;

namespace IPTech.BuildTool {
    public class IPTechBuildInfo : ScriptableObject {
        public string BuildNumber;

        public static IPTechBuildInfo LoadFromResources() {
            return Resources.Load<IPTechBuildInfo>(nameof(IPTechBuildInfo));
        }
    }
}
