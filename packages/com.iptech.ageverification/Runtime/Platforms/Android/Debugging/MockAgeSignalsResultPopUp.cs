using System;
using System.Threading;
using System.Threading.Tasks;
using IPTech.AgeVerification.Debugging;
using UnityEngine;
using UnityEngine.UIElements;

namespace IPTech.AgeVerification.Android.AgeSignals.Debugging
{
    [RequireComponent(typeof(PanelRenderer))]
    public class MockAgeSignalsResultPopUp : MonoBehaviour
    {
        private const string MOCK_RESULT_POPUP_RESOURCE_NAME = "com.iptech.ageverification.android.mockagesignalsresultpopup";
        
        private PanelRenderer _panelRenderer;
        private VisualElement _dialogOverlay;
        private VisualElement _contentArea;
        private TaskCompletionSource<bool> _onOkClicked;
        private Toggle _rememberChoiceToggle;
        
        private CachedAgeSignalsResult.ResultType _mockResultType;
        private AgeSignalsResult _mutatedAgeSignalsResult;
        private CachedError _mutatedCachedError;
        private VisualElement _autoApproveButtons;
        private VisualElement _approveButtons;
        private bool _useAutoApprove;
        private Button _autoApproveCancelButton;
        private float _autoApproveTimeoutTime;
        private bool _wasAutoApproved;
        private int _panelVersion = -1;

        public static async Task<AgeSignalsResult> ShowDialog(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            var popUpPrefab = Resources.Load<MockAgeSignalsResultPopUp>(MOCK_RESULT_POPUP_RESOURCE_NAME);
            if(popUpPrefab == null) {
                throw new Exception($"Popup prefab resource '{MOCK_RESULT_POPUP_RESOURCE_NAME}' not found in Resources folder.");
            }

            var popupInst = Instantiate(popUpPrefab);
            DontDestroyOnLoad(popupInst.gameObject);
            try 
            {
                return await popupInst.Show(AgeSignalsDebugSettings.CachedAgeSignalsResult, ct);
            } 
            finally 
            {
                if(popupInst != null) {
                    UnityEngine.Object.Destroy(popupInst.gameObject);
                }
            }
        }

        private async Task<AgeSignalsResult> Show(CachedAgeSignalsResult cachedResult, CancellationToken ct)
        {
            if(cachedResult != null) {
                _mockResultType = cachedResult.ResultKind;
                if(_mockResultType == CachedAgeSignalsResult.ResultType.AgeSignalsResult) {
                    _mutatedAgeSignalsResult = new AgeSignalsResult(cachedResult.Result);
                } else if(_mockResultType == CachedAgeSignalsResult.ResultType.Exception) {
                    _mutatedCachedError = new CachedError(cachedResult.Error);
                }
                _useAutoApprove = true;
            }

            _mutatedAgeSignalsResult ??= new AgeSignalsResult();
            _mutatedCachedError ??= new CachedError();

            ComposeContent();
                
            using var reg = ct.Register(() => _onOkClicked.TrySetCanceled());

            await _onOkClicked.Task;

            var newCachedResult = CreateCachedResult();
            if(!_wasAutoApproved)
            {
                AgeSignalsDebugSettings.CachedAgeSignalsResult = _rememberChoiceToggle.value ? newCachedResult : null;
            }
            return newCachedResult.EmulateApiResponseAsIfCalled();
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

        void OnEnable()
        {
            _panelRenderer.RegisterUIReloadCallback(UpdateUI);
        }

        void OnDisable()
        {
            _panelRenderer.UnregisterUIReloadCallback(UpdateUI);
        }

        private void UpdateUI(PanelRenderer panelRenderer, VisualElement rootElement, int version)
        {
            if(_panelVersion == version) return;
            _panelVersion = version;
            rootElement.Add(_dialogOverlay);
        }

        private CachedAgeSignalsResult CreateCachedResult()
        {
            if(_mockResultType == CachedAgeSignalsResult.ResultType.AgeSignalsResult) 
            {
                return new CachedAgeSignalsResult(_mutatedAgeSignalsResult);
            }
            else if(_mockResultType == CachedAgeSignalsResult.ResultType.Exception) 
            {
                return new CachedAgeSignalsResult(_mutatedCachedError);
            }
            throw new Exception("Invalid mock result type selected.");
        }

        
        private void CreateDialog()
        {
            var b = new MockPopupBuilder();
            _dialogOverlay = b.BeginFullScreenOverlay();
            b.BeginDialogBox("Mock Age Signals Result");
            
            var resultType = b.AddEnumProperty("Result Type", _mockResultType);
            
            b.BeginScrollView();
            _contentArea = b.BeginGroup("content");
            b.EndGroup();
            b.EndScrollView();

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

            resultType.RegisterValueChangedCallback(evt =>
            {
                _mockResultType = (CachedAgeSignalsResult.ResultType)evt.newValue;
                ComposeContent();
            });
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

        private void ComposeContent()
        {
            _contentArea.Clear();
            if (_mockResultType == CachedAgeSignalsResult.ResultType.AgeSignalsResult)
            {
                _contentArea.Add(new AgeSignalsResultUI(_mutatedAgeSignalsResult, false));
            }
            else if (_mockResultType == CachedAgeSignalsResult.ResultType.Exception)
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
    }
}