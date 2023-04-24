using UnityEngine;
using UnityEditor;
using System.Linq;
using System;

namespace IPTech.BuildTool.Encryption
{
    [CustomPropertyDrawer(typeof(EncryptedItemInfo), true)]
    public class EncryptedItemInfoPropertyDrawer : PropertyDrawer {
        string[] keys;
        
        DateTime lastModified;
        

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            PopulateKeys(property);

            var nameProp = property.FindPropertyRelative("Name");
            var fullTypeNameProp = property.FindPropertyRelative("FullTypeName");
            
            using(var pp = new EditorGUI.PropertyScope(position, label, property)) {
                using(var cc = new EditorGUI.ChangeCheckScope()) {
                    var pos = position;
                    pos.width -= 102;
                    pos.height = 18F;
                    pos.y += 1;

                    var lName = new GUIContent(!string.IsNullOrEmpty(nameProp.stringValue) ? 
                        $"{nameProp.stringValue} : {fullTypeNameProp.stringValue}" :
                        $"(none) : {fullTypeNameProp.stringValue}");

                    EditorGUI.LabelField(pos, label, lName);

                    pos.x += (pos.width + 1);
                    pos.width = 100;
                    var i = EditorGUI.Popup(pos, 0, keys);
                    if(i!=0) {
                        if(i==1) {
                            nameProp.stringValue = null;
                        } else if(i==keys.Length-1) {
                            if(!string.IsNullOrEmpty(fullTypeNameProp.stringValue)) {
                                EncryptedStorageImportDialog.ImportItem(fullTypeNameProp.stringValue);
                            } else {
                                EditorUtility.DisplayDialog("Generic Reference", "Generic References to encrypted items must be imported in the settings.", "ok");
                            }
                        } else {
                            nameProp.stringValue = keys[i];
                        }
                    }
                }
            }
        }

        void PopulateKeys(SerializedProperty prop) {
            var es = BuildToolsSettings.instance.EncryptedStorage;
            if(keys == null || es.LastModified!=lastModified) {
                lastModified = es.LastModified;

                var fullTypeNameProp = prop.FindPropertyRelative("FullTypeName");
                var typeName = fullTypeNameProp.stringValue;

                var filteredTypes = es.Where(i => string.IsNullOrWhiteSpace(typeName) || i.FullTypeName == typeName);
                keys = new string[] { "select", "(none)" }.Concat(filteredTypes.Select(k => k.Name)).Concat(new string[] { "(import)" }).ToArray();
            }
        }

       
    }
}
