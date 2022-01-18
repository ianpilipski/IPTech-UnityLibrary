using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IPTech.Localization
{
    public interface ILocalizationData {
        bool TryGetLocalizedStringEntry(string key, out ILocalizedString localizedString);
	}

	[CreateAssetMenu(menuName = "IPTech/Localization/Data")]
    public class LocalizationData : ScriptableObject, ILocalizationData {
        public List<LocalizedString> StringEntries;

		public bool TryGetLocalizedStringEntry(string key, out ILocalizedString localizedString) {
			for(int i=0;i<StringEntries.Count;i++) {
				LocalizedString lString = StringEntries[i];
				if(key.Equals(lString.name, System.StringComparison.OrdinalIgnoreCase)) {
					localizedString = lString;
					return true;
				}
			}
			localizedString = null;
			return false;
		}
    }
}
