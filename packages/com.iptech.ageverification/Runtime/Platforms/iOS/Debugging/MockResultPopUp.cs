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
        private VisualElement _dialogRoot;
        private VisualElement _contentArea;
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
            rootElement.Add(_dialogRoot);
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
            if (_rememberChoiceToggle.value)
            {
                AgeRangeDebugSettings.CachedResult = cachedResult;
            }
            return GetResult(cachedResult);
        }

        void CreateUI()
        {
            var b = new MockPopupBuilder();
            _dialogRoot = b.BeginFullScreenOverlay();
            
            var dialogBox = b.BeginDialogBox("Age Verification");
            
            var resultTypeField = b.AddEnumProperty("Result Type", _mockResultType);

            b.BeginScrollView();
            _contentArea = b.BeginGroup("Content Area");
            b.EndGroup();
            b.EndScrollView();

            var footer = b.BeginGroup("Footer");
            footer.style.flexDirection = FlexDirection.Row;
            b.AddButton("Confirm", CloseDialog);
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
                return new CachedResult(_mockResult.ToAgeRangeResult());
            }
            else if(_mockResultType == CachedResult.ResultType.Exception) 
            {
                return new CachedResult(_mockError.CreateException());
            }
            throw new Exception("Invalid mock result type selected.");
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

        
        private void ComposeContent()
        {
            if (_mockResultType == CachedResult.ResultType.AgeRangeResult)
            {
                _contentArea.Clear();
                _contentArea.Add(new MockResultUI(_mockResult));
            }
            else if (_mockResultType == CachedResult.ResultType.Exception)
            {
                var ui = new MockErrorUI(_mockError);
                _contentArea.Clear();
                _contentArea.Add(ui);
            }
        }

        private void CloseDialog()
        {
            _onClosed?.Invoke();
        }
    }
}