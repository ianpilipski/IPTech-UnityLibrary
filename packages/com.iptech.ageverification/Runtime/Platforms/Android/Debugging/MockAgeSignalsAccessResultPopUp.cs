using System;
using System.Threading;
using System.Threading.Tasks;
using IPTech.AgeVerification.Debugging;
using UnityEngine;
using UnityEngine.UIElements;

namespace IPTech.AgeVerification.Android.AgeSignals.Debugging
{
    [RequireComponent(typeof(PanelRenderer))]
    public class MockAgeSignalsAccessResultPopUp : MonoBehaviour
    {
        private const string MOCK_RESULT_POPUP_RESOURCE_NAME = "com.iptech.ageverification.android.mockagesignalsaccessresultpopup";
        
        private PanelRenderer _panelRenderer;
        private VisualElement _root;
        private float _autoApproveTimeoutTime;
        private VisualElement _contentArea;
        private TaskCompletionSource<bool> _onOkClicked;
        private Toggle _rememberChoiceToggle;
        
        private CachedAgeSignalsAccessResult.ResultType _mockResultType;
        private AgeSignalsAccessResult _mutatedAgeSignalsAccessResult;
        private CachedError _mutatedCachedError;
        private VisualElement _autoApproveButtons;
        private VisualElement _approveButtons;
        private Button _autoApproveCancelButton;
        private bool _useAutoApprove;
        private bool _wasAutoApproved;

        public static async Task<AgeSignalsAccessResult> ShowDialog(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            var popUpPrefab = Resources.Load<MockAgeSignalsAccessResultPopUp>(MOCK_RESULT_POPUP_RESOURCE_NAME);
            if(popUpPrefab == null) {
                throw new Exception($"Popup prefab resource '{MOCK_RESULT_POPUP_RESOURCE_NAME}' not found in Resources folder.");
            }

            var popupInst = Instantiate(popUpPrefab);
            DontDestroyOnLoad(popupInst.gameObject);
            try 
            {
                return await popupInst.Show(AgeSignalsDebugSettings.CachedAgeSignalsAccessResult, ct);
            } 
            finally 
            {
                if(popupInst != null) {
                    UnityEngine.Object.Destroy(popupInst.gameObject);
                }
            }
        }

        private async Task<AgeSignalsAccessResult> Show(CachedAgeSignalsAccessResult cachedResult, CancellationToken ct)
        {
            if(cachedResult != null)
            {
                _mockResultType = cachedResult.ResultKind;
                if(cachedResult.ResultKind == CachedAgeSignalsAccessResult.ResultType.AgeSignalsAccessResult)
                {
                    _mutatedAgeSignalsAccessResult = new AgeSignalsAccessResult(cachedResult.Result);
                }
                else if(cachedResult.ResultKind == CachedAgeSignalsAccessResult.ResultType.Exception)
                {
                    _mutatedCachedError = new CachedError(cachedResult.Error);
                }
                _useAutoApprove = true;
            }

            _mutatedAgeSignalsAccessResult ??= new AgeSignalsAccessResult();
            _mutatedCachedError ??= new CachedError();

            CreateDialog();
            ComposeContent();
                
            using var reg = ct.Register(() => _onOkClicked.TrySetCanceled());

            await _onOkClicked.Task;

            var newCachedResult = CreateCachedResult();
            if(!_wasAutoApproved) {
                AgeSignalsDebugSettings.CachedAgeSignalsAccessResult = _rememberChoiceToggle.value ? newCachedResult : null;
            }
            return newCachedResult.EmulateApiResponseAsIfCalled();
        }

        private void Awake()
        {
            _onOkClicked = new TaskCompletionSource<bool>();
            _panelRenderer = GetComponent<PanelRenderer>();
            _root = new VisualElement();
            _root.name = "age-signals-access-result-popup";
            

            _autoApproveTimeoutTime = Time.realtimeSinceStartup + 5f; // 5 seconds from now
        }

        private void OnDestroy()
        {
            _onOkClicked.TrySetException(new TaskCanceledException("Popup was destroyed."));
        }

        void OnEnable()
        {
            _panelRenderer.RegisterUIReloadCallback(UpdateUI);
        }

        void OnDisable()
        {
            _panelRenderer.UnregisterUIReloadCallback(UpdateUI);
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

        private void UpdateUI(PanelRenderer panelRenderer, VisualElement rootElement)
        {
            rootElement.Add(_root);
        }

        private CachedAgeSignalsAccessResult CreateCachedResult()
        {
            if(_mockResultType == CachedAgeSignalsAccessResult.ResultType.AgeSignalsAccessResult) 
            {
                return new CachedAgeSignalsAccessResult(_mutatedAgeSignalsAccessResult);
            }
            else if(_mockResultType == CachedAgeSignalsAccessResult.ResultType.Exception) 
            {
                return new CachedAgeSignalsAccessResult(_mutatedCachedError);
            }
            throw new Exception("Invalid mock result type selected.");
        }

        private void CreateDialog()
        {
            var b = new MockPopupBuilder();
            _root.Add(b.Root);
            b.BeginFullScreenOverlay();
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
                _mockResultType = (CachedAgeSignalsAccessResult.ResultType)evt.newValue;
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
            _onOkClicked.TrySetResult(true);
        }

        private void ComposeContent()
        {
            _contentArea.Clear();
            if (_mockResultType == CachedAgeSignalsAccessResult.ResultType.AgeSignalsAccessResult)
            {
                _contentArea.Add(new AgeSignalsAccessResultUI(_mutatedAgeSignalsAccessResult));
            }
            else if (_mockResultType == CachedAgeSignalsAccessResult.ResultType.Exception)
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