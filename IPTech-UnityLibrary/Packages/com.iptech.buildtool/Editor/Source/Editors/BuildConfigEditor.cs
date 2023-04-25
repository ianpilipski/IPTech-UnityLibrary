using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace IPTech.BuildTool
{
    using Internal;

    [CustomEditor(typeof(BuildConfig), true)]
    public class BuildConfigEditor : Editor {
        Dictionary<int, Editor> cachedEditors = new Dictionary<int, Editor>();
        
        bool isCreatingOptions;
        IReadOnlyList<Type> createOptionsTypes;
        string[] createOptions = { "select your poison" };

        List<ConfigModifier> subAssets;
        

        protected void OnEnable() {
            RefreshSubAssets();
            UpdateCreateOptions();
        }

        void UpdateCreateOptions() {
            if(createOptionsTypes==null && !isCreatingOptions) {
                isCreatingOptions = true;

                Context.ListGenerator.GetList(typeof(ConfigModifier), (ll) => {
                    createOptionsTypes = ll;
                    createOptions = new string[createOptionsTypes.Count + 1];
                    createOptions[0] = "select your poison";
                    for(int i=0;i<createOptionsTypes.Count;i++) {
                        var n = createOptionsTypes[i].Name;
                        createOptions[i+1] = n.StartsWith("ConfigModifier") ? n.Substring("ConfigModifier".Length) : n; 
                    }
                });
            }
        }

        void RefreshSubAssets() {
            subAssets = ((BuildConfig)target).LoadConfigModifiers().ToList();
        }

        bool TryGetEditor(ConfigModifier obj, int index, out Editor editor) {
            if(cachedEditors.TryGetValue(index, out Editor foundEditor)) {
                if(obj != null) {
                    Editor.CreateCachedEditor(obj, null, ref foundEditor);
                    cachedEditors[index] = foundEditor;
                    editor = foundEditor;
                    return true;
                } else {
                    DestroyImmediate(foundEditor);
                    cachedEditors.Remove(index);
                    editor = null;
                    return false;
                }
            }

            if(obj!=null) {
                cachedEditors[index] = Editor.CreateEditor(obj);
                editor = cachedEditors[index];
                return true;
            }

            editor = null;
            return false;

            
        }


        public override void OnInspectorGUI() {
            UpdateCreateOptions();
            
            //using(var ss = new EditorGUILayout.ScrollViewScope(scrollPos)) {
            //    scrollPos = ss.scrollPosition;

                serializedObject.UpdateIfRequiredOrScript();
                var sp = serializedObject.GetIterator();

                bool enterChildren = true;
                while(sp.NextVisible(enterChildren)) {
                    enterChildren = false;
                    if(sp.propertyPath != "m_Script") {
                        if(sp.propertyPath == nameof(PlayerBuildConfig.BuildProcessors)) {
                            EditorGUILayout.PropertyField(sp, true);
                        } else {
                            EditorGUILayout.PropertyField(sp, false);
                        }
                    }
                }

                DrawSubAssets();

                serializedObject.ApplyModifiedProperties();
            //}
        }

        protected void DrawSubAssets() {
            EditorGUILayout.LabelField("Config Modifiers");
            using(new EditorGUI.IndentLevelScope()) {
                int delIndex = -1;
                for(int i = 0; i < subAssets.Count; i++) {
                    using(new EditorGUILayout.HorizontalScope()) {
                        using(var s = new EditorGUILayout.VerticalScope()) {
                            var obj = subAssets[i];
                            if(TryGetEditor(obj, i, out Editor ed)) {
                                ed.OnInspectorGUI();
                            }
                        }
                        if(GUILayout.Button("-", GUILayout.Width(30))) {
                            delIndex = i;
                        }
                    }
                }
                if(delIndex > -1) {
                    DeleteItemAtIndex(delIndex);
                }

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                DrawAddModifierButton();

                EditorGUILayout.EndHorizontal();
            }
        }

        void DrawAddModifierButton() {
            var selected = EditorGUILayout.Popup( 0, createOptions);
            if(selected > 0) {
                var t = createOptionsTypes[selected - 1];
                AddItem(t);
            }
        }

        void AddItem(Type t) {
            var targetObj = target;
            var path = AssetDatabase.GetAssetPath(targetObj);
            if(!string.IsNullOrEmpty(path)) {
                var guid = AssetDatabase.GUIDFromAssetPath(path);

                var newProp = ScriptableObject.CreateInstance(t);
                newProp.name = t.Name;
                AssetDatabase.AddObjectToAsset(newProp, path);
                Undo.RegisterCreatedObjectUndo(newProp, "undo config modifier creation");

                //EditorUtility.SetDirty(target);
                //AssetDatabase.SaveAssetIfDirty(AssetDatabase.GUIDFromAssetPath(path));
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                RefreshSubAssets();
            }
        }

        void DeleteItemAtIndex(int delIndex) {
            Undo.DestroyObjectImmediate(subAssets[delIndex]);
            subAssets.RemoveAt(delIndex);
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(target));
        }
    }
}
