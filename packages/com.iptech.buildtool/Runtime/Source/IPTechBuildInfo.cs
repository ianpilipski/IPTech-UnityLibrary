using UnityEngine;

namespace IPTech.BuildTool {
    [CreateAssetMenu(menuName = "IPTech/BuildTool/IPTechBuildInfo", fileName = nameof(IPTechBuildInfo))]
    public class IPTechBuildInfo : ScriptableObject {
        public string BuildNumber;

        public static IPTechBuildInfo LoadFromResources() {
            return Resources.Load<IPTechBuildInfo>(nameof(IPTechBuildInfo));
        }
    }
}
