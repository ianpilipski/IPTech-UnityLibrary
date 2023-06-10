using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditor.IMGUI.Controls;

namespace IPTech.EditorTools
{
	public class ProjectCleanerEditor : EditorWindow
	{
		List<string> _rootObjectGuids;
		MultiColumnHeader _rootObjectMultiColumnHeader;
		float _rootObjectColumnWidth = 400F;
		Vector2 _rootObjectsScrollPos;
		Vector2 _rootObjectsInnerScrollPos;
		Rect _rootObjectInnerContentRect;

		[MenuItem("IPTech/Project/Cleaner ...")]
		[MenuItem("Window/IPTech/Project/Cleaner")]
		static void MenuOpen() {
			var win = EditorWindow.GetWindow<ProjectCleanerEditor>();
			win.titleContent = new GUIContent("Project Cleaner");
			win.Show();
		}

		private void OnEnable() {
			//if(_rootObjectGuids==null) {
				_rootObjectGuids = new List<string>();

				PopulateRootObjectsFromSceneList();
				BuildMultiColumnHeader();
			//}

			void PopulateRootObjectsFromSceneList() {
				foreach(EditorBuildSettingsScene scene in EditorBuildSettings.scenes) {
					string guid = AssetDatabase.AssetPathToGUID(scene.path);
					if(string.IsNullOrEmpty(guid)) {
						Debug.LogError("asset not found for scene: " + scene.path);
					}
					_rootObjectGuids.Add(guid);
				}
			}

			void BuildMultiColumnHeader() {
				var columnA = new MultiColumnHeaderState.Column() { 
	 				headerContent = new GUIContent("Name", "Name of the object"), 
	 				headerTextAlignment = TextAlignment.Left, 
	 				autoResize = false, 
	 				minWidth = 100, 
	 				width = _rootObjectColumnWidth,
					allowToggleVisibility = false
				};
				var columnB = new MultiColumnHeaderState.Column() { 
	 				headerContent = new GUIContent("Path", "Path to the asset"), 
	 				headerTextAlignment = TextAlignment.Left, 
	 				autoResize = true, 
	 				minWidth = 100,
					allowToggleVisibility = false
				};
				var columns = new MultiColumnHeaderState.Column[] { columnA, columnB };
				
				var state = new MultiColumnHeaderState(columns);
				_rootObjectMultiColumnHeader = new MultiColumnHeader(state);
				_rootObjectMultiColumnHeader.ResizeToFit();
			}
		}

		private void OnGUI() {
			GUISkin skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);
			DrawRootObjects();

			void DrawRootObjects() {
				using(new EditorGUILayout.VerticalScope(EditorStyles.helpBox)) {
					DrawRootObjectsToolbar();
					using(var scrollView = new EditorGUILayout.ScrollViewScope(_rootObjectsScrollPos, new GUIStyle(), new GUIStyle())) {
						DrawRootObjectsMultiColumnHeader();
					}
					using(var innerScroll = new EditorGUILayout.ScrollViewScope(_rootObjectsScrollPos)) {
						DrawRootObjectsMultiColumnList();
						if(Event.current.type == EventType.Repaint) {
							_rootObjectInnerContentRect = GUILayoutUtility.GetLastRect();
						}
						_rootObjectsScrollPos = innerScroll.scrollPosition;
					}
				}

				void DrawRootObjectsToolbar() {
					using(new EditorGUILayout.HorizontalScope(EditorStyles.toolbar)) {
						GUILayout.FlexibleSpace();
						if(GUILayout.Button("+Add", EditorStyles.toolbarButton)) {
							//TODO: add
						}
					}
				}

				void DrawRootObjectsMultiColumnHeader() {
					using(new EditorGUILayout.VerticalScope(GUILayout.MinWidth(_rootObjectInnerContentRect.width))) {
						_rootObjectMultiColumnHeader.ResizeToFit();
						_rootObjectColumnWidth = _rootObjectMultiColumnHeader.GetColumn(0).width;
						_rootObjectMultiColumnHeader.OnGUI(GUILayoutUtility.GetRect(100F, 10000F, 24F, 24F), 0);
					}
				}

				void DrawRootObjectsMultiColumnList() {
					using(new GUILayout.VerticalScope()) {
						DrawSceneRootObjectsMultiColumnList();
						DrawResourcesRootObjectsMultiColumnList();
					}

					void DrawSceneRootObjectsMultiColumnList() {
						for(int i = 0; i < _rootObjectGuids.Count; i++) {
							DrawMultiColumnObject(_rootObjectGuids[i]);
						}
					}

					void DrawResourcesRootObjectsMultiColumnList() {
						string[] resGuids = AssetDatabase.FindAssets("Resources");
						foreach(var resGuid in resGuids) {
							string resPath = AssetDatabase.GUIDToAssetPath(resGuid);
							string[] files = Directory.GetFiles(resPath, "*.*", SearchOption.AllDirectories);
							foreach(var file in files) {
								string assetGuid = AssetDatabase.AssetPathToGUID(file);
								if(!string.IsNullOrEmpty(assetGuid)) {
									DrawMultiColumnObject(assetGuid);
								}
							}
						}
					}

					void DrawMultiColumnObject(string guid) {
						ObjectData od = GetObjectDataFromGuid(guid);
						using(new EditorGUILayout.HorizontalScope()) {
							GUILayout.Label(od.name, GUILayout.Width(_rootObjectColumnWidth));
							GUILayout.Label(od.path, GUILayout.ExpandWidth(true));
						}
					}
				}
			}
		}

		ObjectData GetObjectDataFromGuid(string guid) {
			var retVal = new ObjectData {
				guid = guid,
				path = AssetDatabase.GUIDToAssetPath(guid)
			};
			retVal.name = Path.GetFileNameWithoutExtension(retVal.path);
			return retVal;
		}

		struct ObjectData {
			public string guid;
			public string name;
			public string path;
		}


	}
}
