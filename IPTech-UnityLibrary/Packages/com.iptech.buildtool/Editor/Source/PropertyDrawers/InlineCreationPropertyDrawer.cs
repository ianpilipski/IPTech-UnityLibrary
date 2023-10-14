using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

namespace IPTech.BuildTool
{
    [CustomPropertyDrawer(typeof(InlineCreationAttribute))]
    public class AllowRefOrInstPropertyDrawer : PropertyDrawer {
        const float HEIGHT = 18F;
        const float PROP_MARGIN = 2F;
        const float NEXT_PROP_HEIGHT = HEIGHT + PROP_MARGIN;

        static string[] EmptyCreateList = new string[1] { "create" };

        Type createListType;
        string[] createList;
        IReadOnlyList<Type> createTypes;
        
        Dictionary<string, float> propertyHeights;
        SerializedObject soTarget;
        float propHeight = 0F;
        bool shouldDrawItem;
        Type propType;
        string toolTipAttributeText;
        float toolTipHeight;

        public AllowRefOrInstPropertyDrawer() {
            propertyHeights = new Dictionary<string, float>();
        }
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            UpdatePropertyType(property);
            return base.GetPropertyHeight(property, label) + propHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            UpdatePropertyType(property);

            var pos = position;
            
            if(propType!=null) {
                var origObjectReference = property.objectReferenceValue;
                
                using(var ps = new EditorGUI.PropertyScope(pos, label, property)) {
                    
                    const int buttonWidth = 100;
                    pos.width -= buttonWidth + 4;
                    pos.height = HEIGHT;
                    if(origObjectReference != null) {
                        //label = new GUIContent(origObjectReference.name);
                    }
                    EditorGUI.PropertyField(pos, property, ps.content);

                
                    pos = new Rect(pos.x + (position.width - buttonWidth), pos.y, buttonWidth, pos.height);
                    int val = EditorGUI.Popup(pos, 0, createList);

                    if(val > 0) {
                        val--;
                        var t = createTypes[val];
                        if(t.IsSubclassOf(typeof(ScriptableObject))) {
                            property.objectReferenceValue = CreateObject(property, t);
                            
                        } else {
                            property.objectReferenceValue = Activator.CreateInstance(t) as UnityEngine.Object;
                        }
                    }
                }
                
                if(shouldDrawItem && soTarget!=null) {
                    EditorGUI.indentLevel++;

                    pos = position;
                    pos.height -= NEXT_PROP_HEIGHT;
                    pos.y += NEXT_PROP_HEIGHT;
                    pos.x += 16F;
                    pos.width -= 16F;

                    EditorGUI.HelpBox(pos, "", MessageType.None);

                    soTarget.Update();
                    var p = soTarget.GetIterator();
                    EditorGUI.BeginProperty(pos, new GUIContent(p.displayName), p);
                    pos.height = HEIGHT;
                    p.NextVisible(true);
                    float lastProperyHeight = 0F;
                    do {
                        if(p.propertyPath == "m_Script") {
                            if(!string.IsNullOrEmpty(toolTipAttributeText)) {
                                pos.y = pos.y + lastProperyHeight;
                                lastProperyHeight = toolTipHeight + PROP_MARGIN;
                                pos.height = lastProperyHeight - PROP_MARGIN;
                                EditorGUI.HelpBox(pos, toolTipAttributeText, MessageType.Info);
                            }
                        } else { 
                            pos.y = pos.y + lastProperyHeight;
                            lastProperyHeight = propertyHeights[p.propertyPath] + PROP_MARGIN;
                            pos.height = lastProperyHeight - PROP_MARGIN;
                            EditorGUI.PropertyField(pos, p);
                        }
                    } while(p.NextVisible(false));
                    EditorGUI.EndProperty();
                    soTarget.ApplyModifiedProperties();

                    EditorGUI.indentLevel--;
                }
            } else {
                EditorGUI.LabelField(position, "the property type is not supported for this property drawer");
            }
        }

        void EnsureCreateListIsPopulated() {
            if(createList == null || propType != createListType) {
                createListType = propType;
                createList = EmptyCreateList;

                var scopedType = createListType;
                Internal.Context.ListGenerator.GetList(createListType, (ll) => {
                    if(scopedType == createListType) {
                        createTypes = ll;
                        createList = EmptyCreateList.Concat(createTypes.Select(t => t.Name)).ToArray();
                    }
                });
            }
        }

        bool UpdatePropertyType(SerializedProperty prop) {
            if(propType == null) {
                var t = fieldInfo.FieldType;
                if(t.IsArray) {
                    propType = t.GetElementType();
                } else if(t.IsGenericType) {
                    var gtd = t.GetGenericTypeDefinition();
                    if(gtd == typeof(List<>)) {
                        var gts = t.GetGenericArguments();
                        propType = gts[0];
                    }
                } else {
                    if(t.IsSubclassOf(typeof(UnityEngine.Object))) {
                        propType = t;
                    }
                }
                EnsureCreateListIsPopulated();
            }
            
            shouldDrawItem = !IsPropertyInst();
            if(shouldDrawItem && prop.objectReferenceValue != null) {
                if(UpdateScriptableObjectReference()) {
                    CalculateAdditionalPropertyHeights();
                }
            } else {
                ClearScriptableObjectReference();
            }
            return propType != null;

            bool UpdateScriptableObjectReference() {
                if(soTarget == null || soTarget.targetObject != prop.objectReferenceValue) {
                    soTarget = new SerializedObject(prop.objectReferenceValue);
                    return true;
                }
                return soTarget.UpdateIfRequiredOrScript();
            }

            void ClearScriptableObjectReference() {
                soTarget = null;
                toolTipAttributeText = null;
                propHeight = 0F;
                propertyHeights.Clear();
            }

            bool IsPropertyInst() {
                if(prop.objectReferenceValue != null) {
                    return AssetDatabase.Contains(prop.objectReferenceValue) && !AssetDatabase.IsSubAsset(prop.objectReferenceValue);
                }
                return false;
            }

            void CalculateAdditionalPropertyHeights() {
                propHeight = 0;
                var p = soTarget.GetIterator();
                p.NextVisible(true);
                do {
                    if(p.propertyPath != "m_Script") {
                        var h = EditorGUI.GetPropertyHeight(p);
                        propertyHeights[p.propertyPath] = h;
                        propHeight += h + PROP_MARGIN;
                    }
                } while(p.NextVisible(false));

                toolTipAttributeText = null;
                var attribs = soTarget.targetObject.GetType().GetCustomAttributes(false);
                if(attribs != null) {
                    foreach(var attrib in attribs) {
                        if(attrib is TooltipAttribute tt) {
                            toolTipAttributeText = tt.tooltip;
                            var hss = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Scene).GetStyle("helpBox");
                            toolTipHeight = hss.CalcHeight(new GUIContent(toolTipAttributeText), Mathf.Max(EditorGUIUtility.currentViewWidth - 100, 100));
                            propHeight += toolTipHeight + PROP_MARGIN;
                            break;
                        }
                    }
                }
            }
        }

        UnityEngine.Object CreateObject(SerializedProperty prop, Type t) {
            var obj = ScriptableObject.CreateInstance(t);
            obj.name = t.Name;
            var targetObj = prop.serializedObject.targetObject;
            if(AssetDatabase.Contains(targetObj)) {
                AssetDatabase.AddObjectToAsset(obj, targetObj);
                EditorApplication.delayCall += () => {
                    AssetDatabase.SaveAssetIfDirty(targetObj);
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(targetObj));
                };
            }
            return obj;
        }
    }
}
