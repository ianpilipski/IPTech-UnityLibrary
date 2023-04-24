using UnityEngine;
using UnityEditor;
using System.IO;

namespace IPTech.BuildTool {
    [CustomPropertyDrawer(typeof(FileData))]
    public class FileDataPropertyDrawer: PropertyDrawer {
        static string lastFilePath;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            SerializedProperty dataProp = property.FindPropertyRelative(nameof(FileData.data));
            SerializedProperty fileNameProp = property.FindPropertyRelative(nameof(FileData.fileName));

            using(var pp =  new EditorGUI.PropertyScope(position, label, property)) {
                using(var cc = new EditorGUI.ChangeCheckScope()) {

                    var pos = position;
                    pos.y += 1;
                    pos.height -= 2;
                    pos.width = pos.width - (100 + 25 + 2);
                    
                    EditorGUI.LabelField(pos, label, new GUIContent(fileNameProp.stringValue ?? ""));

                    pos.x += pos.width + 1;
                    pos.width = 100;

                    using(new EditorGUI.DisabledGroupScope(dataProp.arraySize==0)) {
                        if(GUI.Button(pos, "export")) {
                            ExportFileData(dataProp);
                        }
                    }

                    pos.x += 100 + 1;
                    pos.width = 25;
                    if(GUI.Button(pos, "...")) {
                        ImportFileData(dataProp, fileNameProp);
                    }

                    if(cc.changed) {

                    }
                }
            }
        }

        void ExportFileData(SerializedProperty data) {
            if(TryGetExportFilePath(out string filePath)) {
                SetLastFilePath(filePath);
                byte[] fileData = new byte[data.arraySize];
                for(int i=0;i<data.arraySize;i++) {
                    var el = data.GetArrayElementAtIndex(i);
                    fileData[i] = (byte)el.intValue;
                }
                File.WriteAllBytes(filePath, fileData);
            }
        }

        bool TryGetExportFilePath(out string filePath) {
            string extension = GetExtension();
            filePath = EditorUtility.SaveFilePanel("Export File", GetLastFilePath(), "filedata", extension);
            return !string.IsNullOrEmpty(filePath);
        }

        void ImportFileData(SerializedProperty data, SerializedProperty fileName) {
            if(TryGetImportFilePath(out string filePath)) {
                SetLastFilePath(filePath);
                byte[] fileData = File.ReadAllBytes(filePath);
                data.arraySize = fileData.Length;
                for(int i=0;i<fileData.Length;i++) {
                    var el = data.GetArrayElementAtIndex(i);
                    el.intValue = fileData[i];
                }
                fileName.stringValue = Path.GetFileName(filePath);
            }
        }

        bool TryGetImportFilePath(out string filePath) {
            string extension = GetExtension();
            filePath = EditorUtility.OpenFilePanel("Import File", GetLastFilePath(), extension);
            return !string.IsNullOrEmpty(filePath);
        }

        string GetExtension() {
            object[] ftas = this.fieldInfo.GetCustomAttributes(typeof(FileTypeAttribute), true);
            if(ftas.Length>0) {
                return ((FileTypeAttribute)ftas[0]).Extension;
            }
            return null;
        }

        static string GetLastFilePath() {
            if(string.IsNullOrEmpty(lastFilePath)) {
                return Path.Combine(Application.dataPath, "..");
            }
            return lastFilePath;
        }

        static void SetLastFilePath(string filePath) {
            lastFilePath = Path.GetDirectoryName(filePath);
        }
    }
}