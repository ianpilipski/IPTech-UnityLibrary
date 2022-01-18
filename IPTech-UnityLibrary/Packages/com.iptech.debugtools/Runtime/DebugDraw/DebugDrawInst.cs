using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IPTech.DebugTools {

    public class DebugDrawInst {

        const string OBJECT_NAME = "IPTech::DebugDraw";

        private static DebugDraw _instance;
        public static DebugDraw Instance {
            get {
                if (_instance == null) {
                    DebugDraw[] ddArray = UnityEngine.Object.FindObjectsOfType<DebugDraw>();
                    if (ddArray != null) {
                        for (int i = 0; i < ddArray.Length; i++) {
                            if (ddArray[i].gameObject.name == OBJECT_NAME) {
                                _instance = ddArray[i];
                            }
                        }
                    }
                    if (_instance == null) {
                        GameObject go = new GameObject(OBJECT_NAME);
                        //go.hideFlags = HideFlags.HideAndDontSave;
                        _instance = go.AddComponent<DebugDraw>();
                    }
                }
                return _instance;
            }
        }

        public static void Sphere(Vector3 center, float radius) {
            Instance.Sphere(center, radius);
        }

        public static void GizmosDelegate(Action onDrawGizmos) {
            Instance.GizmosDelegate(onDrawGizmos);
        }

        public static void Text(Vector3 center, string text) {
            Instance.Text(center, text);
        }

        public void Line(Vector3 startPoint, Vector3 endPoint) {
            Instance.Line(startPoint, endPoint);
        }

        public void WireCube(Rect rect) {
            Instance.WireCube(rect);
        }

        public void WireCube(Rect rect, Matrix4x4 matrix) {
            Instance.WireCube(rect, matrix);
        }

        public void WireCube(Vector3 center, Vector3 size) {
            Instance.WireCube(center, size);
        }

        public void WireCube(Vector3 center, Vector3 size, Matrix4x4 matrix) {
            Instance.WireCube(center, size, matrix);
        }
    }

}
