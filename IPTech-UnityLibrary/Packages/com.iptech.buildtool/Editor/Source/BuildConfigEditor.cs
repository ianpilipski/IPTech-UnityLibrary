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

        [SerializeField] List<int> stateList;
        [SerializeField] bool isDirty;
        [SerializeField] string buildArgments;

        bool needsRefresh;
        bool isBuilding;

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
            for(int i=buildConfigs.Count-1;i>=0;i--) {
                var bc = buildConfigs[i];
                if(!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(bc.GetInstanceID(), out string _, out long __)) {
                    buildConfigs.RemoveAt(i);
                }
            }
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
            BuildToolsSettings.Inst.BuildInEditorArguments = EditorGUILayout.TextField("Build in editor arguments", BuildToolsSettings.Inst.BuildInEditorArguments);

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

                    bool newExpanded = isExpanded;
                    using(new EditorGUILayout.HorizontalScope()) {
                        newExpanded = EditorGUILayout.Foldout(isExpanded, bc.name);
                        if(newExpanded != isExpanded) {
                            if(isExpanded) {
                                stateList.Remove(bc.GetInstanceID());
                            } else {
                                stateList.Add(bc.GetInstanceID());
                            }
                            isExpanded = newExpanded;
                        }
                        if(!isExpanded) {
                            DrawBuildButton(bc);
                            DrawDeleteBuildConfigButton(bc);
                        }
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
                            isDirty = isDirty || EditorUtility.IsDirty(bc.GetInstanceID());
                            using(new EditorGUILayout.HorizontalScope()) {
                                GUILayout.FlexibleSpace();
                                DrawBuildButton(bc);
                                DrawDeleteBuildConfigButton(bc);
                            }
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
                    using(new EditorGUI.DisabledScope(!isDirty)) {
                        if(GUILayout.Button("Save")) {
                            foreach(var bc in buildConfigs) {
                                if(AssetDatabase.TryGetGUIDAndLocalFileIdentifier(bc.GetInstanceID(), out string guid, out long _)) {
                                    AssetDatabase.SaveAssetIfDirty(new GUID(guid));
                                }
                            }
                            isDirty = false;
                        }
                    }
                }
            }
        }

        void DrawDeleteBuildConfigButton(BuildConfig bc) {
            if(GUILayout.Button("delete", GUILayout.Width(75))) {
                DeleteBuildConfig(bc);
            }
        }

        void DeleteBuildConfig(BuildConfig bc) {
            EditorApplication.delayCall += () => {
                string path = AssetDatabase.GetAssetPath(bc);
                AssetDatabase.DeleteAsset(path);
                buildConfigs.Remove(bc);
            };
        }

        void DrawBuildButton(BuildConfig bc) {
            using(new EditorGUI.DisabledScope(isBuilding || !bc.CanBuildWithCurrentEditorBuildTarget())) {
                if(GUILayout.Button("build", GUILayout.Width(75))) {
                    isBuilding = true;
                    EditorApplication.delayCall += () => PerformBuild(bc);
                }
            }
        }

        void PerformBuild(BuildConfig bc) {
            try {
                string path = AssetDatabase.GetAssetPath(bc);
                GUID guid = AssetDatabase.GUIDFromAssetPath(path);
                AssetDatabase.SaveAssetIfDirty(guid);

                string args = $"-buildConfig {bc.name} {buildArgments}";
                Builder.BuildWithArguments(args);
            } catch(Exception e) {
                Debug.LogException(e);
                EditorUtility.DisplayDialog("Error Building", e.Message, "Ok");
            } finally {
                isBuilding = false;
            }
        }

        void CreateNewBuildType(Type type) {
            string name = EditorUtility.SaveFilePanel("Config Name", Path.Combine("Assets", GetBuildConfigFolderPath()), "NewBuildConfig", ".asset");
            if(!string.IsNullOrEmpty(name)) {
                if(TryGetAssetRelativeBuildConfigPath(name, out string destFolder)) {
                    destFolder = Path.Combine("Assets", destFolder);
                    var fileName = GetSanitizedFileName(name);
                    if(!File.Exists(Path.Combine(destFolder, fileName))) {
                        UpdateDefaultBuildConfigPath(destFolder);

                        BuildConfig newConfig = (BuildConfig)ScriptableObject.CreateInstance(type.FullName);
                        if(!Directory.Exists(destFolder)) {
                            Directory.CreateDirectory(destFolder);
                        }
                        AssetDatabase.CreateAsset(newConfig, Path.Combine(destFolder, fileName + ".asset"));
                        AssetDatabase.SaveAssets();
                        EditorApplication.delayCall += () => {
                            ReloadConfigs();
                        };
                    } else {
                        EditorUtility.DisplayDialog("File Already Exists", "That file already exists, you must supply a unique file name.", "Ok");
                    }
                } else {
                    EditorUtility.DisplayDialog("Invalid Path", "You must supply a path within the unity project assets folder.", "Ok");
                }
            }
        }

        bool TryGetAssetRelativeBuildConfigPath(string newFullPath, out string relativePath) {
            string destFolder = Path.GetDirectoryName(newFullPath);
            var dataPath = Path.GetFullPath(Application.dataPath);
            if(destFolder.StartsWith(dataPath)) {
                if(destFolder.Length > dataPath.Length) {
                    relativePath = destFolder.Substring(dataPath.Length + 1);
                } else {
                    relativePath = "";
                }
                return true;
            }
            relativePath = null;
            return false;
        }

        void UpdateDefaultBuildConfigPath(string newRelativePath) {
            BuildToolsSettings.Inst.DefaultConfigPath = newRelativePath;
            BuildToolsSettings.Save();
        }

        string GetBuildConfigFolderPath() {
            return Path.Combine("Assets", BuildToolsSettings.Inst.DefaultConfigPath);
        }

        string GetSanitizedFileName(string newFilePath) {
            var fileName = Path.GetFileName(newFilePath);
            if(fileName.ToUpper().EndsWith(".ASSET")) {
                fileName = fileName.Substring(0, fileName.Length - ".asset".Length);
            }
            return fileName;
        }
    }
}
