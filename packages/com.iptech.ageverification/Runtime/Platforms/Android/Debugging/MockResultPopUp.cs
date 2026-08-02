using System;
using System.Threading;
using System.Threading.Tasks;
using IPTech.AgeVerification.Debugging;
using UnityEngine;
using UnityEngine.UIElements;

namespace IPTech.AgeVerification.Android.AgeSignals.Debugging
{
    [RequireComponent(typeof(PanelRenderer))]
    public class MockResultPopUp : MonoBehaviour
    {
        private const string MOCK_RESULT_POPUP_RESOURCE_NAME = "com.iptech.ageverification.android.mockresultpopup";
        
        private PanelRenderer _panelRenderer;
        private VisualElement _dialogOverlay;
        private VisualElement _dialogContainer;
        private VisualElement _contentArea;
        private Button _okButton;
        private Action _onClosed;
        private Toggle _rememberChoiceToggle;
        
        private CachedResult.ResultType _mockResultType;
        private MockResult _mockResult;
        private MockError _mockError;
        
        public static async Task<AgeSignalsResult> ShowDialog(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            if (AgeSignalsDebugSettings.CachedResult != null)
            {
                return GetResult(AgeSignalsDebugSettings.CachedResult);
            }
            
            var popUpPrefab = Resources.Load<MockResultPopUp>(MOCK_RESULT_POPUP_RESOURCE_NAME);
            if(popUpPrefab == null) {
                throw new Exception($"Popup prefab resource '{MOCK_RESULT_POPUP_RESOURCE_NAME}' not found in Resources folder.");
            }

            var popupInst = Instantiate(popUpPrefab);
            DontDestroyOnLoad(popupInst.gameObject);
            try 
            {
                return await popupInst.Show(ct);
            } 
            finally 
            {
                if(popupInst != null) {
                    UnityEngine.Object.Destroy(popupInst.gameObject);
                }
            }
        }

        private async Task<AgeSignalsResult> Show(CancellationToken ct)
        {
            _mockResult = new MockResult();
            _mockError = new MockError();

            ComposeContent();
                
            bool isOkClicked = false;
            _onClosed = () => { isOkClicked = true; };

            while (!isOkClicked)
            {
                await Task.Yield();
                if (ct.IsCancellationRequested)
                {
                    ct.ThrowIfCancellationRequested();
                }
            }

            var cachedResult = CreateCachedResult();
            ConditionallyRememberChoice(cachedResult);
            return GetResult(cachedResult);
        }

        private void Awake()
        {
            _panelRenderer = GetComponent<PanelRenderer>();
            CreateDialog();
        }

        void OnEnable()
        {
            _panelRenderer.RegisterUIReloadCallback(UpdateUI);
        }

        void OnDisable()
        {
            _panelRenderer.UnregisterUIReloadCallback(UpdateUI);
        }

        private void UpdateUI(PanelRenderer panelRenderer, VisualElement rootElement)
        {
            rootElement.Add(_dialogOverlay);
        }

        private CachedResult CreateCachedResult()
        {
            if(_mockResultType == CachedResult.ResultType.AgeSignalsResult) 
            {
                var res = _mockResult.ToAgeSignalsResult();
                return new CachedResult(res);
            }
            else if(_mockResultType == CachedResult.ResultType.Exception) 
            {
                var ex = _mockError.CreateException();
                var err = new CachedError(ex);
                return new CachedResult(err);
            }
            throw new Exception("Invalid mock result type selected.");
        }

        private void ConditionallyRememberChoice(CachedResult result)
        {
            if (_rememberChoiceToggle.value)
            {
                AgeSignalsDebugSettings.CachedResult = result;
            }
        }

        private static AgeSignalsResult GetResult(CachedResult cachedResult)
        {
            if(cachedResult.ResultKind == CachedResult.ResultType.AgeSignalsResult) 
            {
                return cachedResult.Result;
            }
            else if(cachedResult.ResultKind == CachedResult.ResultType.Exception) 
            {
                throw new MockError(cachedResult.Error).CreateException();
            }
            throw new Exception("Invalid cached result type.");
        }

        
        private void CreateDialog()
        {
            var b = new MockPopupBuilder();
            _dialogOverlay = b.BeginFullScreenOverlay();
            _dialogContainer = b.BeginDialogBox("Mock Age Signals Result");
            
            var resultType = b.AddEnumProperty("Result Type", _mockResultType);
            
            b.BeginScrollView();
            _contentArea = b.BeginGroup("content");
            b.EndGroup();
            b.EndScrollView();

            var buttons = b.BeginGroup("buttons");
            buttons.style.flexDirection = FlexDirection.Row;
            b.AddButton("Ok", OnOkClicked);
            _rememberChoiceToggle = b.AddToggleProperty("Remember this choice", true);
            b.EndGroup();
            
            b.EndDialogBox();
            b.EndFullScreenOverlay();

            resultType.RegisterValueChangedCallback(evt =>
            {
                _mockResultType = (CachedResult.ResultType)evt.newValue;
                // Rebuild the content area based on selected type
                _contentArea.Clear();
                ComposeContent();
            });
        }

        private void OnOkClicked()
        {
            _onClosed?.Invoke();
        }

        private void ComposeContent()
        {
            if (_mockResultType == CachedResult.ResultType.AgeSignalsResult)
            {
                var ui = new MockResultUI(_mockResult);
                _contentArea.Add(ui.GetRootElement());
            }
            else if (_mockResultType == CachedResult.ResultType.Exception)
            {
                var ui = new MockErrorUI(_mockError);
                _contentArea.Add(ui);
            }
        }

        private void OnDestroy()
        {
            _onClosed = null;
        }
    }
}