
using UnityEditor;
using UnityEngine;

namespace IPTech.ExternalDependencyManager
{
    [FilePath("ProjectSettings/IPTech/ExternalDependencyManagerSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    public class ExternalDependencyManagerSettings : ScriptableSingleton<ExternalDependencyManagerSettings>
    {
        [SerializeField] private bool _scanOnStartup = true;

        public bool ScanOnStartup
        {
            get => _scanOnStartup;
            set
            {
                if(_scanOnStartup != value)
                {
                    _scanOnStartup = value;
                    Save(true);
                }
            }
        }
    }
}
