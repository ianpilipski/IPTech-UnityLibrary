#if !(UNITY_EDITOR && (DEVELOPMENT_BUILD || QA_BUILD))
#define CONTAINERDEBUGSERVICE_DISABLED
#endif

#if !CONTAINERDEBUGSERVICE_DISABLED

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace IPTech.DebugConsoleService
{
    public class SceneTextureMemoryInspector : EditorWindow {
        const string HELPMESSAGE = 
            "This tool will scan for all referenced textures in the current scene, " +
            "and will then calculate the estimated runtime memory for each texture.\n" + 
            "Playmode may show a different version due to sprite packing and should be " +
            "used for better estimate of runtime memory on device.";

        private TreeViewItem rootTreeViewItem;
        private float allTexturesSizeInMB;

        private Vector2 scrollPosObjectPane;
        private Vector2 scrollPosDetailPane;
        private float currentObjectPaneWidth = 0.5f;
        private Rect cursorChangeRect;
        private bool resize = false;
        private bool calculationNeeded = true;

        private static bool NeedsRepaint;
        private static Texture2D _grayTex;
        private static GUIStyle _labelAlignRight;
        private static GUIStyle _selectedStyle;
        private static GUIStyle _splitter;

        [MenuItem("IPTech/Memory/Scene Memory Inspector...")]
        [MenuItem("Window/IPTech/Scene Memory Inspector")]
        public static void ShowSceneTextureMemoryInspectorWindow() {
            SceneTextureMemoryInspector stmi = (SceneTextureMemoryInspector)EditorWindow.GetWindow<SceneTextureMemoryInspector>();
            stmi.Show();
        }

        void OnEnable() {
            this.titleContent = new GUIContent("Texture Memory");
            HookEditorCallbacks();
        }

        void OnHierarchyChange() {
            calculationNeeded = true;
        }

        void OnDestroy() {
            UnHookEditorCallbacks();
        }

        void OnInspectorUpdate() {
            if(NeedsRepaint) {
                NeedsRepaint = false;
                Repaint();
            }
        }

        private void HookEditorCallbacks() {
            UnHookEditorCallbacks();
            EditorApplication.playModeStateChanged += HandlePlayModeStateChanged;
        }

        private void UnHookEditorCallbacks() {
            EditorApplication.playModeStateChanged -= HandlePlayModeStateChanged;
        }

        private void HandlePlayModeStateChanged(PlayModeStateChange stateChange) {
            if (
                stateChange == PlayModeStateChange.EnteredEditMode ||
                stateChange == PlayModeStateChange.EnteredPlayMode
            ) {
                calculationNeeded = true;
                NeedsRepaint = true;
            }
        }

        private static Texture2D GrayTexture {
            get {
                if(_grayTex==null) {
                    _grayTex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                    _grayTex.SetPixel(0, 0, new Color(1, 1, 1, 0.1f));
                    _grayTex.alphaIsTransparency = true;
                    _grayTex.Apply();
                }
                return _grayTex;
            }
        }

        private static GUIStyle SelectedBackgroundStyle {
            get {
                if(_selectedStyle==null) {
                    _selectedStyle = new GUIStyle();
                    _selectedStyle.normal.background = GrayTexture;
                    _selectedStyle.focused.background = GrayTexture;
                }
                return _selectedStyle;
            }
        }

        private static GUIStyle LabelAlignRight {
            get {
                if(_labelAlignRight==null) {
                    _labelAlignRight = new GUIStyle(EditorStyles.label);
                    _labelAlignRight.alignment = TextAnchor.MiddleRight;
                }
                return _labelAlignRight;
            }
        }

        private static GUIStyle Splitter {
            get {
                if(_splitter==null) {
                    _splitter = new GUIStyle();
                    _splitter.normal.background = EditorGUIUtility.whiteTexture;
                    _splitter.stretchWidth = true;
                    _splitter.margin = new RectOffset(0,0,7,0);
                }
                return _splitter;
            }
        }

        void OnGUI() {
            DrawHeader();
            DrawBody();
        }

        private void DrawHeader() {
            EditorGUILayout.HelpBox(HELPMESSAGE, MessageType.Info);
            EditorGUILayout.BeginHorizontal();
            if(GUILayout.Button("Calculate")) {
                Recalculate();
            }
            GUILayout.Label(calculationNeeded ? "(scene changed needs calculate)" : "");
            GUILayout.FlexibleSpace();
            GUILayout.Label(TotalTextureMemory, LabelAlignRight);
            EditorGUILayout.EndHorizontal();
            DrawSplitter(2, Color.gray);
        }

        private void DrawSplitter(float thickness, Color color) {
            Rect pos = GUILayoutUtility.GetRect(GUIContent.none, Splitter, GUILayout.Height(thickness));
            if (Event.current.type == EventType.Repaint) {
                Color restoreColor = GUI.color;
                GUI.color = color;
                Splitter.Draw(pos, false, false, false, false);
                GUI.color = restoreColor;
            }
        }

        private void DrawBody() {
            Rect r = EditorGUILayout.BeginHorizontal();
            DrawObjectPane();
            ResizeScrollView(r);
            DrawDetailPane();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawObjectPane() {
            scrollPosObjectPane = EditorGUILayout.BeginScrollView(scrollPosObjectPane, GUILayout.Width(position.width * currentObjectPaneWidth));
            if(rootTreeViewItem!=null) {
                for(int i=0;i<rootTreeViewItem.Count;i++) {
                    rootTreeViewItem[i].OnGUI();
                }
            }
            EditorGUILayout.EndScrollView();
        }

        private void DrawDetailPane() {
            scrollPosDetailPane = EditorGUILayout.BeginScrollView(scrollPosDetailPane, EditorStyles.helpBox);
            EditorGUILayout.BeginVertical();
            if(rootTreeViewItem!=null && rootTreeViewItem.SelectedItem!=null) {
                rootTreeViewItem.SelectedItem.DrawDetail();
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }

        private void ResizeScrollView(Rect windowRect){
             if(Event.current.type != EventType.Layout) {
                cursorChangeRect = new Rect((windowRect.width * currentObjectPaneWidth), windowRect.y, 5f, windowRect.height);
             }
             GUILayout.Space(5);
             GUI.DrawTexture(cursorChangeRect,GrayTexture);
             EditorGUIUtility.AddCursorRect(cursorChangeRect,MouseCursor.ResizeHorizontal);
             
             if( Event.current.type == EventType.MouseDown && cursorChangeRect.Contains(Event.current.mousePosition)){
                 resize = true;
             }
             if(resize && Event.current.type == EventType.MouseDrag){
                currentObjectPaneWidth = Event.current.mousePosition.x / windowRect.width;
                Repaint();
             }
             if(Event.current.type == EventType.MouseUp)
                 resize = false;      

            currentObjectPaneWidth = Mathf.Clamp(currentObjectPaneWidth, 0f, 0.9f);        
        }

        private string TotalTextureMemory {
            get {
                return HasCalculated ? string.Format("{0:F} MB", allTexturesSizeInMB) : "??";
            }
        }

        private bool HasCalculated { 
            get { return this.rootTreeViewItem!=null; }
        }

        private void Recalculate() {
            GameObject[] rootObjects = RuntimeInspector.GetAllRootGameObjects();

            allTexturesSizeInMB = TextureMemoryAnalyzer.CalculateMemoryForAllTextures().SizeInMegabytes;
            rootTreeViewItem = new TreeViewItem("root");
            foreach(var obj in rootObjects) {
                rootTreeViewItem.Add(CreateTreeViewItemFromGameObject(obj));
            }
            calculationNeeded = false;
        }

        private TreeViewItem CreateTreeViewItemFromGameObject(GameObject obj) {
            TreeViewItem tvi = new TreeViewItem(obj);
            foreach(Transform tchild in obj.transform) {
                tvi.Add(CreateTreeViewItemFromGameObject(tchild.gameObject));
            }
            return tvi;
        }

        private class TreeViewItem : List<TreeViewItem> {
            const int FOLDOUTICONSIZE = 12;
            const int INDENTSIZE = 16;

            private static int indentLevel;
            private static TreeViewItem selectedItem;
            private static GUIStyle _selectedLabelStyle;

            private GameObject go;
            private string name;
            public bool ShowDetail;
            public string Name { get { return go==null ? name : go.name; } }
            private MemoryData memoryData;
            private int controlID;

            public TreeViewItem(string name) {
                this.name = name;
            }

            public TreeViewItem(GameObject go) : this(go.name) {
                this.go = go;
                Recalculate();
            }

            private void Recalculate() {
                if(go!=null) {
                    Texture[] textures = RuntimeInspector.FindAllTexturesUnder(go);
                    memoryData = (MemoryData)TextureMemoryAnalyzer.CalculateMemory(textures);
                }
            }

            public TreeViewItem SelectedItem {
                get {
                    return selectedItem;
                }
            }

            public void DrawDetail() {
                if(this.go!=null) {
                    DetailViewItem dvi = new DetailViewItem(this.go);
                    dvi.OnGUI();
                }
            }

            private GUIStyle selectedLabelStyle {
                get {
                    if(_selectedLabelStyle==null) {
                        _selectedLabelStyle = new GUIStyle(GUI.skin.label);
                        _selectedLabelStyle.focused.textColor = EditorStyles.foldout.active.textColor;
                        //_selectedLabelStyle.normal.textColor = EditorStyles.foldout.active.textColor;
                    }
                    return _selectedLabelStyle;
                }
            }

            public void OnGUI() {
                UpdateFocusedState();
                Rect r = EditorGUILayout.BeginHorizontal();
                DrawFocusBackground(r);
                GUILayout.Space(INDENTSIZE * indentLevel);
                DrawFoldout();
                GUILayout.FlexibleSpace();
                GUILayout.Label(SizeInMBText());
                EditorGUILayout.EndHorizontal();
                DrawFoldoutDetail();
            }

            private void UpdateFocusedState() {
                if(Event.current.type==EventType.Layout) {
                    bool focused = GUIUtility.keyboardControl == controlID;
                    if(focused && (selectedItem!=this)) {
                        selectedItem = this;
                    }
                }
            }

            private void DrawFocusBackground(Rect r) {
                if(selectedItem == this) {
                    GUI.DrawTexture(r, GrayTexture);
                }
            }

            private void DrawFoldout() {
                ShowDetail = FoldOutControl.Foldout(ShowDetail, Name, Count > 0, out controlID);
            }

            private void DrawFoldoutDetail() {
                if(ShowDetail) {
                    indentLevel++;
                    for(int i = 0; i < Count; i++) {
                        this[i].OnGUI();
                    }
                    indentLevel--;
                }
            }

            private string SizeInMBText() {
                if(memoryData!=null) {
                    return memoryData.SizeInMegabytes.ToString("F") + " MB";
                }
                return "";
            }
        }

        private class DetailViewItem {
            private static GUIStyle _labelStyle;

            private Dictionary<MemoryData, Texture> memoryData;
            private List<MemoryData> sortedKeys;

            public DetailViewItem(GameObject go) {
                //this.go = go;
                GenerateDetails(go);
            }

            private void GenerateDetails(GameObject go) {
                memoryData = new Dictionary<MemoryData, Texture>();
                Texture[] textures = RuntimeInspector.FindAllTexturesUnder(go);
                foreach(Texture tex in textures) {
                    memoryData.Add(TextureMemoryAnalyzer.CalculateMemory(tex), tex);
                }
                sortedKeys = memoryData.Keys.ToList();
                sortedKeys.Sort((x,y) => (int)(y.SizeInBytes - x.SizeInBytes));
            }

            private static GUIStyle LabelStyle {
                get {
                    if(_labelStyle==null) {
                        _labelStyle = new GUIStyle(GUI.skin.label);
                        _labelStyle.alignment = TextAnchor.MiddleRight;
                        _labelStyle.stretchWidth = true;
                        _labelStyle.fixedWidth = 0;
                    }
                    return _labelStyle;
                }
            }

            public void DrawHeader() {
                EditorGUILayout.BeginVertical();
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

                GUILayout.Label("Texture");
                GUILayout.FlexibleSpace();
                GUILayout.Label("bytes", LabelStyle);
                GUILayout.Label("megabytes", LabelStyle);

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }

            public void OnGUI() {
                DrawHeader();
                EditorGUILayout.BeginVertical();
                for(int i=0; i<sortedKeys.Count; i++) {
                    MemoryData md = sortedKeys[i];
                    Rect startRect = EditorGUILayout.BeginVertical( (i%2)==0 ? SelectedBackgroundStyle : GUIStyle.none);
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label(memoryData[md].name);
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(md.SizeInBytes.ToString(), LabelStyle);
                    GUILayout.Label(md.SizeInMegabytes.ToString("F") + " MB", LabelStyle);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    if(GUI.Button(startRect,"", GUIStyle.none)) {
                        Selection.activeObject = memoryData[md];
                    };
                }
                EditorGUILayout.EndVertical();
            }
        }
    }
}

#endif

