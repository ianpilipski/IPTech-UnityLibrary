using UnityEngine;
using UnityEditor;
using UObject = UnityEngine.Object;
using System;

namespace IPTech.BuildTool {
    using Encryption;
    
    public class EncryptedStorageImportDialog : EditorWindow {
        SerializedObject serializedObject;
        EncryptedItem target;

        bool nameLocked;

        public static void EditItem(string key) {
            if(CheckStorageUnlocked()) {
                if(BuildToolsSettings.instance.EncryptedStorage.TryGetDecryptedValue(key, out EncryptedItem value)) {
                    var window = ScriptableObject.CreateInstance<EncryptedStorageImportDialog>();
                    window.ShowWindowInternal(value, true);
                } else {
                    Debug.LogError("could not get encrypted item");
                }
            }
        }

        public static void ImportItem(string fullTypeName) {
            var t = Type.GetType(fullTypeName);
            if(t!=null) {
                ImportItem(t);
            } else {
                Debug.LogError("could not find type " + fullTypeName);
            }
        }

        public static void ImportItem(Type type) {
            if(CheckStorageUnlocked()) {
                if(type.IsSubclassOf(typeof(EncryptedItem))) {
                    var window = ScriptableObject.CreateInstance<EncryptedStorageImportDialog>();
                    window.ShowWindowInternal((EncryptedItem)ScriptableObject.CreateInstance(type), false);
                } else {
                    throw new InvalidOperationException($"the type {type.FullName} was not a subclass of EncryptedItem");
                }
            }
        }

        static bool CheckStorageUnlocked() {
            if(!BuildToolsSettings.instance.EncryptedStorage.IsUnlocked) {
                EditorUtility.DisplayDialog("Unlock Encrypted Storage", "You must unlock the encrypted storage to import and item", "ok");
                return false;
            }
            return true;
        }

        void ShowWindowInternal(EncryptedItem item, bool nameLocked) {
            SetTarget(item);
            this.nameLocked = nameLocked;
            ShowModalUtility();

            CenterOnMainWin();
        }

        void CenterOnMainWin()
        {
            Rect main = EditorGUIUtility.GetMainWindowPosition();
            Rect pos = this.position;
            float centerWidth = (main.width - pos.width) * 0.5f;
            float centerHeight = (main.height - pos.height) * 0.25f;
            pos.x = main.x + centerWidth;
            pos.y = main.y + centerHeight;
            this.position = pos;
        }

        void SetTarget(EncryptedItem target) {
            this.target = target;
            this.serializedObject = new SerializedObject(target);
            titleContent = new GUIContent("Import " + target.GetType().Name);
        }

        void OnEnable() {
            if(target!=null) {
                // happens during script reload
                if(serializedObject==null) {
                    serializedObject = new SerializedObject(target);
                }
            }
        }
    
        private void OnGUI() {
            HandleEscape();
            
            serializedObject.Update();
            
            GUIStyle marginStyle = UnityEditor.EditorStyles.inspectorFullWidthMargins;
            EditorGUILayout.BeginVertical(marginStyle);

            var nameProp = serializedObject.FindProperty("m_Name");
            using(new EditorGUI.DisabledScope(nameLocked)) {
                EditorGUILayout.PropertyField(nameProp);
            }

            using(var cc = new EditorGUI.ChangeCheckScope()) {
                bool allowNameHintUsage = !nameLocked && string.IsNullOrEmpty(nameProp.stringValue);

                var p = serializedObject.GetIterator();
                bool enterChildren = true;
                while(p.NextVisible(enterChildren)) {
                    enterChildren=false;
                    if(p.propertyPath!="m_Script") {
                        EditorGUILayout.PropertyField(p);
                    }
                }

                if(cc.changed && allowNameHintUsage) {
                    string pp = target.NameHintPropertyPath;
                    if(!string.IsNullOrEmpty(pp)) {
                        var prop = serializedObject.FindProperty(target.NameHintPropertyPath);
                        if(prop!=null && prop.stringValue!=null) {
                            nameProp.stringValue = prop.stringValue;
                        }
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();

            GUILayout.FlexibleSpace();
            EditorGUILayout.Space();

            using(new EditorGUILayout.HorizontalScope()) {
                GUILayout.FlexibleSpace();
                if(GUILayout.Button("Cancel", GUILayout.Width(100), GUILayout.Height(32))) {
                    Close();
                }
                EditorGUILayout.Space();
                if(GUILayout.Button("Import", GUILayout.Width(100), GUILayout.Height(32))) {
                    if(string.IsNullOrWhiteSpace(target.name)) {
                        EditorUtility.DisplayDialog("key invalid","you must enter a name", "ok");
                    } else {
                        if(!nameLocked && BuildToolsSettings.instance.EncryptedStorage.ContainsKey(target.name)) {
                            EditorUtility.DisplayDialog("key invalid", "that key already exists in the encrypted storage, delete it first to re-import it", "ok");
                        } else {
                            if(nameLocked) {
                                BuildToolsSettings.instance.EncryptedStorage.Remove(target.name);
                            }
                            BuildToolsSettings.instance.EncryptedStorage.Add(target.name, target);
                            Close();
                        }
                    }
                }
                EditorGUILayout.Space();
                EditorGUILayout.Space();
            }
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.EndVertical();
        }

        void HandleEscape() {
            Event e = Event.current;
            if(e.type == EventType.KeyDown) {
                if(e.keyCode == KeyCode.Escape) {
                    Close();
                    e.Use();
                }
            }
        }
    }
}