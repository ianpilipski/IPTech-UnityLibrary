using UnityEditor;
using UnityEngine;

namespace IPTech.Utils.Editor
{
    public class JavaHomeInEditor
    {
        [InitializeOnLoadMethod]
        public static void Initialize()
        {
            // set the JAVA_HOME environment to the unity installed version
            var javaHome = System.Environment.GetEnvironmentVariable("JAVA_HOME");
            var unityJdkPath = UnityEditor.EditorApplication.applicationContentsPath + "/Tools/Java";
                
            if (javaHome != unityJdkPath)
            {
                Debug.Log("[IPTech.Utils] Setting JAVA_HOME to: " + unityJdkPath);
                System.Environment.SetEnvironmentVariable("JAVA_HOME", unityJdkPath);
            }
        }
    }
}