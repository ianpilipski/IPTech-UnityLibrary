using System.IO;
using UnityEditor;
using UnityEngine;

namespace IPTech.ConsentScreen.Editor 
{

    public class ConsentScreenHandlerCreator
    {
        [MenuItem("Assets/Create/IPTech/ConsentScreen/ConsentScreenHandler")]
        public static void CreateConsentScreenHandler()
        {
            var path = Path.Combine(GetCurrentSelectedFolder(), "ConsentScreenHandler.cs");
            if (!string.IsNullOrEmpty(path))
            {
                var fileContent = CreateConsentScreenHandlerFileContent();
                System.IO.File.WriteAllText(path, fileContent);
                AssetDatabase.Refresh();
            }
        }

        public static string CreateConsentScreenHandlerFileContent()
        {
            return @"using UnityEngine;
using IPTech.ConsentScreen;

public class ConsentScreenHandler : IPTech.ConsentScreen.ConsentScreenHandler
{
    public override ConsentInfo GetCurrentConsentInfo() 
    {
        #error implement GetCurrentConsentInfo
        return new ConsentInfo();
    }

    public override void SetConsentInfo(ConsentInfo info)
    {
        #error implement SetConsentInfo
    }
}
";
        }

        private static string GetCurrentSelectedFolder()
        {
            // Get the GUIDs of all selected assets
            string[] guids = Selection.assetGUIDs;
            
            if (guids.Length == 0)
            {
                return "Assets"; // Default fallback
            }

            // Convert the first selected GUID to an asset path
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);

            // If the path is a file, get its parent directory
            if (!Directory.Exists(path))
            {
                path = Path.GetDirectoryName(path);
            }

            return path;
        }
    }
}
