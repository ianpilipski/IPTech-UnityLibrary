#if !(UNITY_EDITOR || DEVELOPMENT_BUILD || QA_BUILD)
#define CONTAINERDEBUGSERVICE_DISABLED
#endif

#if !CONTAINERDEBUGSERVICE_DISABLED

using System;
using UnityEngine;
using UnityEditor;

namespace IPTech.DebugConsoleService
{
    public static class FoldOutControl {
        private static double foldoutDestTime;

        public static bool Foldout(bool foldout, string label, bool showToggle, out int controlID) {
            Rect r = EditorGUILayout.GetControlRect();
            return Foldout(r, foldout, new GUIContent(label), EditorStyles.foldout, showToggle, out controlID);
        }

        private static bool Foldout(Rect position, bool foldout, GUIContent content, GUIStyle style, bool showToggle, out int controlID) {
            controlID = GUIUtility.GetControlID(content, FocusType.Keyboard, position);
            EventType eventType = Event.current.type;
            switch(eventType) {
                case EventType.MouseDown:
                    return HandleMouseDown(position, controlID, style, foldout);
                case EventType.MouseUp:
                    return HandleMouseUp(position, controlID, style, foldout);
                case EventType.KeyDown:
                    return HandleKeyDown(controlID, foldout);
                case EventType.Repaint:
                    return HandleRepaint(position, controlID, style, content, foldout, showToggle);
            }
            return foldout;
        }

        static bool HandleMouseDown(Rect position, int controlID, GUIStyle style, bool foldout) {
            if(position.Contains(Event.current.mousePosition) && Event.current.button == 0) {
                if(!GetFoldoutRect(position,style).Contains(Event.current.mousePosition)) {
                    GUIUtility.hotControl = controlID;
                    GUIUtility.keyboardControl = controlID;
                }
                Event.current.Use();
            }
            return foldout;
        }

        static Rect GetFoldoutRect(Rect position, GUIStyle style) {
            Rect foldoutRect = position;
            foldoutRect.width = (float)style.padding.left;
            foldoutRect.x += EditorGUI.indentLevel;
            return foldoutRect;
        }

        static bool HandleMouseUp(Rect position, int controlID, GUIStyle style, bool foldout) {
            if(GUIUtility.hotControl == controlID) {
                GUIUtility.hotControl = 0;
                GUIUtility.keyboardControl = controlID;
                Event.current.Use();
            }
            if(GetFoldoutRect(position,style).Contains(Event.current.mousePosition)) {
                Event.current.Use();
                GUI.changed = true;
                return !foldout;
            }
            return foldout;
        }

        static bool HandleKeyDown(int controlID, bool foldout) {
            if(GUIUtility.keyboardControl == controlID) {
                KeyCode keyCode = Event.current.keyCode;
                if((keyCode == KeyCode.LeftArrow && foldout) || (keyCode == KeyCode.RightArrow && !foldout)) {
                    foldout = !foldout;
                    GUI.changed = true;
                    Event.current.Use();
                }
            }
            return foldout;
        }

        static bool HandleRepaint(Rect position, int controlID, GUIStyle style, GUIContent content, bool foldout, bool showToggle) {
            Rect position2 = new Rect(position.x + EditorGUI.indentLevel, position.y, EditorGUIUtility.labelWidth - EditorGUI.indentLevel, position.height);
            if(showToggle) {
                style.Draw(position2, content, controlID, foldout);
            } else {
                position2.x += 12;
                EditorStyles.label.Draw(position2, content, controlID, foldout);
            }
            return foldout;
        }
    }
}

#endif

