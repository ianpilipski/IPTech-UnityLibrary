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
        
        private PanelRenderer _panelRenderer;
        private VisualElement _dialogOverlay;
        private EnumField _resultTypeField;
        private VisualElement _contentArea;
        private TaskCompletionSource<bool> _onOkClicked;
        private Toggle _rememberChoiceToggle;
        
        private CachedIsEligibleResult.ResultType _mockResultType;
        private IsEligibleResult _mutatedIsEligibleResult;
        private CachedIsEligibleError _mutatedCachedError;
        private bool _useAutoApprove;
        private float _autoApproveTimeoutTime;
        private VisualElement _autoApproveButtons;
        private Button _autoApproveCancelButton;
        private VisualElement _approveButtons;
        private bool _wasAutoApproved;
        private int _panelVersion = -1;

        public static async Task<bool> ShowDialog(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            
            var mockPopUpPrefab = Resources.Load<MockIsEligiblePopUp>(MOCK_IS_ELIGIBLE_POPUP_PREFAB_RESOURCE_NAME);
            if(mockPopUpPrefab == null) {
                throw new Exception($"MockPopUp resource '{MOCK_IS_ELIGIBLE_POPUP_PREFAB_RESOURCE_NAME}' not found in Resources folder.");
            }

            var mockPopUpInst = Instantiate(mockPopUpPrefab);
            DontDestroyOnLoad(mockPopUpInst.gameObject);
            try 
            {
                return await mockPopUpInst.Show(AgeRangeDebugSettings.CachedIsEligibleForAgeFeaturesResult, ct);
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
            _onOkClicked = new TaskCompletionSource<bool>();
            _panelRenderer = GetComponent<PanelRenderer>();
            CreateDialog();

            _autoApproveTimeoutTime = Time.realtimeSinceStartup + 5f; // 5 seconds from now
        }

        private void OnDestroy()
        {
            _onOkClicked.TrySetException(new TaskCanceledException("Popup was destroyed."));
        }

        void OnEnable()
        {
            _panelRenderer.RegisterUIReloadCallback(OnUIReload);
        }

        void OnDisable() 
        {
            _panelRenderer.UnregisterUIReloadCallback(OnUIReload);
        }

        void OnUIReload(PanelRenderer panelRenderer, VisualElement rootElement, int version)
        {
            if(_panelVersion == version) return;
            _panelVersion = version;
            rootElement.Clear();
            rootElement.Add(_dialogOverlay);
        }

        void Update()
        {
            if(_useAutoApprove && !_wasAutoApproved)
            {
                if(_autoApproveCancelButton != null)
                {
                    var secondsRemaining = _autoApproveTimeoutTime - Time.realtimeSinceStartup;
                    var secondsRemainingString = secondsRemaining.ToString("F1");
                    _autoApproveCancelButton.text = $"Cancel Auto-approve in {secondsRemainingString}s";
                    if(Time.realtimeSinceStartup >= _autoApproveTimeoutTime)
                    {
                        _autoApproveCancelButton.text = "Auto-approved";
                        _wasAutoApproved = true;
                        OnOkClicked();
                    }
                }
            }
        }

        private async Task<bool> Show(CachedIsEligibleResult cachedResult, CancellationToken ct)
        {
            if(cachedResult != null) {
                _mockResultType = cachedResult.ResultKind;
                if(_mockResultType == CachedIsEligibleResult.ResultType.IsEligibleResult) {
                    _mutatedIsEligibleResult = new IsEligibleResult(cachedResult.Result);
                } else if(_mockResultType == CachedIsEligibleResult.ResultType.Exception) {
                    _mutatedCachedError = new CachedIsEligibleError(cachedResult.Error);
                }
                _useAutoApprove = true;
            }

            _mutatedIsEligibleResult ??= new IsEligibleResult();
            _mutatedCachedError ??= new CachedIsEligibleError();
            
            ComposeContent();

            using var reg = ct.Register(() => _onOkClicked.TrySetCanceled());
            
            await _onOkClicked.Task;

            var newCachedResult = CreateCachedResult();
            if(!_wasAutoApproved) {
                AgeRangeDebugSettings.CachedIsEligibleForAgeFeaturesResult = _rememberChoiceToggle.value ? newCachedResult : null;
            }
            return newCachedResult.EmulateApiResponseAsIfCalled();
        }

        private void CreateDialog()
        {
            var b = new MockPopupBuilder();
            _dialogOverlay = b.BeginFullScreenOverlay();
            
            
            b.BeginDialogBox("Mock Age Features Eligibility");
            _resultTypeField = b.AddEnumProperty("Result Type", _mockResultType);
            _contentArea = b.BeginGroup("content-area");
            b.EndGroup();
            
            _autoApproveButtons = b.BeginGroup("auto-approve");
            _autoApproveButtons.style.flexDirection = FlexDirection.Row;
            b.AddButton("Ok", OnOkClicked);
            _autoApproveCancelButton = b.AddButton("Cancel", OnAutoApproveCancelled);
            b.EndGroup();
            
            _approveButtons = b.BeginGroup("buttons");
            _approveButtons.style.flexDirection = FlexDirection.Row;
            b.AddButton("Ok", OnOkClicked);
            _rememberChoiceToggle = b.AddToggleProperty("Remember this choice", true);
            b.EndGroup();

            b.EndDialogBox();
            b.EndFullScreenOverlay();

            _resultTypeField.RegisterValueChangedCallback(evt => 
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
             _resultTypeField.SetValueWithoutNotify(_mockResultType);
             
            _contentArea.Clear();
            if (_mockResultType == CachedIsEligibleResult.ResultType.IsEligibleResult)
            {
                _contentArea.Add(new IsEligibleResultUI(_mutatedIsEligibleResult, false));
            }
            else if (_mockResultType == CachedIsEligibleResult.ResultType.Exception)
            {
                _contentArea.Add(new CachedIsEligibleErrorUI(_mutatedCachedError, false));
            }

            if(_useAutoApprove)
            {
                _autoApproveButtons.style.display = DisplayStyle.Flex;
                _approveButtons.style.display = DisplayStyle.None;
                _contentArea.SetEnabled(false);
            }
            else
            {
                _autoApproveButtons.style.display = DisplayStyle.None;
                _approveButtons.style.display = DisplayStyle.Flex;
                _contentArea.SetEnabled(true);
            }
        }

        private void OnAutoApproveCancelled()
        {
            if(_wasAutoApproved) return;
            _useAutoApprove = false;
            ComposeContent();
        }

        private void OnOkClicked()
        {
            _onOkClicked.TrySetResult(true);
        }

        private CachedIsEligibleResult CreateCachedResult()
        {
            if(_mockResultType == CachedIsEligibleResult.ResultType.IsEligibleResult) 
            {
                return new CachedIsEligibleResult(_mutatedIsEligibleResult);
            }
            else if(_mockResultType == CachedIsEligibleResult.ResultType.Exception) 
            {
                return new CachedIsEligibleResult(_mutatedCachedError);
            }
            throw new Exception("Invalid mock result type selected.");
        }
    }
}
