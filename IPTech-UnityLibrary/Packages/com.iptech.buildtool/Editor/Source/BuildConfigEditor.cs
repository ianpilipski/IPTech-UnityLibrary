using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace IPTech.BuildTool {
    public class BuildConfigEditor : EditorWindow {
        const string CREATE_NEW = "Create New";
        static BuildConfigEditor instance;

        List<BuildConfig> buildConfigs;
        List<Type> buildTypes;
        int selectedBuildType;
        string[] buildTypeNames;

        [SerializeField]
        List<int> stateList;
        bool needsRefresh;

        [MenuItem("Window/IPTech/Build Tool")]
        public static void ShowWindow() {
            instance = EditorWindow.GetWindow<BuildConfigEditor>("Build Tool");
            instance.Show();
        }

        private void OnEnable() {
            if(stateList == null) stateList = new List<int>();

            buildTypes = new List<Type>();
            ReloadConfigs();
            GenerateBuildConfigTypeDropDown();
        }


        void ReloadConfigs() {
            var bcs = Resources.FindObjectsOfTypeAll<BuildConfig>();
            buildConfigs = bcs.ToList();
            needsRefresh = false;
        }

        void GenerateBuildConfigTypeDropDown() {
            buildTypes.Clear();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach(var asm in assemblies) {
                var types = asm.GetTypes();
                foreach(var t in types) {
                    if(!t.IsAbstract && t.IsSubclassOf(typeof(BuildConfig))) {
                        buildTypes.Add(t);
                    }
                }
            }

            if(buildTypes.Count > 0) {
                buildTypeNames = new string[buildTypes.Count + 1];
                buildTypeNames[0] = CREATE_NEW;
                for(int i=0;i < buildTypes.Count;i++) {
                    buildTypeNames[i+1] = buildTypes[i].Name; 
                }
            } else {
                buildTypeNames = new string[1] { CREATE_NEW };
            }
        }

        private void OnGUI() {
            if(Event.current.type == EventType.Layout) {
                if(needsRefresh) {
                    ReloadConfigs();
                }
            }

            EditorGUILayout.LabelField("Build Tool Settings");

            EditorGUI.BeginChangeCheck();
            BuildToolsSettings.Inst.AddGradleWrapper = EditorGUILayout.Toggle("Add Gradle Wrapper To Unity Builds", BuildToolsSettings.Inst.AddGradleWrapper);
            BuildToolsSettings.Inst.DefaultConfigPath = EditorGUILayout.TextField("Default Config Path", BuildToolsSettings.Inst.DefaultConfigPath);
            if(EditorGUI.EndChangeCheck()) {
                BuildToolsSettings.Save();
            }

            EditorGUILayout.Separator();

            EditorGUILayout.LabelField("Build Configs");
            using(new EditorGUI.IndentLevelScope()) {
                foreach(var bc in buildConfigs) {
                    if(bc == null) {
                        needsRefresh = true;
                        continue;
                    }
                    bool isExpanded = stateList.Contains(bc.GetInstanceID());
                    
                    var newExpanded = EditorGUILayout.Foldout(isExpanded, bc.name);
                    if(newExpanded != isExpanded) {
                        if(isExpanded) {
                            stateList.Remove(bc.GetInstanceID());
                        } else {
                            stateList.Add(bc.GetInstanceID());
                        }
                        isExpanded = newExpanded;
                    }

                    if(isExpanded) {
                        using(new EditorGUI.IndentLevelScope()) {
                            var rect = EditorGUILayout.GetControlRect();
                            var offset = (EditorGUI.indentLevel * 15F);
                            rect.x = rect.x + offset;
                            rect.width = rect.width - offset;
                            if(GUI.Button(rect, AssetDatabase.GetAssetPath(bc.GetInstanceID()))) {
                                //Selection.objects = new UnityEngine.Object[] { bc };
                                EditorGUIUtility.PingObject(bc.GetInstanceID());
                            }
                            var ed = Editor.CreateEditor(bc);
                            ed.OnInspectorGUI();
                        }
                    }
                }

                using(new EditorGUILayout.HorizontalScope()) {
                    GUILayout.FlexibleSpace();
                    selectedBuildType = EditorGUILayout.Popup(selectedBuildType, buildTypeNames);
                    if(selectedBuildType > 0) {
                        var t = buildTypes[selectedBuildType - 1];
                        selectedBuildType = 0;
                        CreateNewBuildType(t);
                    }
                }
            }
        }

        void CreateNewBuildType(Type type) {
            BuildConfig newConfig = (BuildConfig)ScriptableObject.CreateInstance(type.FullName);
            AssetDatabase.CreateAsset(newConfig, GetNewBuildConfigAssetPath());
            AssetDatabase.SaveAssets();
            EditorApplication.delayCall += () => {
                ReloadConfigs();
            };
        }

        string GetNewBuildConfigAssetPath() {
            int counter = 0;
            string newName = "BuildConfig.asset";
            while(File.Exists(Path.Combine(Application.dataPath, BuildToolsSettings.Inst.DefaultConfigPath, newName))) {
                counter++;
                newName = $"BuildConfig_{counter}";
            }
            return Path.Combine("Assets", BuildToolsSettings.Inst.DefaultConfigPath, newName);
        }

    }
}
