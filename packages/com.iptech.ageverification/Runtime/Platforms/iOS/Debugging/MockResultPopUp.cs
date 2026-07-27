using System;
using System.Threading;
using System.Threading.Tasks;
using IPTech.AgeVerification.Debugging;
using UnityEngine;
using UnityEngine.UIElements;

namespace IPTech.AgeVerification.iOS.Debugging
{
    [RequireComponent(typeof(PanelRenderer))]
    public class MockResultPopUp : MonoBehaviour
    {
        private const string MOCKRESULT_POPUP_RESOURCE_NAME = "com.iptech.ageverification.ios.mockresultpopup";
        
        private PanelRenderer _panelRenderer;
        private VisualElement _dialogOverlay;
        private VisualElement _contentArea;
        private Button _okButton;
        private Action _onClosed;
        private Toggle _rememberChoiceToggle;
        
        private CachedResult.ResultType _mockResultType;
        private MockResult _mockResult;
        private MockError _mockError;
        
        public static async Task<AgeRangeResult> ShowDialog(int requiredMinAge, CancellationToken ct, int additionalMinAge1 = 0, int additionalMinAge2 = 0)
        {
            ct.ThrowIfCancellationRequested();
            if (AgeRangeDebugSettings.CachedResult != null)
            {
                return GetResult(AgeRangeDebugSettings.CachedResult);
            }

            var popUp = Resources.Load<MockResultPopUp>(MOCKRESULT_POPUP_RESOURCE_NAME);
            if(popUp == null) {
                throw new Exception($"Mock result popup resource '{MOCKRESULT_POPUP_RESOURCE_NAME}' not found in Resources folder.");
            }

            var popUpInst = Instantiate<MockResultPopUp>(popUp);
            DontDestroyOnLoad(popUpInst.gameObject);
            try 
            {
                return await popUpInst.Show(ct);
            }
            finally
            {
                if (popUpInst != null)
                {
                    UnityEngine.Object.Destroy(popUpInst.gameObject);
                }
            }
        }

        void Awake()
        {
            _panelRenderer = GetComponent<PanelRenderer>();
            CreateUI();
        }

        public async Task<AgeRangeResult> Show(CancellationToken ct)
        {
            _mockResult = new MockResult();
            _mockError = new MockError();
            
            var isOkClicked = false;
            _onClosed = () => { isOkClicked = true; };

            ComposeContent();

            while(!isOkClicked)
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

        void CreateUI()
        {
            var b = new MockPopupBuilder();
            _dialogOverlay = b.BeginFullScreenOverlay();
            
            var dialogBox = b.BeginDialogBox("Age Verification");
            
            var resultTypeField = b.AddEnumProperty("Result Type", _mockResultType);

            b.BeginScrollView();
            _contentArea = b.BeginGroup("Content Area");
            b.EndGroup();
            b.EndScrollView();

            var footer = b.BeginGroup("Footer");
            footer.style.flexDirection = FlexDirection.Row;
            b.AddButton("Confirm", OnOkClicked);
            _rememberChoiceToggle = b.AddToggleProperty("Remember My Choice", true);
            b.EndGroup(); // footer
            b.EndDialogBox(); // age verification
            b.EndFullScreenOverlay(); // dialog overlay

            resultTypeField.RegisterValueChangedCallback(evt =>
            {
                _mockResultType = (CachedResult.ResultType)evt.newValue;
                ComposeContent();
            });
        }

        private CachedResult CreateCachedResult()
        {
            if(_mockResultType == CachedResult.ResultType.AgeRangeResult) 
            {
                var res = _mockResult.CreateResult();
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
                AgeRangeDebugSettings.CachedResult = result;
            }
        }

        private static AgeRangeResult GetResult(CachedResult cachedResult)
        {
            if(cachedResult.ResultKind == CachedResult.ResultType.AgeRangeResult) 
            {
                return cachedResult.Result;
            }
            else if(cachedResult.ResultKind == CachedResult.ResultType.Exception) 
            {
                throw new MockError(cachedResult.Error).CreateException();
            }
            throw new Exception("Invalid cached result type.");
        }

        void OnEnable()
        {
            _panelRenderer.RegisterUIReloadCallback(OnUIReload);
        }

        void OnDisable() 
        {
            _panelRenderer.UnregisterUIReloadCallback(OnUIReload);
        }

        void OnUIReload(PanelRenderer panelRenderer, VisualElement rootElement)
        {
            rootElement.Add(_dialogOverlay);
        }

        private void ComposeContent()
        {
            if (_mockResultType == CachedResult.ResultType.AgeRangeResult)
            {
                var ui = new MockResultUI(_mockResult);
                _contentArea.Clear();
                _contentArea.Add(ui.GetRootElement());
            }
            else if (_mockResultType == CachedResult.ResultType.Exception)
            {
                var ui = new MockErrorUI(_mockError);
                _contentArea.Clear();
                _contentArea.Add(ui);
            }
        }

        private void OnOkClicked()
        {
            CloseDialog();
        }

        private void OnOverlayClicked(ClickEvent evt)
        {
            // Close dialog when clicking on overlay background
            CloseDialog();
        }

        private void OnDialogClicked(ClickEvent evt)
        {
            // Stop propagation to prevent overlay click
            evt.StopImmediatePropagation();
        }

        private void CloseDialog()
        {
            _onClosed?.Invoke();
        }


        private void OnDestroy()
        {
            _onClosed = null;
        }
    }
}