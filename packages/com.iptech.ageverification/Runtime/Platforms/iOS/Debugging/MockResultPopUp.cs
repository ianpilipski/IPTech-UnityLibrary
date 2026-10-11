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
        private TaskCompletionSource<bool> _onOkClicked;
        private Toggle _rememberChoiceToggle;
        
        private CachedResult.ResultType _mockResultType;
        private AgeRangeResult _mutatedAgeRangeResult;
        private CachedError _mutatedCachedError;
        private VisualElement _autoApproveButtons;
        private Button _autoApproveCancelButton;
        private VisualElement _approveButtons;
        private bool _useAutoApprove;
        private float _autoApproveTimeoutTime;
        private EnumField _resultTypeField;
        private bool _wasAutoApproved;
        private int _panelVersion = -1;

        public static async Task<AgeRangeResult> ShowDialog(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            
            var popUp = Resources.Load<MockResultPopUp>(MOCKRESULT_POPUP_RESOURCE_NAME);
            if(popUp == null) {
                throw new Exception($"Mock result popup resource '{MOCKRESULT_POPUP_RESOURCE_NAME}' not found in Resources folder.");
            }

            var popUpInst = Instantiate<MockResultPopUp>(popUp);
            DontDestroyOnLoad(popUpInst.gameObject);
            try 
            {
                return await popUpInst.Show(AgeRangeDebugSettings.CachedResult, ct);
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
            _onOkClicked = new TaskCompletionSource<bool>();
            _panelRenderer = GetComponent<PanelRenderer>();
            CreateUI();

            _autoApproveTimeoutTime = Time.realtimeSinceStartup + 5f; // 5 seconds from now
        }

        void OnDestroy()
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
            rootElement.Add(_dialogRoot);
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
                        _wasAutoApproved = true;
                        _autoApproveCancelButton.text = "Auto-approved";
                        OnOkClicked();
                    }
                }
            }
        }

        private async Task<AgeRangeResult> Show(CachedResult existingCachedResult, CancellationToken ct)
        {
            if(existingCachedResult != null) {
                _mockResultType = existingCachedResult.ResultKind;
                if(_mockResultType == CachedResult.ResultType.AgeRangeResult) {
                    _mutatedAgeRangeResult = new AgeRangeResult(existingCachedResult.Result);
                } else if(_mockResultType == CachedResult.ResultType.Exception) {
                    _mutatedCachedError = new CachedError(existingCachedResult.Error);
                }
                _useAutoApprove = true;
            }

            _mutatedAgeRangeResult = new AgeRangeResult();
            _mutatedCachedError = new CachedError();
            
            ComposeContent();

            using var reg = ct.Register(() => _onOkClicked.TrySetCanceled());

            await _onOkClicked.Task;

            var newCachedResult = CreateCachedResult();
            if(!_wasAutoApproved) {
                AgeRangeDebugSettings.CachedResult = _rememberChoiceToggle.value ? newCachedResult : null;
            }
            return newCachedResult.EmulateApiResponseAsIfCalled();
        }

        void CreateUI()
        {
            var b = new MockPopupBuilder();
            _dialogRoot = b.BeginFullScreenOverlay();
            
            var dialogBox = b.BeginDialogBox("Age Verification");
            
            _resultTypeField = b.AddEnumProperty("Result Type", _mockResultType);

            b.BeginScrollView();
            _contentArea = b.BeginGroup("Content Area");
            b.EndGroup();
            b.EndScrollView();

            var footer = b.BeginGroup("Footer");
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
            b.EndGroup(); // footer
            b.EndDialogBox(); // age verification
            b.EndFullScreenOverlay(); // dialog overlay

            _resultTypeField.RegisterValueChangedCallback(evt =>
            {
                _mockResultType = (CachedResult.ResultType)evt.newValue;
                ComposeContent();
            });
        }
        
        private void ComposeContent()
        {
            _resultTypeField.SetValueWithoutNotify(_mockResultType);
            
            _contentArea.Clear();
            if (_mockResultType == CachedResult.ResultType.AgeRangeResult)
            {
                _contentArea.Add(new AgeRangeResultUI(_mutatedAgeRangeResult));
            }
            else if (_mockResultType == CachedResult.ResultType.Exception)
            {
                _contentArea.Add(new CachedErrorUI(_mutatedCachedError));
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
            _onOkClicked?.TrySetResult(true);
        }

        private CachedResult CreateCachedResult()
        {
            if(_mockResultType == CachedResult.ResultType.AgeRangeResult) 
            {
                return new CachedResult(_mutatedAgeRangeResult);
            }
            else if(_mockResultType == CachedResult.ResultType.Exception) 
            {
                return new CachedResult(_mutatedCachedError.CreateException());
            }
            throw new Exception("Invalid mock result type selected.");
        }
    }
}