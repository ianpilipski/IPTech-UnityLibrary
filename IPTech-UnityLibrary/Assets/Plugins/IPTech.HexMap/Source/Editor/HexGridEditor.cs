using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.SceneManagement;

namespace IPTech.HexMap {
	[CustomEditor(typeof(HexGrid))]
	public class HexGridEditor : Editor {
		void OnSceneGUI() {
			// do nothing in this one, as it only exists to allow toggle of gizmo in editor
		}
	}
	
	public class HexGridEditorStatic : ScriptableObject {
		static HexGridEditorStatic _instance;

		bool _optionShowHexGrid = true;
		bool _optionSnapOnDragAndDrop = true;
		bool _optionSnapToGrid = true;

		Vector3 _dragStartPos;
		bool _mouseDown;
		Rect _onScreenControlsWindowRect = new Rect(10, 20, 100, 50);

		GameObject _dragAndDropObjectInstance;
		
		static IList<TouchingInfo> _touchingInfoList = new List<TouchingInfo>();

		[InitializeOnLoadMethod]
		static void InitializeOnLoad() {
			HexGridEditorStatic[] hexGridEditorStatics = Resources.FindObjectsOfTypeAll<HexGridEditorStatic>();
			if(hexGridEditorStatics.Length==0) {
				_instance = ScriptableObject.CreateInstance<HexGridEditorStatic>();
			} else {
				_instance = hexGridEditorStatics[0];
			}
		}

		void OnEnable() {
			hideFlags = HideFlags.HideAndDontSave;
#if UNITY_2019_1_OR_NEWER
			SceneView.duringSceneGui += HandleOnSceneGUI;
#else
			SceneView.onSceneGUIDelegate += HandleOnSceneGUI;
#endif
		}

		void OnDisable() {
#if UNITY_2019_1_OR_NEWER
			SceneView.duringSceneGui -= HandleOnSceneGUI;
#else
			SceneView.onSceneGUIDelegate -= HandleOnSceneGUI;
#endif
		}

		void HandleOnSceneGUI(SceneView sceneView) {
			if(Event.current.type == EventType.MouseDown) {
				ConditionalDragStart();
			} else if(Event.current.type == EventType.MouseUp) {
				ConditionalDragEnd();
			} else if(Event.current.type == EventType.MouseDrag) {
				ConditionalUpdateDrag();
			} else if(Event.current.type == EventType.DragUpdated) {
				ConditionalDragUpdatedNewPrefab();
			} else if(Event.current.type == EventType.DragPerform) {
				ConditionalDragPerformNewPrefab();
			} else if(Event.current.type == EventType.DragExited) {
				ConditionalDragExitedNewPrefab();
			}
			
			HexGrid hexGrid = GetTopMostHexGridFromActiveSelection();
			if(hexGrid!=null) {
				DrawOnScreenControls(sceneView, hexGrid);
			}
		}

		void ConditionalDragUpdatedNewPrefab() {
			if(IsDragAndDropEnabled()) {
				GameObject draggingPrefab = GetValidDragAndDropGameObject();
				bool didHitHexPlane = false;
				Vector3 hitHexPlanePosition = Vector3.zero;

				if(draggingPrefab != null) {
					Vector2 guiPosition = Event.current.mousePosition;
					Ray ray = HandleUtility.GUIPointToWorldRay(guiPosition);
					Plane hexPlane = GetHexGridPlane();
					float hitTime;
					didHitHexPlane = hexPlane.Raycast(ray, out hitTime);
					hitHexPlanePosition = ray.origin + (ray.direction * hitTime);
				}
				
				if(!didHitHexPlane) { 
					DestroyTempDragObject();
					DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
					Event.current.Use();
					return;
				}

				EnsureCreateTempDragObject(draggingPrefab);
				UpdateTempDragObjectPosition(hitHexPlanePosition);
				
				DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
				Event.current.Use();
			}
		}
		
		bool IsDragAndDropEnabled() {
			return _optionSnapOnDragAndDrop && GetTopMostHexGridFromActiveSelection() != null;
		}
		
		void EnsureCreateTempDragObject(GameObject draggingPrefab) {
			if(_dragAndDropObjectInstance==null) {
				HexGrid hexGrid = GetTopMostHexGridFromActiveSelection();
				_dragAndDropObjectInstance = Object.Instantiate<GameObject>(draggingPrefab, hexGrid.transform);
				_dragAndDropObjectInstance.name = draggingPrefab.name;
				_dragAndDropObjectInstance.hideFlags = HideFlags.HideAndDontSave;
			}
		}
		
		void DestroyTempDragObject() {
			if(_dragAndDropObjectInstance!=null) {
				Object.DestroyImmediate(_dragAndDropObjectInstance);
				_dragAndDropObjectInstance = null;
			}
		}
		
		void UpdateTempDragObjectPosition(Vector3 hitHexPlanePosition) {
			_dragAndDropObjectInstance.transform.position = hitHexPlanePosition;
			Vector3 snapOffset = CalculateSnapWorldOffset(GetTopMostHexGridFromActiveSelection(), _dragAndDropObjectInstance.transform);
			_dragAndDropObjectInstance.transform.Translate(snapOffset, Space.World);
		}

		Plane GetHexGridPlane() {
			HexGrid hexGrid = GetTopMostHexGridFromActiveSelection();
			return new Plane(hexGrid.transform.TransformDirection(Vector3.up), hexGrid.transform.position);
		}
		
		GameObject GetValidDragAndDropGameObject() {
			if(DragAndDrop.objectReferences.Length > 1) return null;
			return DragAndDrop.objectReferences[0] as GameObject;
		}
		
		void ConditionalDragPerformNewPrefab() {
			if(_dragAndDropObjectInstance) {
				GameObject prefab = (GameObject)PrefabUtility.InstantiatePrefab(GetValidDragAndDropGameObject());
				prefab.transform.SetPositionAndRotation(
					_dragAndDropObjectInstance.transform.position,
					_dragAndDropObjectInstance.transform.rotation
				);
				prefab.transform.SetParent(_dragAndDropObjectInstance.transform.parent, true);
				Undo.RegisterCreatedObjectUndo(prefab, "Create " + prefab.name);
				DestroyTempDragObject();
				
				Selection.activeObject = prefab;
				DragAndDrop.AcceptDrag();
				Event.current.Use();
			}
		}
		
		void ConditionalDragExitedNewPrefab() {
			DestroyTempDragObject();
		}
		
		HexGrid GetTopMostHexGridFromActiveSelection() {
			if(Selection.transforms!=null) {
				var tt = TransformsControlledByHexGrid(Selection.transforms, HexGridCheckType.IncludeHexGrid);
				foreach(var kvp in tt) { 
					HexGrid[] parentHexGrids = kvp.Key.GetComponentsInParent<HexGrid>();
					if(!tt.Any(ttKvp =>
						parentHexGrids.Any(pp => pp != kvp.Key && pp == ttKvp.Key)
					)) return kvp.Key;
				}
			}
			return null;
		}

		void ConditionalDragStart() {
			_dragStartPos = Tools.handlePosition;
			if(Tools.current == Tool.Move) {
				_mouseDown = true;
			}
		}

		void ConditionalDragEnd() {
			if(_mouseDown) {
				if(Tools.current == Tool.Move) {
					if(_optionSnapToGrid) {
						Vector3 dragDelta = Tools.handlePosition - _dragStartPos;
						if(dragDelta.magnitude > float.Epsilon) {
							TransformSelectionToNearestHexGrid();
						}
					}
				}
				ClearDragVisuals();
				_mouseDown = false;
			}
		}

		void TransformSelectionToNearestHexGrid() {
			IDictionary<HexGrid, IList<Transform>> transformsControlledByHexGrid = TransformsControlledByHexGrid(Selection.GetTransforms(SelectionMode.Unfiltered));

			foreach(var kvp in transformsControlledByHexGrid) {
				Vector3 snapOffsetWorld = CalculateSnapWorldOffset(kvp.Key, kvp.Value[0]);

				foreach(var t in kvp.Value) {
					t.position = t.position + snapOffsetWorld;
				}
			}
		}

		Vector3 CalculateSnapWorldOffset(HexGrid hexGrid, Transform t) {
			Vector3 localPos = hexGrid.transform.InverseTransformPoint(t.position);
			HexCoordinates hexCoordinates = HexCoordinates.FromPosition(localPos);
			Vector3 snappedLocalPos = hexCoordinates.ToPosition();
			snappedLocalPos.y = localPos.y;
			Vector3 snappedWorldPos = hexGrid.transform.TransformPoint(snappedLocalPos);
			return snappedWorldPos - t.position;
		}

		void ConditionalUpdateDrag() {
			if(_mouseDown && Tools.current == Tool.Move) {
				UpdateDragVisuals();
			}
		}

		[DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
		static void OnDrawGizmosStatic(HexGrid hexGrid, GizmoType gizmoType) {
			if(_instance != null) {
				_instance.HandleOnDrawGizmos(hexGrid, gizmoType);
			}
		}
		
		void HandleOnDrawGizmos(HexGrid hexGrid, GizmoType gizmoType) {
			if(ShouldDrawGrid(hexGrid)) {
				DrawHexGrid(hexGrid);
			}

			if(ShouldDrawTouchingInfo(hexGrid)) {
				DrawTouchingInfo(hexGrid);
			}
		}
		
		bool ShouldDrawGrid(HexGrid hexGrid) {
			if(_optionShowHexGrid) {
				// Only draw the topmost hexGrid in a chain
				Transform[] selectedTransforms = Selection.GetTransforms(SelectionMode.Unfiltered);
				var transCheck = TransformsControlledByHexGrid(selectedTransforms, HexGridCheckType.IncludeHexGrid);

				bool isHexGridSelected = transCheck.Any(kvp => kvp.Key == hexGrid);
				if(isHexGridSelected) {
					HexGrid[] parentHexGrids = hexGrid.GetComponentsInParent<HexGrid>();
					return !transCheck.Any(kvp => parentHexGrids.Any(p => p != hexGrid && kvp.Key == p));
				}
			}
			return false;
		}
		
		bool ShouldDrawTouchingInfo(HexGrid hexGrid) {
			return TransformsControlledByHexGrid(Selection.GetTransforms(SelectionMode.Unfiltered)).Any(kvp => kvp.Key == hexGrid);
		}

		void DrawHexGrid(HexGrid hexGrid) {
			HexCoordinates centerHexCoordinates = RecalcScreenCenterHex(hexGrid);
			int colCenter;
			int rowCenter;
			centerHexCoordinates.ToOffsetCoordinates(out colCenter, out rowCenter);
			for(int i = -23; i < 23; i++) {
				for(int j = -20; j < 20; j++) {
					DrawHexGizmo(hexGrid, colCenter + i, rowCenter + j);
				}
			}
		}

		void DrawHexGizmo(HexGrid hexGrid, int col, int row, float columnHeight = 0, bool highlight = false) {
			Vector3 offset = HexCoordinates.ToPosition(col, row);
			Matrix4x4 matrix = hexGrid.transform.localToWorldMatrix * Matrix4x4.Translate(offset);
			HexTile.DrawHexGizmo(matrix, (highlight ? hexGrid.HighlightColor : hexGrid.GridColor), columnHeight);
			//UnityEditor.Handles.Label(transform.TransformPoint(offset), HexCoordinates.FromOffsetCoordinates(col,row).ToString());
		}
		
		void DrawOnScreenControls(SceneView sceneView, HexGrid hexGrid) {
			Handles.BeginGUI();
			_onScreenControlsWindowRect = GUILayout.Window(
				0, 
				_onScreenControlsWindowRect, 
				(id) => { HandleWindowFunction(hexGrid); },
				"HexGrid"
			);

			if(sceneView.camera != null) {
				Rect sceneRect = sceneView.position;
				_onScreenControlsWindowRect.x = Mathf.Max(_onScreenControlsWindowRect.x, 10);
				_onScreenControlsWindowRect.x = Mathf.Min(_onScreenControlsWindowRect.x, sceneRect.width - _onScreenControlsWindowRect.width - 10);
				_onScreenControlsWindowRect.y = Mathf.Max(_onScreenControlsWindowRect.y, 20);
				_onScreenControlsWindowRect.y = Mathf.Min(_onScreenControlsWindowRect.y, sceneRect.height - _onScreenControlsWindowRect.height - 10);
			}
			Handles.EndGUI();
		}

		void HandleWindowFunction(HexGrid hexGrid) {
			GUILayout.BeginVertical();
			_optionShowHexGrid = GUILayout.Toggle(_optionShowHexGrid, "Show Grid");
			_optionSnapOnDragAndDrop = GUILayout.Toggle(_optionSnapOnDragAndDrop, "Snap On Drag/Drop");
			_optionSnapToGrid = GUILayout.Toggle(_optionSnapToGrid, "Enabled");
			GUILayout.EndVertical();
			Rect contentsRect = GUILayoutUtility.GetLastRect();
			GUI.DragWindow(new Rect(0,0, contentsRect.width, contentsRect.y));
		}


		void DrawTouchingInfo(HexGrid hexGrid) {
			foreach(var ti in _touchingInfoList) {
				if(GetHexGridForTransform(ti.transform) == hexGrid) {
					Vector3 hexGridLocalPos = hexGrid.transform.InverseTransformPoint(ti.position);
					HexCoordinates hexCoordinates = HexCoordinates.FromPosition(hexGridLocalPos);
					int touchCol;
					int touchRow;
					hexCoordinates.ToOffsetCoordinates(out touchCol, out touchRow);
					float columnHeight = hexGridLocalPos.y;
					columnHeight = (Mathf.Abs(columnHeight) < float.Epsilon) ? columnHeight = float.Epsilon : columnHeight;
					DrawHexGizmo(hexGrid, touchCol, touchRow, columnHeight, true);
				}
			}
		}

		void UpdateDragVisuals() {
			ClearDragVisuals();
			IDictionary<HexGrid, IList<Transform>> transformsControlledByHexGrid = TransformsControlledByHexGrid(UnityEditor.Selection.GetTransforms(UnityEditor.SelectionMode.Unfiltered));

			foreach(var kvp in transformsControlledByHexGrid) {
				Vector3 snapWorldOffset = CalculateSnapWorldOffset(kvp.Key, kvp.Value[0]);

				foreach(var t in kvp.Value) {
					TouchCell(t.position + snapWorldOffset, t);
				}
			}
		}

		void ClearDragVisuals() {
			_touchingInfoList.Clear();
		}

		IDictionary<HexGrid, IList<Transform>> TransformsControlledByHexGrid(Transform[] transforms, HexGridCheckType checkType = HexGridCheckType.OnlyChildren) {
			IDictionary<HexGrid, IList<Transform>> retVal = new Dictionary<HexGrid, IList<Transform>>();
			if(transforms != null) {
				foreach(var t in transforms) {
					HexGrid hexGrid = GetHexGridForTransform(t, checkType);
					if(hexGrid!=null) {
						if(!retVal.ContainsKey(hexGrid)) {
							retVal.Add(hexGrid, new List<Transform>());
						}
						retVal[hexGrid].Add(t);
					}
				}
			}
			return retVal;
		}

		bool TransformIsControlledByHexGrid(HexGrid hexGrid, Transform transformToCheck) {
			HexGrid transformHexGrid = GetHexGridForTransform(transformToCheck);
			return (transformHexGrid != null) && (hexGrid == null || hexGrid == transformHexGrid);
		}
		
		bool TransformIsControlledByHexGrid(Transform transformToCheck) {
			return GetHexGridForTransform(transformToCheck) != null;
		}
		
		enum HexGridCheckType {
			OnlyChildren,
			IncludeHexGrid
		}
		
		HexGrid GetHexGridForTransform(Transform transformToCheck, HexGridCheckType checkType = HexGridCheckType.OnlyChildren ) {
			HexGrid retVal = null;
			if(transformToCheck != null) {
				if(checkType == HexGridCheckType.IncludeHexGrid) {
					//retVal = transformToCheck.GetComponent<HexGrid>();
					retVal = transformToCheck.GetComponentInParent<HexGrid>();
				}
				if(retVal == null) {
					Transform parentToCheck = transformToCheck.parent;
					if(parentToCheck != null) {
						/*retVal = parentToCheck.GetComponent<HexGrid>();
						 * if(retVal == null) {
							HexGridChild hgc = parentToCheck.GetComponent<HexGridChild>();
							if(hgc != null) {
								retVal = hgc.GetComponentInParent<HexGrid>();
							}
						}*/
						retVal = parentToCheck.GetComponentInParent<HexGrid>();
					}
				}
			}
			return retVal;
		}

		HexCoordinates RecalcScreenCenterHex(HexGrid hexGrid) {
			if(Camera.current != null) {
				Ray dirRay = Camera.current.ViewportPointToRay(new Vector3(0.5f, 0.5f, 1F));
				Vector3 worldPos;
				if(RaycastHexPlane(hexGrid, dirRay, out worldPos)) {
					Vector3 localPos = hexGrid.transform.InverseTransformPoint(worldPos);
					return HexCoordinates.FromPosition(localPos);
				}
			}
			return new HexCoordinates(0, 0);
		}

		bool RaycastHexPlane(HexGrid hexGrid, Ray inputRay, out Vector3 worldPos) {
			Plane plane = new Plane(hexGrid.transform.TransformDirection(Vector3.up), hexGrid.transform.position);
			float enter;
			if(plane.Raycast(inputRay, out enter)) {
				worldPos = inputRay.origin + (inputRay.direction * enter);
				return true;
			}
			worldPos = Vector3.zero;
			return false;
		}

		void TouchCell(Vector3 position, Transform activeTransform) {
			_touchingInfoList.Add(new TouchingInfo() {
				position = position,
				transform = activeTransform	
			});
		}

		struct TouchingInfo {
			public Vector3 position;
			public Transform transform;
		}

	}
}
