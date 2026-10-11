using UnityEditor;
using UnityEngine;  

namespace IPTech.EditorExtensions.Tools
{
    public static class ReserializeUtility
    { 
        [MenuItem("IPTech/Project/Reserialize All Assets")]
        public static void ReserializeAll()
        {
            AssetDatabase.ForceReserializeAssets();
            Debug.Log("Assets have been reserialized.");
        }
    }
}