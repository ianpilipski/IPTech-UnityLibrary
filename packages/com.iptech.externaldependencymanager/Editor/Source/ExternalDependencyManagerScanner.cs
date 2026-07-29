using UnityEditor;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine;
using UnityEditor.PackageManager;
using System.Threading;
using System;

namespace IPTech.ExternalDependencyManager
{
    public static class ExternalDependencyManagerScanner
    {
        private const string STATE_INSTALLING = "InstallPackage";
        private const string STATE_ALREADYLOGGED = "AlreadyLogged";
        private const string StateKey = "ExternalDependencyManagerScanner_State";
        private static Task<bool> _scanTask;

        [InitializeOnLoadMethod]
        private static void InitializeOnLoad()
        {
            string currentStep = SessionState.GetString(StateKey, "None");
            
            if(Application.isBatchMode)
            {
                SessionState.EraseString(StateKey);
                return;
            }

            if(currentStep == STATE_INSTALLING)
            {
                SessionState.EraseString(StateKey);
                PerformInstall();
            }
            else 
            {
                if(ExternalDependencyManagerSettings.instance.ScanOnStartup)
                {
                    ScanForEDM4U(currentStep != STATE_ALREADYLOGGED);
                    SessionState.SetString(StateKey, STATE_ALREADYLOGGED);
                }
            }
        }

        public static async void ScanForEDM4U(bool logOnHasEDM4U = true)
        {
            if(_scanTask != null && !_scanTask.IsCompleted)
            {
                Debug.Log("[IPTech][ExternalDependencyManager] ScanOnStartup: EDM4U is already being scanned.");
                return;
            }
            
            try
            {
                _scanTask = Task.Run(HasEDM4UInstalled);
                var res = await _scanTask;
                if(res)
                {
                    if(!logOnHasEDM4U) return;
                    Debug.Log("[IPTech][ExternalDependencyManager] ScanOnStartup: EDM4U is installed: " + res);
                    return;
                }
                ShowEDM4UInstallationPrompt();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        public static bool HasEDM4UInstalled()
        {
            return TypeCache.GetTypesDerivedFrom<object>("Google.PackageManagerResolver").Any();
        }

        public static void ShowEDM4UInstallationPrompt()
        {
            var res = EditorUtility.DisplayDialog("EDM4U Installation", "EDM4U is not installed. Would you like to install it?", "Yes", "No");
            if(res)
            {
                SessionState.SetString(StateKey, STATE_INSTALLING);
                PerformInstall();
            }
        }

        static async void PerformInstall()
        {
            try 
            {
                InstallOpenUPMScopedRegistry();
                await InstallEDM4U(CancellationToken.None);
            } 
            catch(Exception ex)
            {
                Debug.LogException(ex);
                Debug.LogError($"Error installing EDM4U: {ex.Message}");
            }
        }

        public static bool InstallOpenUPMScopedRegistry()
        {
            return ScopedRegistryUtility.AddRegistry("OpenUPM", "https://package.openupm.com", new string[] { "com.google.external-dependency-manager" });
        }

        public static async Task InstallEDM4U(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();            

            var addRequest = Client.Add("com.google.external-dependency-manager");
            while(!addRequest.IsCompleted)
            {
                await Task.Yield();
                ct.ThrowIfCancellationRequested();
            }

            if(addRequest.Error != null)
            {
                throw new System.Exception($"Error installing EDM4U: errorCode={addRequest.Error.errorCode}, {addRequest.Error.message}");
            }
        }
    }
}
