using UnityEngine;
using UnityEditor;

namespace IPTech.BuildTool
{
    [CustomEditor(typeof(ConfigModifier), true)]
    public class ConfigModifierEditor : Editor {
        public override void OnInspectorGUI() {
            serializedObject.UpdateIfRequiredOrScript();
            var sp = serializedObject.GetIterator();

            bool enterChildren = true;
            while(sp.NextVisible(enterChildren)) {
                enterChildren = false;
                if(sp.propertyPath != "m_Script") {
                    EditorGUILayout.PropertyField(sp, new GUIContent(sp.displayName));
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
