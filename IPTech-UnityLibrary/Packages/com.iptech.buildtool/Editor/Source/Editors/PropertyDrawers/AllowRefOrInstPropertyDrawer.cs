using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System;
using System.Linq;

namespace IPTech.BuildTool {
    [CustomPropertyDrawer(typeof(AllowRefOrInstAttribute))]
    public class AllowRefOrInstPropertyDrawer : PropertyDrawer {
        const float HEIGHT = 18F;
        const float PROP_MARGIN = 2F;
        const float NEXT_PROP_HEIGHT = HEIGHT + PROP_MARGIN;

        static string[] EmptyArray = new string[1] { "create" };

        Type createListType;
        string[] createList;
        Type[] createTypes;
        Dictionary<string, float> propertyHeights;
        SerializedObject soTarget;
        float propHeight = 0F;
        bool isInst;
        Type propType;
        string toolTipAttributeText;
        float toolTipHeight;

        public AllowRefOrInstPropertyDrawer() {
            propertyHeights = new Dictionary<string, float>();
        }
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            SetPropertyType();
            EnsureCreateListIsPopulated();
            CalculateAdditionalPropertyHeights();
            return base.GetPropertyHeight(property, label) + propHeight;

            void SetPropertyType() {
                if(TryGetPropertyType(property, out Type propType)) {
                    this.propType = propType;
                } else {
                    this.propType = null;
                }
            }

            void CalculateAdditionalPropertyHeights() {
                if(isInst = IsPropertyInst(property)) {
                    if(property.objectReferenceValue != null) {
                        if(UpdateScriptableObjectReference()) {
                            CalcHeights();
                        }
                    }
                } else {
                    ClearScriptableObjectReference();
                }

                void CalcHeights() {
                    propHeight = 0F;
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
                    if(attribs!=null) {
                        foreach(var attrib in attribs) {
                            if(attrib is TooltipAttribute tt) {
                                toolTipAttributeText = tt.tooltip;
                                var hss = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Scene).GetStyle("helpBox");
                                toolTipHeight = hss.CalcHeight(new GUIContent(toolTipAttributeText), Mathf.Max(EditorGUIUtility.currentViewWidth - 100, 100));
                                propHeight += toolTipHeight + PROP_MARGIN;
                                //propHeight += NEXT_PROP_HEIGHT;
                                break;
                            }
                        }
                    }
                }
            }

            bool UpdateScriptableObjectReference() {
                if(soTarget == null || soTarget.targetObject != property.objectReferenceValue) {
                    soTarget = new SerializedObject(property.objectReferenceValue);
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
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            var pos = position;
            
            if(propType!=null) {
                EditorGUI.BeginProperty(pos, label, property);
                
                const int buttonWidth = 100;
                pos.width -= buttonWidth;
                pos.height = HEIGHT;
                EditorGUI.PropertyField(pos, property);
                
                pos = new Rect(pos.x + (pos.width), pos.y, buttonWidth, pos.height);

                EditorGUI.BeginChangeCheck();
                int val = EditorGUI.Popup(pos, 0, createList);
                if(EditorGUI.EndChangeCheck()) {
                    if(val > 0) {
                        val--;
                        var t = createTypes[val];
                        if(t.IsSubclassOf(typeof(ScriptableObject))) {
                            property.objectReferenceValue = ScriptableObject.CreateInstance(t);
                        } else {
                            property.objectReferenceValue = Activator.CreateInstance(t) as UnityEngine.Object;
                        }
                    }
                }
                EditorGUI.EndProperty();

                if(isInst && property.objectReferenceValue!=null) {
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

        bool IsPropertyInst(SerializedProperty prop) {
            if(prop.objectReferenceValue!=null) {
                return string.IsNullOrEmpty(AssetDatabase.GetAssetPath(prop.objectReferenceValue));
            }
            return false;
        }

        void EnsureCreateListIsPopulated() {
            try {
                if(createList == null || propType != createListType) {
                    var options = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a =>
                        a.GetTypes().Where(
                            t => !t.IsAbstract && t.IsSubclassOf(propType)
                        )).OrderBy(t => t.Name);
                    createTypes = options.ToArray();

                    var createOptions = new List<string>(createTypes.Select(t => t.Name));
                    createOptions.Insert(0, "create");

                    createList = createOptions.ToArray();
                }
            } catch(Exception e) {
                if(createList != EmptyArray) {
                    Debug.LogError(e);
                    createListType = propType;
                    createList = EmptyArray;
                }
            }
        }

        bool TryGetPropertyType(SerializedProperty prop, out Type propType) {
            propType = null;

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

            if(propType!=null) {
                return true;
            }
            return false;
        }
    }
}
