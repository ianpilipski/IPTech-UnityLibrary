using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace IPTech.BuildTool {
    public class BuildConfigEditor : EditorWindow {
        static BuildConfigEditor instance;

        List<BuildConfig> buildConfigs;

        [MenuItem("Window/IPTech/Build Tool")]
        public static void ShowWindow() {
            instance = EditorWindow.GetWindow<BuildConfigEditor>("Build Tool");
            instance.Show();
        }

        private void OnEnable() {
            ReloadConfigs();
        }


        void ReloadConfigs() {
            var bcs = Resources.FindObjectsOfTypeAll<BuildConfig>();
            buildConfigs = bcs.ToList();
        }

        private void OnGUI() {
            foreach(var bc in buildConfigs) {
                var ed = Editor.CreateEditor(bc);
                ed.OnInspectorGUI();
            }

            using(new EditorGUILayout.HorizontalScope()) {
                GUILayout.FlexibleSpace();
                if(GUILayout.Button("Create")) {

                }
            }
        }
    }
}
