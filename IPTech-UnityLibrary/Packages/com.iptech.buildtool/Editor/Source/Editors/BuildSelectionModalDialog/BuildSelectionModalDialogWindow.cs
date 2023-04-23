using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Collections.Generic;

namespace IPTech.BuildTool.Internal {
    public class BuildSelectionModalDialogWindow : EditorWindow {
        
        [InitializeOnLoadMethod]
        static void InitializeOnLoad() {
            UnityEditor.BuildPlayerWindow.RegisterGetBuildPlayerOptionsHandler((bpo) => {
                if(!BuildToolsSettings.instance.EnableBuildWindowIntegration) {
                    return BuildPlayerWindow.DefaultBuildMethods.GetBuildPlayerOptions(bpo);
                }
                return bpo;
            });
            UnityEditor.BuildPlayerWindow.RegisterBuildPlayerHandler((bpo) => {
                if(BuildToolsSettings.instance.EnableBuildWindowIntegration) {
                    ShowDialog();
                } else {
                    BuildPlayerWindow.DefaultBuildMethods.BuildPlayer(bpo);
                }
            });
        }

        static void ShowDialog() {
            BuildSelectionModalDialogWindow wnd = ScriptableObject.CreateInstance<BuildSelectionModalDialogWindow>();
            wnd.titleContent = new GUIContent("Select Build Config");
            var pos = wnd.position;
            pos.width = 640;
            pos.height = 480;
            wnd.position = pos;
            
            wnd.ShowModalUtility();
        }

        public void CreateGUI() {
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;
            
            // Import UXML
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.iptech.buildtool/Editor/Source/Editors/BuildSelectionModalDialog/BuildSelectionModalDialogWindow.uxml");
            VisualElement visTree = visualTree.Instantiate();
            visTree.StretchToParentSize();
            root.Add(visTree);
            
            PopulateListView();
            HookDefaultBuildButton();
            HookCancelButton();
        }

        void PopulateListView() {
            var allBuildConfigGuids = AssetDatabase.FindAssets("t:PlayerBuildConfig");
            var allBuildConfigs = new List<PlayerBuildConfig>();
            foreach(var guid in allBuildConfigGuids) {
                var bc = AssetDatabase.LoadAssetAtPath<PlayerBuildConfig>(AssetDatabase.GUIDToAssetPath(guid));
                if(bc.CanBuildWithCurrentEditorBuildTarget()) {
                    allBuildConfigs.Add(bc);
                }
            }

            Func<VisualElement> makeItem = () => new Button();
            Action<VisualElement, int> bindItem = (e, i) => {
                var b = (e as Button);
                b.text = allBuildConfigs[i].name;
                b.clicked += () => HandleBuildButtonClicked(i);
            };

            var listView = rootVisualElement.Q<ListView>();
            listView.makeItem = makeItem;
            listView.bindItem = bindItem;
            listView.itemsSource = allBuildConfigs;
            listView.selectionType = SelectionType.Single;

            /*
            listView.onItemsChosen += (_) => { Debug.Log("item chosen"); };
            listView.onSelectionChange += (chosenObjects) => {
                try {
                    Debug.Log("whaa");
                    foreach(var obj in chosenObjects) {
                        PlayerBuildConfig bc = (PlayerBuildConfig)obj;
                        BuildToolEditor.PerformBuild(bc);
                        break;
                    }
                } catch(Exception e) {
                    Debug.LogException(e);
                }
            };
            */

            void HandleBuildButtonClicked(int index) {
                Close();
                var bc = allBuildConfigs[index];
                EditorApplication.delayCall += () => {
                    BuildToolEditor.PerformBuild(bc);
                };
            }
        }

        

        void HookDefaultBuildButton() {
            var button = rootVisualElement.Q<Button>("buttonDefaultBuild");
            button.clicked += () => {
                try {
                    BuildPlayerOptions bpo = new BuildPlayerOptions();
                    try {
                        bpo = UnityEditor.BuildPlayerWindow.DefaultBuildMethods.GetBuildPlayerOptions(bpo);
                    } catch(UnityEditor.BuildPlayerWindow.BuildMethodException) {
                        Close();
                    }
                    UnityEditor.BuildPlayerWindow.DefaultBuildMethods.BuildPlayer(bpo);
                } finally {
                    Close();
                }
            };
        }

        void HookCancelButton() {
            var button = rootVisualElement.Q<Button>("buttonCancel");
            button.clicked += Close;
        }
    }
}