using System;
using System.Threading;
using System.Threading.Tasks;
using IPTech.AgeVerification.Debugging;
using UnityEngine;
using UnityEngine.UIElements;

namespace IPTech.AgeVerification.iOS.Debugging
{
    [RequireComponent(typeof(PanelRenderer))]
    public class MockIsEligiblePopUp : MonoBehaviour
    {
        private const string MOCK_IS_ELIGIBLE_POPUP_PREFAB_RESOURCE_NAME = "com.iptech.ageverification.ios.mockiseligiblepopup";
        
        [SerializeField]
        private string _title = "Mock Is Eligible";
        
        private PanelRenderer _panelRenderer;
        private VisualElement _dialogOverlay;
        private VisualElement _dialogContainer;
        private VisualElement _contentArea;
        private Label _dialogTitle;
        private Button _okButton;
        private Action _onClosed;
        private Toggle _rememberChoiceToggle;
        
        private CachedIsEligibleResult.ResultType _mockResultType;
        private MockIsEligible _mockIsEligible;
        private MockIsEligibleError _mockError;
        
        public static async Task<bool> ShowDialog(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            if (AgeRangeDebugSettings.CachedIsEligibleForAgeFeaturesResult != null)
            {
                return GetResult(AgeRangeDebugSettings.CachedIsEligibleForAgeFeaturesResult);
            }

            var mockPopUpPrefab = Resources.Load<MockIsEligiblePopUp>(MOCK_IS_ELIGIBLE_POPUP_PREFAB_RESOURCE_NAME);
            if(mockPopUpPrefab == null) {
                throw new Exception($"MockPopUp resource '{MOCK_IS_ELIGIBLE_POPUP_PREFAB_RESOURCE_NAME}' not found in Resources folder.");
            }

            var mockPopUpInst = Instantiate(mockPopUpPrefab);
            DontDestroyOnLoad(mockPopUpInst.gameObject);

            try 
            {
                return await mockPopUpInst.Show(ct);
            }
            finally
            {
                if (mockPopUpInst != null)
                {
                    UnityEngine.Object.Destroy(mockPopUpInst.gameObject);
                }
            }
        }

        private void Awake()
        {
            _panelRenderer = GetComponent<PanelRenderer>();
            CreateDialog();
        }

        public async Task<bool> Show(CancellationToken ct)
        {
            _title = "Mock Age Features Eligibility";
            _mockIsEligible = new MockIsEligible();
            _mockError = new MockIsEligibleError();
            _mockResultType = CachedIsEligibleResult.ResultType.IsEligibleResult;
    
            bool isOkClicked = false;
            _onClosed = () => { isOkClicked = true; };

            ComposeContent();

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
            rootElement.Clear();
            rootElement.Add(_dialogOverlay);
        }

        private void CreateDialog()
        {
            var b = new MockPopupBuilder();
            _dialogOverlay = b.BeginFullScreenOverlay();
            
            
            b.BeginDialogBox(_title);
            var resultTypeField = b.AddEnumProperty("Result Type", _mockResultType);
            _contentArea = b.BeginGroup("content-area");
            b.EndGroup();
            
            var buttons = b.BeginGroup("buttons");
            buttons.style.flexDirection = FlexDirection.Row;
            _okButton = b.AddButton("Confirm", OnOkClicked);
            _rememberChoiceToggle = b.AddToggleProperty("Remember this choice", true);
            b.EndGroup(); // buttons

            b.EndDialogBox();
            b.EndFullScreenOverlay();

            resultTypeField.RegisterValueChangedCallback(evt => 
            {
                if (_mockResultType != (CachedIsEligibleResult.ResultType)evt.newValue)
                {
                    _mockResultType = (CachedIsEligibleResult.ResultType)evt.newValue;
                    ComposeContent();
                }
            });
        }

        private void ComposeContent()
        {
            _contentArea.Clear();
            if (_mockResultType == CachedIsEligibleResult.ResultType.IsEligibleResult)
            {
                var ui = new MockIsEligibleUI(_mockIsEligible, true);
                _contentArea.Add(ui);
            }
            else if (_mockResultType == CachedIsEligibleResult.ResultType.Exception)
            {
                var ui = new MockIsEligibleErrorUI(_mockError);
                _contentArea.Add(ui);
            }
        }

        private void OnOkClicked()
        {
            _onClosed?.Invoke();
        }

        private CachedIsEligibleResult CreateCachedResult()
        {
            if(_mockResultType == CachedIsEligibleResult.ResultType.IsEligibleResult) 
            {
                return new CachedIsEligibleResult(_mockIsEligible.IsEligible);
            }
            else if(_mockResultType == CachedIsEligibleResult.ResultType.Exception) 
            {
                var ex = _mockError.CreateException();
                var err = new CachedIsEligibleError(ex);
                return new CachedIsEligibleResult(err);
            }
            throw new Exception("Invalid mock result type selected.");
        }

        private void ConditionallyRememberChoice(CachedIsEligibleResult result)
        {
            if (_rememberChoiceToggle.value)
            {
                AgeRangeDebugSettings.CachedIsEligibleForAgeFeaturesResult = result;
            }
        }

        private static bool GetResult(CachedIsEligibleResult cachedResult)
        {
            if(cachedResult.ResultKind == CachedIsEligibleResult.ResultType.IsEligibleResult) 
            {
                return cachedResult.Result;
            }
            else if(cachedResult.ResultKind == CachedIsEligibleResult.ResultType.Exception) 
            {
                throw new MockIsEligibleError(cachedResult.Error).CreateException();
            }
            throw new Exception("Invalid cached result type.");
        }

        private void OnDestroy()
        {
            _onClosed = null;
        }
    }
}
