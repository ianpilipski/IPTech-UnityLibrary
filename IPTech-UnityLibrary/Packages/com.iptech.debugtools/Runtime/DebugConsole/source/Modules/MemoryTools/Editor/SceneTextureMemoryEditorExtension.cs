#if !(UNITY_EDITOR || DEVELOPMENT_BUILD || QA_BUILD)
#define CONTAINERDEBUGSERVICE_DISABLED
#endif

#if !CONTAINERDEBUGSERVICE_DISABLED

using UnityEngine;
using UnityEditor;

namespace IPTech.DebugConsoleService
{
    public class SceneTextureMemoryEditorExtension {

        private const string ENABLE_MENU = "IPTech/Memory/Scene Memory In Hierarchy/Enable";
        private const string DISABLE_MENU = "IPTech/Memory/Scene Memory In Hierarchy/Disable";
        private static bool extensionEnabled;
        private static GUIStyle alignRightLabelStyle;

        [MenuItem(ENABLE_MENU)]
        public static void EnableCalculateTextureMemoryInSceneHierarchy() {
            if(!extensionEnabled) {
                EditorApplication.hierarchyWindowItemOnGUI += HandleHierarchyWindowItemOnGUI;
                extensionEnabled = true;
                EditorApplication.RepaintHierarchyWindow();
            }
        }

        [MenuItem(DISABLE_MENU)]
        public static void DisableCalculateTextureMemoryInSceneHierarchy() {
            EditorApplication.hierarchyWindowItemOnGUI -= HandleHierarchyWindowItemOnGUI;
            extensionEnabled = false;
            EditorApplication.RepaintHierarchyWindow();
        }

        [MenuItem(ENABLE_MENU, true)]
        private static bool ShouldShowEnableMenu() {
            return !extensionEnabled;
        }

        [MenuItem(DISABLE_MENU, true)]
        private static bool ShouldShowDisableMenu() {
            return extensionEnabled;
        }

        static void HandleHierarchyWindowItemOnGUI(int instanceID, Rect selectionRect) {
            EnsureLabelStyleIsCreated();

            string memString = GenerateStringForMemoryLabel(instanceID);
            DrawMemoryLabel(selectionRect, memString);
        }

        private static void EnsureLabelStyleIsCreated() {
            if(alignRightLabelStyle == null) {
                alignRightLabelStyle = new GUIStyle(GUI.skin.label) {
                    alignment = TextAnchor.MiddleRight
                };
            }
        }

        private static void DrawMemoryLabel(Rect selectionRect, string memString) {
            Rect r = new Rect(selectionRect);
            GUI.Label(r, memString, alignRightLabelStyle);
        }

        private static string GenerateStringForMemoryLabel(int instanceID) {
            GameObject obj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if(obj!=null) {
                MemoryData mem = CalculateTextureMemoryForGameObject(obj);
                return GenerateStringForMemoryMB(mem.SizeInMegabytes);
            }
            return "---";
        }

        private static string GenerateStringForMemoryMB(float memSizeInMB) {
            return string.Format("{0:F2} MB", memSizeInMB);
        }

        private static MemoryData CalculateTextureMemoryForGameObject(GameObject obj) {
            Texture[] textures = RuntimeInspector.FindAllTexturesUnder(obj);
            return TextureMemoryAnalyzer.CalculateMemory(textures);
        }
    }
}

#endif
