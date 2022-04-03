using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;

namespace IPTech.EditorExtensions {
	[CustomEditor(typeof(ReferenceVisualizer))]
	public class ReferenceVisualizerEditor : Editor {

		bool _recalculating;
		DateTime _nextRefreshTime;
		int _recalculateCounter;
		
		IList<Transform> _toReferences;
		IList<Transform> _fromReferences;
		IEnumerator<Transform> _refToThisEnumerator;
		
		ReferenceVisualizer refVis;
		
		void OnEnable() {
			refVis = (ReferenceVisualizer)target;
			RecalculateReferences();
		}

		void OnDisable() {
			if(_recalculating) {
				EndRecalculateReferences();
			}
		}

		public override void OnInspectorGUI() {
			bool changed = DrawDefaultInspector();
			
			if(GUILayout.Button("Recalculate References") || changed) {
				RecalculateReferences();
			}
		}
		
		void RecalculateReferences() {
			BeginRecalculateReferences();
		}

		void BeginRecalculateReferences() {
			_toReferences = new List<Transform>();
			_fromReferences = GetReferencedTransforms(refVis.transform);
			if(refVis.ShowTo) {
				_nextRefreshTime = DateTime.Now.AddMilliseconds(250);
				_recalculateCounter = 0;
				_recalculating = true;
				_refToThisEnumerator = GetReferencedTransfromsToThis(refVis.transform);
				EditorApplication.update -= UpdateRecalc;
				EditorApplication.update += UpdateRecalc;
			}
		}
		
		void UpdateRecalc() {
			if(DateTime.Now >= _nextRefreshTime) {
				_recalculateCounter++;
				_nextRefreshTime = DateTime.Now.AddMilliseconds(250);
				SceneView.RepaintAll();
			}
			if(_refToThisEnumerator.MoveNext()) {
				if(_refToThisEnumerator.Current != null) {
					_toReferences.Add(_refToThisEnumerator.Current);
				}
				return;
			}
			EndRecalculateReferences();
		}

		void EndRecalculateReferences() {
			EditorApplication.update -= UpdateRecalc;
			_recalculating = false;
			SceneView.RepaintAll();
		}
		
		void OnSceneGUI() {
			using(new Handles.DrawingScope(Color.yellow)) {
				DrawRecalculating();
				DrawConnectionsFrom();
				DrawConnectionsTo();
			}
		}

		void DrawRecalculating() {
			if(_recalculating) {
				string recalcText = "recalculating...".Substring(0, _recalculateCounter % "recalculating...".Length);
				GUIContent content = new GUIContent(recalcText);
				Vector2 size = GUI.skin.label.CalcSize(content);
				Vector3 pos = refVis.transform.position;
				Vector3 screenPos = Camera.current.WorldToScreenPoint(pos);
				screenPos.x = screenPos.x - size.x;
				screenPos.y = screenPos.y - 10;
				Vector3 worldPos = Camera.current.ScreenToWorldPoint(screenPos);
				
				Handles.Label(worldPos, content);
			}
		}
		
		void DrawConnectionsFrom() {
			if(_fromReferences == null) return;
			foreach(var t in _fromReferences) {
				Handles.DrawLine(refVis.transform.position, t.position);
				DrawSelectionHandle(t);
			}
		}
		
		void DrawConnectionsTo() {
			if(_toReferences == null) return;
			foreach(var t in _toReferences) {
				Handles.DrawDottedLine(refVis.transform.position, t.position, 1.0f);
				DrawSelectionHandle(t);
			}
		}
		
		void DrawSelectionHandle(Transform t) {
			float size = HandleUtility.GetHandleSize(t.position) * 0.1f;
			if(Handles.Button(t.position, Quaternion.identity, size, size, Handles.SphereHandleCap)) { 
				Selection.SetActiveObjectWithContext(t, null);
			}
		}

		IEnumerator<Transform> GetReferencedTransfromsToThis(Transform toTransform) {
			Transform[] transforms = FindObjectsOfType<Transform>();
			foreach(var t in transforms) {
				Transform yieldTransform = null;
				if(t!=null && t!=toTransform) {
					Component[] components = t.GetComponents<Component>();
					foreach(var c in components) {
						if(c is Transform) continue;
						
						if(HasReferenceToTransform(c, toTransform)) {
							yieldTransform = t;
							break;
						}
					}
				}
				yield return yieldTransform;
			}
		}

		IList<Transform> GetReferencedTransforms(Transform fromTransform) {
			IList<Transform> transforms = new List<Transform>();
			Component[] components = fromTransform.GetComponents<Component>();
			foreach(Component c in components) {
				if(c!=null && c!=fromTransform) {
					IList<Transform> tforms = GetReferrencedComponents(c, t => t!=fromTransform);
					foreach(var t in tforms) {
						if(!transforms.Contains(t)) {
							transforms.Add(t);
						}
					}
				}
			}
			return transforms;
		}
		
		bool HasReferenceToTransform(Component obj, Transform toTransform) {
			SerializedObject so = new SerializedObject(obj);
			SerializedProperty sp = so.GetIterator();
			
			while(sp.NextVisible(true)) {
				if(sp.propertyType == SerializedPropertyType.ObjectReference) {
					if(sp.objectReferenceValue!=null) {
						Transform t = GetTransformFromObjectRef(sp.objectReferenceValue);
						if(t == toTransform) return true;
					}
				}
			}
			return false;
		}
		
		IList<Transform> GetReferrencedComponents(Component obj, Func<Transform, bool> predicate) {
			List<Transform> retVal = new List<Transform>();

			SerializedObject so = new SerializedObject(obj);
			SerializedProperty sp = so.GetIterator();

			while(sp.NextVisible(true)) {
				if(sp.propertyType == SerializedPropertyType.ObjectReference) {
					if(sp.objectReferenceValue != null) {
						Transform t = GetTransformFromObjectRef(sp.objectReferenceValue);
						if(t != null && !retVal.Contains(t) && predicate(t)) {
							retVal.Add(t);
						}
					}
				}
			}

			return retVal;
		}
		
		Transform GetTransformFromObjectRef(UnityEngine.Object obj) {
			Transform t = null;
			if(obj is GameObject) {
				t = ((GameObject)obj).transform;
			} else if(obj is Component) {
				Component c = (Component)obj;
				t = c.transform;
			}
			return t;
		}
	}
}
