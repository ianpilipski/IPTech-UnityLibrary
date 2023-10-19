using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace IPTech.BuildTool.Editors {
    public class PasswordInputDialog : EditorWindow {
        string retVal;
        int passControlId;

        bool didFocusControl;

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
                HandleEscapeReturn();

                //passControlId = GUIUtility.GetControlID(FocusType.Keyboard);
                GUI.SetNextControlName("pass");
                retVal = EditorGUILayout.PasswordField("Password", retVal);
                if(!didFocusControl && Event.current.type == EventType.Layout) {
                    EditorGUI.FocusTextInControl("pass");
                    didFocusControl = true;
                }

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

        void HandleEscapeReturn() {
            Event e = Event.current;
            if(e.type == EventType.KeyDown) {
                if(GUIUtility.hotControl == passControlId) {
                    if(e.keyCode == KeyCode.Return) {
                        Close();
                        e.Use();
                    }
                }

                if(e.keyCode == KeyCode.Escape) {
                    retVal = null;
                    Close();
                    e.Use();
                }
            }
        }
    }
}