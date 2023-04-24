using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace IPTech.BuildTool
{
    using Encryption;
    using Internal;

    public class BuildToolEditor : EditorWindow {
        const string CREATE_NEW = "Create New";
        static BuildToolEditor instance;

        List<BuildConfig> buildConfigs;

        IReadOnlyList<Type> buildTypes;
        string[] buildTypeNames = new string[] { "generating..." };

        Dictionary<BuildConfig, Editor> editors = new Dictionary<BuildConfig, Editor>();
        GUIStyle leftAlignedButton;
        GUIStyle helpBoxStyle;

        IReadOnlyList<Type> importOptionsType = new List<Type>();
        string[] importOptions;
        
        [SerializeField] List<int> stateList;
        [SerializeField] bool isDirty;
        [SerializeField] int activeTab;
        [SerializeField] string password;
        [SerializeField] Vector2 buildConfigsScrollPos;
        [SerializeField] Vector2 settingsScrollPos;
        [SerializeField] Vector2 encryptedStorageScrollPos;

        bool needsRefresh;
        static bool isBuilding;

        SerializedObject buildToolSettingsSerializedObject;
        
        [MenuItem("Window/IPTech/Build Tool")]
        public static void ShowWindow() {
            instance = EditorWindow.GetWindow<BuildToolEditor>("Build Tool");
            instance.Show();
        }

        private void OnEnable() {
            buildToolSettingsSerializedObject = new SerializedObject(BuildToolsSettings.instance);
            if(stateList == null) stateList = new List<int>();
            
            ReloadConfigs();
            GenerateBuildConfigTypeDropDown();
            
            leftAlignedButton = new GUIStyle(EditorGUIUtility.GetBuiltinSkin(EditorSkin.Scene).button);
            leftAlignedButton.alignment = TextAnchor.MiddleLeft;
            leftAlignedButton.fixedHeight = 30F;

            helpBoxStyle = new GUIStyle(EditorGUIUtility.GetBuiltinSkin(EditorSkin.Scene).GetStyle("helpBox"));
            helpBoxStyle.alignment = leftAlignedButton.alignment;
            helpBoxStyle.fixedHeight = leftAlignedButton.fixedHeight;
            helpBoxStyle.fontSize = leftAlignedButton.fontSize;

            if(!string.IsNullOrWhiteSpace(password)) {
                try {
                    var es = BuildToolsSettings.instance.EncryptedStorage;
                    if(!es.IsUnlocked) {
                        es.Unlock(password);
                    }
                } catch {
                    password = null;
                }
            }
        }


        void ReloadConfigs() {
            ClearBuildConfigs();
            PopulateBuildConfigs();
            needsRefresh = false;

            void ClearBuildConfigs() {
                if(buildConfigs == null) {
                    buildConfigs = new List<BuildConfig>();
                } else {
                    buildConfigs.Clear();
                }
            }

            void PopulateBuildConfigs() {
                var bcguids = AssetDatabase.FindAssets("t:IPTech.BuildTool.BuildConfig");
                foreach(var guid in bcguids) {
                    buildConfigs.Add((BuildConfig)AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(guid)));
                }
            }
        }

        void GenerateBuildConfigTypeDropDown() {
            Context.ListGenerator.GetList(typeof(BuildConfig), (bcl) => {
                buildTypes = bcl;
                buildTypeNames = new string[buildTypes.Count + 1];
                buildTypeNames[0] = CREATE_NEW;  
                if(buildTypes.Count > 0) {
                    for(int i=0;i < buildTypes.Count;i++) {
                        buildTypeNames[i+1] = buildTypes[i].Name; 
                    }
                }
            });

            Context.ListGenerator.GetList(typeof(EncryptedItem), (ll) => {
                importOptionsType = ll;
                importOptions = new string[importOptionsType.Count + 1];
                importOptions[0] = "import";
                if(importOptionsType.Count>0) {
                    for(int i=0; i < importOptionsType.Count;i++) {
                        importOptions[i+1] = importOptionsType[i].Name;
                    }
                }
            });
        }

        

        private void OnGUI() {
            ConditionallyReloadConfigs();
            DrawSettingsTab();
            DrawBuildConfigsTab();
            DrawEncryptedStorageTab();
            

            void ConditionallyReloadConfigs() {
                if(Event.current.type == EventType.Layout) {
                    if(needsRefresh) {
                        ReloadConfigs();
                    }
                }
            }

            void DrawSettingsTab() {
                const string title = "Build Tool Settings";
                if(activeTab==0) {
                    EditorGUILayout.LabelField(title, helpBoxStyle);
                    //EditorGUILayout.HelpBox(title, MessageType.None);
                    using(var ss = new EditorGUILayout.ScrollViewScope(settingsScrollPos, GUILayout.ExpandHeight(true))) {
                        settingsScrollPos = ss.scrollPosition;

                        EditorGUI.BeginChangeCheck();
                        buildToolSettingsSerializedObject.Update();
                        var sp = buildToolSettingsSerializedObject.GetIterator();
                        sp.NextVisible(true);
                        do {
                            if(sp.propertyPath == "m_Script") continue;
                            if(sp.propertyType == SerializedPropertyType.Boolean) {
                                float origWidth = EditorGUIUtility.labelWidth;
                                EditorGUIUtility.labelWidth = 250;
                                EditorGUILayout.PropertyField(sp);
                                EditorGUIUtility.labelWidth = origWidth;
                            } else {
                                EditorGUILayout.PropertyField(sp);
                            }
                        } while(sp.NextVisible(false));

                        if(EditorGUI.EndChangeCheck()) {
                            buildToolSettingsSerializedObject.ApplyModifiedProperties();
                            BuildToolsSettings.instance.Save();
                        }
                    }
                } else {
                    if(GUILayout.Button(title, leftAlignedButton)) {
                        activeTab = 0;
                    }
                }
            }

            void DrawBuildConfigsTab() {
                const string title = "Build Configs";
                if(activeTab == 1) {
                    EditorGUILayout.LabelField(title, helpBoxStyle);

                    using(var sv = new EditorGUILayout.ScrollViewScope(buildConfigsScrollPos, GUILayout.ExpandHeight(true))) {
                        DrawBuildConfigs();
                        buildConfigsScrollPos = sv.scrollPosition;
                    }
                    EditorGUILayout.Separator();
                    DrawFooter();
                } else {
                    if(GUILayout.Button(title, leftAlignedButton)) {
                        activeTab = 1;
                    }
                }
            }

            void DrawBuildConfigs() {
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
                                    EditorGUIUtility.PingObject(bc.GetInstanceID());
                                }
                                var ed = GetEditor(bc);                               
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
                }
            }

            Editor GetEditor(BuildConfig bc) {
                if(editors.TryGetValue(bc, out Editor ed)) {
                    return ed;
                }

                var newEd = Editor.CreateEditor(bc);
                editors.Add(bc, newEd);
                return newEd;
            }

            void DrawFooter() {
                using(new EditorGUILayout.HorizontalScope()) {
                    GUILayout.FlexibleSpace();
                    int selectedBuildType = EditorGUILayout.Popup(0, buildTypeNames);
                    if(selectedBuildType > 0) {
                        var t = buildTypes[selectedBuildType - 1];
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

        void DrawEncryptedStorageTab() {
            const string title = "Encrypted Storage";
            if(activeTab == 2) {
                EditorGUILayout.LabelField(title, helpBoxStyle);

                using(var sv = new EditorGUILayout.ScrollViewScope(encryptedStorageScrollPos, GUILayout.ExpandHeight(true))) {
                    DrawEncryptedStorage();
                    encryptedStorageScrollPos = sv.scrollPosition;
                }
            } else {
                if(GUILayout.Button(title, leftAlignedButton)) {
                    activeTab = 2;
                }
            }
        }

        void DrawEncryptedStorage() {
            var es = BuildToolsSettings.instance.EncryptedStorage;

            using(new EditorGUILayout.HorizontalScope()) {
                if(!es.IsUnlocked) {
                    password = EditorGUILayout.PasswordField("password", password);
                    if(GUILayout.Button(es.HasPassword ? "unlock" : "create", GUILayout.Width(100))) {
                        es.Unlock(password);
                    }
                } else {
                    if(GUILayout.Button("lock")) {
                        es.Lock();
                        password = null;
                    }
                }

                GUILayout.Space(16);
                if(GUILayout.Button("! DESTROY !", GUILayout.Width(100))) {
                    if(EditorUtility.DisplayDialog("DESTROY ALL ENCRYPTED ITEMS", "Are you sure you want to destroy all encrypted storage?", "Yes", "No")) {
                        password = null;
                        es.DeleteAllStorageAndPassword();
                    }
                }
            }

            using(new EditorGUI.DisabledScope(!es.IsUnlocked)) {
                EditorGUI.indentLevel++;
                foreach(var key in es) {
                    using(new EditorGUILayout.HorizontalScope()) {
                        EditorGUILayout.LabelField(key.Name);
                        if(GUILayout.Button("edit", GUILayout.Width(100))) {
                            EncryptedStorageImportDialog.EditItem(key.Name);
                        }
                        GUILayout.Space(16);
                        if(GUILayout.Button("delete", GUILayout.Width(100))) {
                            es.Remove(key.Name);
                        }
                    }
                }
                using(new EditorGUILayout.HorizontalScope()) {
                    GUILayout.FlexibleSpace();
                    int i = EditorGUILayout.Popup(0, importOptions);
                    if(i>0) {
                        var t = importOptionsType[i-1];
                        EncryptedStorageImportDialog.ImportItem(t);
                    }
                }
            }
            EditorGUI.indentLevel--;
            
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

        public static void PerformBuild(BuildConfig bc) {
            try {
                isBuilding = true;
                string path = AssetDatabase.GetAssetPath(bc);
                GUID guid = AssetDatabase.GUIDFromAssetPath(path);
                AssetDatabase.SaveAssetIfDirty(guid);

                var argsWithBuildConfig = new List<string>(BuildToolsSettings.instance.BuildInEditorArguments);
                argsWithBuildConfig.Add("-buildConfig");
                argsWithBuildConfig.Add(bc.name);
                Builder.BuildWithArguments(argsWithBuildConfig.ToArray());
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
            BuildToolsSettings.instance.DefaultConfigPath = newRelativePath;
            BuildToolsSettings.instance.Save();
        }

        string GetBuildConfigFolderPath() {
            return Path.Combine("Assets", BuildToolsSettings.instance.DefaultConfigPath);
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
