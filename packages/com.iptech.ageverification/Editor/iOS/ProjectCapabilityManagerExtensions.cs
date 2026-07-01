using System;
using System.Reflection;
using UnityEditor.iOS.Xcode;

namespace IPTech.AgeVerification.iOS.Editor
{
    public static class ProjectCapabilityManagerExtensions
    {
        public static void AddDeclaredAgeRangeCapability(this ProjectCapabilityManager manager)
        {
            const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var managerType = typeof(ProjectCapabilityManager);

            var projectField = managerType.GetField("project", bindingFlags);
            var entitlementFilePathField = managerType.GetField("m_EntitlementFilePath", bindingFlags);
            var targetGuidField = managerType.GetField("m_TargetGuid", bindingFlags);
            var getOrCreateEntitlementDocMethod = managerType.GetMethod("GetOrCreateEntitlementDoc", bindingFlags);
            if (projectField == null ||
                entitlementFilePathField == null ||
                targetGuidField == null ||
                getOrCreateEntitlementDocMethod == null)
                throw new Exception("Can't add DeclaredAgeRange capability programmatically in this Unity version.");

            var entitlementFilePath = entitlementFilePathField.GetValue(manager) as string;
            var entitlementDoc = (PlistDocument)getOrCreateEntitlementDocMethod.Invoke(manager, new object[] { });
            if (entitlementDoc != null)
            {
                var plistBoolean = new PlistElementBoolean(true);
                entitlementDoc.root["com.apple.developer.declared-age-range"] = plistBoolean;
            }

            var project = (PBXProject)projectField.GetValue(manager);
            var capability = CreateDeclaredAgeRangeCapability();

            var mainTargetGuid = (string)targetGuidField.GetValue(manager);

            project.AddCapability(mainTargetGuid, capability, entitlementFilePath);
        }

        private static PBXCapabilityType CreateDeclaredAgeRangeCapability()
        {
            return new PBXCapabilityType(true, "DeclaredAgeRange.framework", false);
        }
    }
}
