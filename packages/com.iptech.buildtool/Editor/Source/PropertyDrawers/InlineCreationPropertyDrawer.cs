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
        
        Dictionary<string, float> propertyHeights = new Dictionary<string, float>();
        SerializedObject soTarget;
        
        bool shouldDrawItem;
        Type propType;
        string toolTipAttributeText;
        
        private float ToolTipHeight => string.IsNullOrWhiteSpace(toolTipAttributeText) ? 0 : EditorGUIUtility.singleLineHeight * 4;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            UpdatePropertyType(property);

            var ttHeight = 0F;
            if(ToolTipHeight > 0 && soTarget != null) {
                var p = soTarget.FindProperty("m_Script");
                ttHeight = EditorGUIUtility.singleLineHeight;
                ttHeight += (p == null || !p.isExpanded) ? 0F : ToolTipHeight;
            }

            var lineCount = 1 + (shouldDrawItem ? (3 + propertyHeights.Count + ((ToolTipHeight > 0) ? 1 : 0)) : 0);
            var totalPropHeight = propertyHeights.Sum(kvp => kvp.Value);
            return EditorGUIUtility.singleLineHeight + totalPropHeight + ttHeight + (lineCount - 1) * EditorGUIUtility.standardVerticalSpacing;
        }

        Rect NextControl(Rect pos) {
            return NextControl(pos, EditorGUIUtility.singleLineHeight);
        }

        Rect NextControl(Rect pos, float height) {
            pos.y += pos.height + EditorGUIUtility.standardVerticalSpacing;
            pos.height = height;
            return pos;
        }

        GUIContent GetPropertyLabel(SerializedProperty property, GUIContent label) {
            if(property.name == "data") { // this is an array member so replace "element" label with our name
                if(property.objectReferenceValue != null) {
                    return new GUIContent(property.objectReferenceValue.name);
                }
            }
            return label;
        }

        Rect DrawMainProperty(Rect position, SerializedProperty property, GUIContent label) {
            position.height = EditorGUIUtility.singleLineHeight;

            var pos = position;
            using(var ps = new EditorGUI.PropertyScope(pos, GetPropertyLabel(property, label), property)) {
                const int buttonWidth = 100;
                pos.width -= buttonWidth + 4;
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
            return position;
        }

        Rect DrawBackGround(Rect pos, Rect orignalRect) {
            var backgroundRect = pos;
            backgroundRect.height = orignalRect.height - (pos.y - orignalRect.y) - EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.HelpBox(backgroundRect, "", MessageType.None);
            pos.y += EditorGUIUtility.standardVerticalSpacing;
            pos.x += EditorGUIUtility.standardVerticalSpacing;
            pos.width -= EditorGUIUtility.standardVerticalSpacing * 2;
            pos.height = EditorGUIUtility.singleLineHeight;
            return pos;
        }

        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            UpdatePropertyType(property);

            var pos = position;
            pos.height = EditorGUIUtility.singleLineHeight;
            
            if(propType!=null) {
                var origObjectReference = property.objectReferenceValue;

                pos = DrawMainProperty(pos, property, label);
                
                if(shouldDrawItem && soTarget!=null ) {
                    EditorGUI.indentLevel++;
                    pos = EditorGUI.IndentedRect(pos);
                    EditorGUI.indentLevel--;

                    pos = NextControl(pos);
                    pos = DrawBackGround(pos, position);

                    soTarget.Update();
                    var p = soTarget.GetIterator();
                    EditorGUI.BeginProperty(pos, new GUIContent(p.displayName), p);
                    p.NextVisible(true);
                    var first = true;
                    do {
                        if(p.propertyPath == "m_Script") {
                            if(!string.IsNullOrEmpty(toolTipAttributeText)) {
                                if(!first) {
                                    pos = NextControl(pos);
                                }
                                var newVal = EditorGUI.Foldout(pos, p.isExpanded, new GUIContent("info"));
                                if(newVal != p.isExpanded) {
                                    p.isExpanded = newVal;
                                    GUI.changed = true;
                                    Event.current.Use();
                                }
                                if(p.isExpanded) {
                                    pos = NextControl(pos, ToolTipHeight);
                                    EditorGUI.HelpBox(pos, toolTipAttributeText, MessageType.Info);
                                }
                                first = false;
                            }
                        } else {
                            if(!first) {
                                pos = NextControl(pos, propertyHeights[p.propertyPath]);
                            } else {
                                pos.height = propertyHeights[p.propertyPath];
                            }
                            EditorGUI.PropertyField(pos, p);
                            first = false;
                        }
                    } while(p.NextVisible(false));
                    EditorGUI.EndProperty();
                    soTarget.ApplyModifiedProperties();
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
            if(shouldDrawItem) {
                if(UpdateScriptableObjectReference()) {
                    CalculateAdditionalPropertyHeights();
                }
            } else {
                ClearScriptableObjectReference();
            }
            shouldDrawItem = soTarget != null && (propertyHeights.Count > 0 || ToolTipHeight > 0);
            return propType != null;

            bool UpdateScriptableObjectReference() {
                if(soTarget == null || soTarget.targetObject != prop.objectReferenceValue) {
                    soTarget = new SerializedObject(prop.objectReferenceValue);
                    return true;
                }
                soTarget.UpdateIfRequiredOrScript();
                return true;
            }

            void ClearScriptableObjectReference() {
                soTarget = null;
                toolTipAttributeText = null;
                propertyHeights.Clear();
            }

            bool IsPropertyInst() {
                if(prop.objectReferenceValue != null) {
                    return AssetDatabase.Contains(prop.objectReferenceValue) && !AssetDatabase.IsSubAsset(prop.objectReferenceValue);
                }
                return false;
            }

            void CalculateAdditionalPropertyHeights() {
                var p = soTarget.GetIterator();
                p.NextVisible(true);
                do {
                    if(p.propertyPath != "m_Script") {
                        propertyHeights[p.propertyPath] = EditorGUI.GetPropertyHeight(p);
                    }
                } while(p.NextVisible(false));

                toolTipAttributeText = null;
                var attribs = soTarget.targetObject.GetType().GetCustomAttributes(false);
                if(attribs != null) {
                    foreach(var attrib in attribs) {
                        if(attrib is TooltipAttribute tt) {
                            toolTipAttributeText = tt.tooltip;
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

        float CalcToolTipHeight(string toolTipText, float width) {
            var hss = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Scene).GetStyle("helpBox");
            return hss.CalcHeight(new GUIContent(toolTipAttributeText), width);
        }
    }
}
