using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IPTech.Localization
{
    public interface ILocalizationService {
        SystemLanguage CurrentLanguage { get; set; }
        string GetText(string key);
	}

    public class LocalizationService {
        readonly ILocalizationData _data;
        readonly ILogger _logger;
        SystemLanguage _currentLanguage;

        public LocalizationService(ILocalizationData data, ILogger logger) {
            _data = data;
            _logger = logger;
            _currentLanguage = Application.systemLanguage;
		}

        public SystemLanguage CurrentLanguage => _currentLanguage;

        public string GetText(string key) {
            if(_data.TryGetLocalizedStringEntry(key, out ILocalizedString localizedString)) {
				switch(_currentLanguage) {
                    case SystemLanguage.French:
                        return localizedString.French;
                    case SystemLanguage.Italian:
                        return localizedString.Italian;
                    case SystemLanguage.German:
                        return localizedString.German;
                    case SystemLanguage.Spanish:
                        return localizedString.Spanish;
                    default:
                        return localizedString.English;
				}
			}
            _logger.LogError(nameof(LocalizationService), "No localization found for " + key);
            return key;
		}
    }
}
