using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace IPTech.AgeVerification.iOS.Debugging
{
    [RequireComponent(typeof(UIDocument))]
    public class MockIsEligiblePopUp : MonoBehaviour
    {
        private const string PANEL_SETTINGS_RESOURCE_NAME = "com.iptech.agverfication.ios.declaredagerange.mockpanelsettings";
        [SerializeField]
        private string _title = "Mock Is Eligible";
        
        private UIDocument _uiDocument;
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

            var panelSettings = Resources.Load<PanelSettings>(PANEL_SETTINGS_RESOURCE_NAME);
            if(panelSettings == null) {
                throw new Exception($"PanelSettings resource '{PANEL_SETTINGS_RESOURCE_NAME}' not found in Resources folder.");
            }

            var popupGO = new GameObject("MockIsEligiblePopUp");
            try 
            {
                var uiDocument = popupGO.AddComponent<UIDocument>();
                uiDocument.panelSettings = panelSettings;
                
                // Add and configure the popup component
                var popup = popupGO.AddComponent<MockIsEligiblePopUp>();
                popup._title = "Mock Age Features Eligibility";
                popup._mockIsEligible = new MockIsEligible();
                popup._mockError = new MockIsEligibleError();
                popup._mockResultType = CachedIsEligibleResult.ResultType.IsEligibleResult;
                
                bool isOkClicked = false;
                popup._onClosed = () => { isOkClicked = true; };

                // Don't destroy the popup when loading new scenes
                DontDestroyOnLoad(popupGO);

                while (!isOkClicked)
                {
                    await Task.Yield();
                    if (ct.IsCancellationRequested)
                    {
                        ct.ThrowIfCancellationRequested();
                    }
                }

                var cachedResult = popup.CreateCachedResult();
                popup.ConditionallyRememberChoice(cachedResult);
                return GetResult(cachedResult);
            }
            finally
            {
                if (popupGO != null)
                {
                    UnityEngine.Object.Destroy(popupGO);
                }
            }
        }

        private void Awake()
        {
            _uiDocument = GetComponent<UIDocument>();
        }

        private void Start()
        {
            // Ensure the root element fills the screen
            var root = _uiDocument.rootVisualElement;
            root.style.position = Position.Absolute;
            root.style.top = 0;
            root.style.left = 0;
            root.style.right = 0;
            root.style.bottom = 0;
            root.style.width = Length.Percent(100);
            root.style.height = Length.Percent(100);
            
            CreateDialog();
            ShowDialog();
        }

        private void CreateDialog()
        {
            // Create the root overlay that covers the entire screen
            _dialogOverlay = new VisualElement();
            _dialogOverlay.style.position = Position.Absolute;
            _dialogOverlay.style.top = 0;
            _dialogOverlay.style.left = 0;
            _dialogOverlay.style.right = 0;
            _dialogOverlay.style.bottom = 0;
            _dialogOverlay.style.width = Length.Percent(100);
            _dialogOverlay.style.height = Length.Percent(100);
            _dialogOverlay.style.backgroundColor = new Color(0, 0, 0, 0.8f);
            _dialogOverlay.style.display = DisplayStyle.Flex;
            _dialogOverlay.style.flexDirection = FlexDirection.Column;
            _dialogOverlay.style.justifyContent = Justify.Center;
            _dialogOverlay.style.alignItems = Align.Center;
            
            // Create the dialog container (the actual dialog box)
            _dialogContainer = new VisualElement();
            _dialogContainer.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 1f);
            _dialogContainer.style.borderTopWidth = 2;
            _dialogContainer.style.borderBottomWidth = 2;
            _dialogContainer.style.borderLeftWidth = 2;
            _dialogContainer.style.borderRightWidth = 2;
            _dialogContainer.style.borderTopColor = new Color(0.5f, 0.5f, 0.5f, 1f);
            _dialogContainer.style.borderBottomColor = new Color(0.5f, 0.5f, 0.5f, 1f);
            _dialogContainer.style.borderLeftColor = new Color(0.5f, 0.5f, 0.5f, 1f);
            _dialogContainer.style.borderRightColor = new Color(0.5f, 0.5f, 0.5f, 1f);
            _dialogContainer.style.borderTopLeftRadius = 8;
            _dialogContainer.style.borderTopRightRadius = 8;
            _dialogContainer.style.borderBottomLeftRadius = 8;
            _dialogContainer.style.borderBottomRightRadius = 8;
            _dialogContainer.style.paddingTop = 20;
            _dialogContainer.style.paddingBottom = 20;
            _dialogContainer.style.paddingLeft = 20;
            _dialogContainer.style.paddingRight = 20;
            _dialogContainer.style.minWidth = 300;
            _dialogContainer.style.maxWidth = Length.Percent(90);
            _dialogContainer.style.minHeight = 150;
            _dialogContainer.style.maxHeight = Length.Percent(90);
            _dialogContainer.style.alignSelf = Align.Center;
            _dialogContainer.style.flexShrink = 0;
            
            _dialogOverlay.Add(_dialogContainer);

            // Create title label
            _dialogTitle = new Label(_title);
            _dialogTitle.style.fontSize = 18;
            _dialogTitle.style.color = Color.white;
            _dialogTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
            _dialogTitle.style.marginBottom = 15;
            _dialogTitle.style.marginTop = 0;
            _dialogTitle.style.unityTextAlign = TextAnchor.MiddleCenter;
            _dialogContainer.Add(_dialogTitle);

            // create the result type selector
            var resultTypeField = new EnumField("Result Type", _mockResultType);
            resultTypeField.RegisterValueChangedCallback(evt =>
            {
                _mockResultType = (CachedIsEligibleResult.ResultType)evt.newValue;
                // Rebuild the content area based on selected type
                _contentArea.Clear();
                ComposeContent();
            });
            _dialogContainer.Add(resultTypeField);

            // create a scrollable container for content
            var scrollView = new ScrollView();
            scrollView.style.flexGrow = 1;
            scrollView.style.marginBottom = 10;
            _dialogContainer.Add(scrollView);

            // create a content area inside the scroll view
            _contentArea = new VisualElement();
            scrollView.Add(_contentArea);

            ComposeContent();

            // Create button container for better layout
            var buttonContainer = new VisualElement();
            buttonContainer.style.flexDirection = FlexDirection.Row;
            buttonContainer.style.justifyContent = Justify.Center;
            buttonContainer.style.alignItems = Align.Center;
            buttonContainer.style.marginTop = 10;
            _dialogContainer.Add(buttonContainer);

            // Create OK button
            _okButton = new Button(OnOkClicked);
            _okButton.text = "Confirm";
            _okButton.style.fontSize = 14;
            _okButton.style.minWidth = 80;
            buttonContainer.Add(_okButton);

            // Create "remember this choice" toggle
            _rememberChoiceToggle = new Toggle("Remember this choice");
            _rememberChoiceToggle.style.marginLeft = 15;
            _rememberChoiceToggle.style.alignSelf = Align.Center;
            buttonContainer.Add(_rememberChoiceToggle);

            // Add the dialog overlay to the UIDocument's root
            _uiDocument.rootVisualElement.Add(_dialogOverlay);

            // Close dialog when clicking on overlay background (but not on the dialog itself)
            _dialogOverlay.RegisterCallback<ClickEvent>(OnOverlayClicked);
            _dialogContainer.RegisterCallback<ClickEvent>(OnDialogClicked);
        }

        private void ComposeContent()
        {
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

        private void ShowDialog()
        {
            if (_dialogOverlay != null)
            {
                _dialogOverlay.style.display = DisplayStyle.Flex;
            }
        }

        private void CloseDialog()
        {
            if (_dialogOverlay != null)
            {
                _dialogOverlay.style.display = DisplayStyle.None;
            }
            
            _onClosed?.Invoke();
            
            // Destroy the GameObject after a short delay to allow callbacks to complete
            Destroy(gameObject, 0.1f);
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
