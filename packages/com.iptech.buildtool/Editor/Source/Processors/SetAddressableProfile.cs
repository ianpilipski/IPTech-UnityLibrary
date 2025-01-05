using UnityEditor;

#if ENABLE_UNITY_ADDRESSABLES
using UnityEditor.AddressableAssets;
#endif

namespace IPTech.BuildTool.Processors {
    public class SetAddressableProfile : BuildProcessor {
        
        public string ProfileName;

        private string origValue;

        public override void ModifyProject(BuildTarget buildTarget) {
            base.ModifyProject(buildTarget);
#if ENABLE_UNITY_ADDRESSABLES
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            string profileId = settings.profileSettings.GetProfileId(ProfileName);

            origValue = settings.activeProfileId;
            settings.activeProfileId = profileId;
#endif
        }

        public override void RestoreProject(BuildTarget buildTarget) {
            base.RestoreProject(buildTarget);
#if ENABLE_UNITY_ADDRESSABLES
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            settings.activeProfileId = origValue;
#endif
        }
    }
}
