using UnityEngine;
using UnityEditor;

namespace IPTech.AgeVerification.iOS.Editor
{
    [FilePath("ProjectSettings/IPTech/AgeRangeSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    public class AgeRangeSettings : ScriptableSingleton<AgeRangeSettings>
    {
        [SerializeField]
        private bool _disablePostProcessor = false;

        public bool DisablePostProcessor
        {
            get => _disablePostProcessor;
            set { _disablePostProcessor = value; Save(true); }
        }
    }
}
