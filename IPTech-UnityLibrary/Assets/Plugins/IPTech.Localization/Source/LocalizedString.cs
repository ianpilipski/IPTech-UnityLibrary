using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IPTech.Localization {
	public interface ILocalizedString {
		string English { get; }
		string French { get; }
		string Italian { get; }
		string German { get; }
		string Spanish { get; }
	}

	[CreateAssetMenu(menuName = "IPTech/Localization/String Entry")]
	public class LocalizedString : ScriptableObject, ILocalizedString {
        public string English;
		public string French;
		public string Italian;
		public string German;
		public string Spanish;

		string ILocalizedString.English => English;
		string ILocalizedString.French => French;
		string ILocalizedString.Italian => Italian;
		string ILocalizedString.German => German;
		string ILocalizedString.Spanish => Spanish;
	}
}
