using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace IPTech.BuildTool.Editors {
    public class PasswordInputDialog : EditorWindow {
        string retVal;

        public static string Display() {
            var win = ScriptableObject.CreateInstance<PasswordInputDialog>();
            win.retVal = null;
            var pos = win.position;
            pos.width = 640;
            pos.height = 480;
            win.position = pos;
            win.ShowModal();
            return win.retVal;
        }

        void OnGUI() {
            using(new EditorGUILayout.VerticalScope(EditorStyles.inspectorFullWidthMargins)) {
                retVal = EditorGUILayout.PasswordField("Password", retVal);
                GUILayout.FlexibleSpace();
                using(new EditorGUILayout.HorizontalScope(EditorStyles.inspectorFullWidthMargins)) {
                    if(GUILayout.Button("Cancel")) {
                        retVal = null;
                        Close();
                    }
                    EditorGUILayout.Space();
                    if(GUILayout.Button("Ok")) {
                        Close();
                    }
                }
                EditorGUILayout.Space(32F);
            }
        }
    }
}