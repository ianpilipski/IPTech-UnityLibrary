using UnityEditor;

namespace IPTech.BuildTool
{
    [CustomEditor(typeof(BuildConfig), true)]
    public class BuildConfigEditor : Editor {
        public override void OnInspectorGUI() {
            //using(var ss = new EditorGUILayout.ScrollViewScope(scrollPos)) {
            //    scrollPos = ss.scrollPosition;

                serializedObject.UpdateIfRequiredOrScript();
                var sp = serializedObject.GetIterator();

                bool enterChildren = true;
                while(sp.NextVisible(enterChildren)) {
                    enterChildren = false;
                    if(sp.propertyPath != "m_Script") {
                        EditorGUILayout.PropertyField(sp, ShouldPropertyBeExpanded(sp.propertyPath));
                    }
                }

                serializedObject.ApplyModifiedProperties();
            //}

            bool ShouldPropertyBeExpanded(string propPath) {
                return propPath == nameof(PlayerBuildConfig.BuildProcessors) ||
                    propPath == nameof(PlayerBuildConfig.ConfigModifiers);
            }
        }
    }
}
