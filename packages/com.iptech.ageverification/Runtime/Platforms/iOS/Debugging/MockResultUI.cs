using UnityEngine;
using UnityEngine.UIElements;

namespace IPTech.AgeVerification.iOS.Debugging
{
    public class MockResultUI
    {
        public event System.Action OnValuesChanged;
        
        private MockResult _mockResult;
        private VisualElement _root;
        private EnumField _statusField;
        private Toggle _hasLowerBoundToggle;
        private IntegerField _lowerBoundField;
        private Toggle _hasUpperBoundToggle;
        private IntegerField _upperBoundField;
        private EnumField _ageDeclarationField;
        private Label _previewStatusLabel;
        private Label _previewLowerBoundLabel;
        private Label _previewUpperBoundLabel;
        private Label _previewAgeDeclarationLabel;
        private TextField _jsonPreviewField;
        private VisualElement _previewContainer;

        public MockResultUI(MockResult mockResult)
        {
            _mockResult = mockResult;

            _root = new VisualElement();

            // Add some styling
            _root.style.paddingTop = 10;
            _root.style.paddingBottom = 10;
            _root.style.paddingLeft = 5;
            _root.style.paddingRight = 5;

            CreateHeader();
            CreateStatusSection();
            CreateAgeBoundsSection();
            CreateAgeDeclarationSection();
            CreatePresetsSection();
            CreatePreviewSection();

            // Initialize values from MockResult
            InitializeValues();

            // Update preview initially and register callbacks
            UpdatePreview();
            RegisterCallbacks();
        }

        public VisualElement GetRootElement()
        {
            return _root;
        }
        
        public VisualElement GetPreviewElement() 
        {
            return _previewContainer;
        }

        private void CreateHeader()
        {
            var header = new Label("Mock Age Range Result Configuration");
            header.style.fontSize = 16;
            header.style.unityFontStyleAndWeight = FontStyle.Bold;
            header.style.marginBottom = 10;
            _root.Add(header);
        }

        private void CreateStatusSection()
        {
            var statusContainer = new VisualElement();
            statusContainer.style.marginBottom = 15;

            var statusLabel = new Label("Result Status");
            statusLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            statusLabel.style.marginBottom = 5;
            statusContainer.Add(statusLabel);

            _statusField = new EnumField("Status", AgeRangeResultStatus.Success);
            _statusField.style.marginLeft = 15;
            statusContainer.Add(_statusField);

            _root.Add(statusContainer);
        }

        private void CreateAgeBoundsSection()
        {
            var boundsContainer = new VisualElement();
            boundsContainer.style.marginBottom = 15;

            var boundsLabel = new Label("Age Bounds");
            boundsLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            boundsLabel.style.marginBottom = 5;
            boundsContainer.Add(boundsLabel);

            // Lower bound container
            var lowerBoundContainer = new VisualElement();
            lowerBoundContainer.style.flexDirection = FlexDirection.Row;
            lowerBoundContainer.style.alignItems = Align.Center;
            lowerBoundContainer.style.marginLeft = 15;
            lowerBoundContainer.style.marginBottom = 5;

            _hasLowerBoundToggle = new Toggle("Has Lower Bound");
            lowerBoundContainer.Add(_hasLowerBoundToggle);

            _lowerBoundField = new IntegerField();
            _lowerBoundField.style.flexGrow = 1;
            _lowerBoundField.style.flexShrink = 1;
            _lowerBoundField.style.marginLeft = 10;
            lowerBoundContainer.Add(_lowerBoundField);

            boundsContainer.Add(lowerBoundContainer);

            // Upper bound container
            var upperBoundContainer = new VisualElement();
            upperBoundContainer.style.flexDirection = FlexDirection.Row;
            upperBoundContainer.style.alignItems = Align.Center;
            upperBoundContainer.style.marginLeft = 15;

            _hasUpperBoundToggle = new Toggle("Has Upper Bound");
            upperBoundContainer.Add(_hasUpperBoundToggle);

            _upperBoundField = new IntegerField();
            _upperBoundField.style.flexGrow = 1;
            _upperBoundField.style.flexShrink = 1;
            _upperBoundField.style.marginLeft = 10;
            upperBoundContainer.Add(_upperBoundField);

            boundsContainer.Add(upperBoundContainer);

            _root.Add(boundsContainer);
        }

        private void CreateAgeDeclarationSection()
        {
            var declarationContainer = new VisualElement();
            declarationContainer.style.marginBottom = 15;

            var declarationLabel = new Label("Age Declaration");
            declarationLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            declarationLabel.style.marginBottom = 5;
            declarationContainer.Add(declarationLabel);

            _ageDeclarationField = new EnumField("Declaration Type", AgeDeclaration.Unknown);
            _ageDeclarationField.style.marginLeft = 15;
            declarationContainer.Add(_ageDeclarationField);

            _root.Add(declarationContainer);
        }

        private void CreatePreviewSection()
        {
            _previewContainer = new VisualElement();
            _previewContainer.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.1f);
            _previewContainer.style.borderTopWidth = 1;
            _previewContainer.style.borderBottomWidth = 1;
            _previewContainer.style.borderLeftWidth = 1;
            _previewContainer.style.borderRightWidth = 1;
            _previewContainer.style.borderTopColor = Color.gray;
            _previewContainer.style.borderBottomColor = Color.gray;
            _previewContainer.style.borderLeftColor = Color.gray;
            _previewContainer.style.borderRightColor = Color.gray;
            _previewContainer.style.paddingTop = 10;
            _previewContainer.style.paddingBottom = 10;
            _previewContainer.style.paddingLeft = 10;
            _previewContainer.style.paddingRight = 10;

            var resultContainer = new VisualElement();

            var resultLabel = new Label("Generated Result:");
            resultLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            resultLabel.style.marginBottom = 5;
            resultContainer.Add(resultLabel);

            _previewStatusLabel = new Label();
            _previewStatusLabel.style.marginLeft = 15;
            resultContainer.Add(_previewStatusLabel);

            _previewLowerBoundLabel = new Label();
            _previewLowerBoundLabel.style.marginLeft = 15;
            resultContainer.Add(_previewLowerBoundLabel);

            _previewUpperBoundLabel = new Label();
            _previewUpperBoundLabel.style.marginLeft = 15;
            resultContainer.Add(_previewUpperBoundLabel);

            _previewAgeDeclarationLabel = new Label();
            _previewAgeDeclarationLabel.style.marginLeft = 15;
            _previewAgeDeclarationLabel.style.marginBottom = 10;
            resultContainer.Add(_previewAgeDeclarationLabel);

            _previewContainer.Add(resultContainer);

            var jsonLabel = new Label("JSON Output:");
            jsonLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            jsonLabel.style.marginBottom = 5;
            _previewContainer.Add(jsonLabel);

            _jsonPreviewField = new TextField();
            _jsonPreviewField.multiline = true;
            _jsonPreviewField.style.height = 160;
            _jsonPreviewField.style.whiteSpace = WhiteSpace.Normal;
            _jsonPreviewField.SetEnabled(false);
            _previewContainer.Add(_jsonPreviewField);
        }

        private void CreatePresetsSection()
        {
            var presetsContainer = new VisualElement();

            var presetsLabel = new Label("Quick Presets");
            presetsLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            presetsLabel.style.marginBottom = 10;
            presetsContainer.Add(presetsLabel);

            // First row of buttons
            var buttonRow1 = new VisualElement();
            buttonRow1.style.flexDirection = FlexDirection.Row;
            buttonRow1.style.marginBottom = 5;

            var successButton1 = new Button(() => SetPreset(AgeRangeResultStatus.Success, true, 13, true, 17, AgeDeclaration.SelfDeclared))
            {
                text = "Success (13-17)"
            };
            successButton1.style.flexGrow = 1;
            successButton1.style.marginRight = 5;
            buttonRow1.Add(successButton1);

            var successButton2 = new Button(() => SetPreset(AgeRangeResultStatus.Success, true, 5, true, 12, AgeDeclaration.GuardianDeclared))
            {
                text = "Success (5-12)"
            };
            successButton2.style.flexGrow = 1;
            buttonRow1.Add(successButton2);

            presetsContainer.Add(buttonRow1);

            // Second row of buttons
            var buttonRow2 = new VisualElement();
            buttonRow2.style.flexDirection = FlexDirection.Row;

            var successButton3 = new Button(() => SetPreset(AgeRangeResultStatus.Success, true, 18, false, 0, AgeDeclaration.SelfDeclared))
            {
                text = "Success (18+)"
            };
            successButton3.style.flexGrow = 1;
            successButton3.style.marginRight = 5;
            buttonRow2.Add(successButton3);

            var declinedButton = new Button(() => SetPreset(AgeRangeResultStatus.UserDeclined, false, 0, false, 0, AgeDeclaration.Unknown))
            {
                text = "User Declined"
            };
            declinedButton.style.flexGrow = 1;
            declinedButton.style.marginRight = 5;
            buttonRow2.Add(declinedButton);

            var unsupportedButton = new Button(() => SetPreset(AgeRangeResultStatus.UnsupportedPlatformVersion, false, 0, false, 0, AgeDeclaration.Unknown))
            {
                text = "Unsupported Platform"
            };
            unsupportedButton.style.flexGrow = 1;
            buttonRow2.Add(unsupportedButton);

            presetsContainer.Add(buttonRow2);

            _root.Add(presetsContainer);
        }

        private void InitializeValues()
        {
            if (_mockResult == null) return;

            _statusField.value = _mockResult.Status;
            _hasLowerBoundToggle.value = _mockResult.HasLowerBound;
            _lowerBoundField.value = _mockResult.LowerBound;
            _hasUpperBoundToggle.value = _mockResult.HasUpperBound;
            _upperBoundField.value = _mockResult.UpperBound;
            _ageDeclarationField.value = _mockResult.AgeDeclaration;

            UpdateLowerBoundFieldState();
            UpdateUpperBoundFieldState();
        }

        private void RegisterCallbacks()
        {
            _statusField.RegisterValueChangedCallback(evt => 
            {
                if (_mockResult != null) _mockResult.Status = (AgeRangeResultStatus)evt.newValue;
                UpdatePreview();
                OnValuesChanged?.Invoke();
            });
            
            _hasLowerBoundToggle.RegisterValueChangedCallback(evt => 
            {
                if (_mockResult != null) _mockResult.HasLowerBound = evt.newValue;
                UpdateLowerBoundFieldState();
                UpdatePreview();
                OnValuesChanged?.Invoke();
            });
            
            _lowerBoundField.RegisterValueChangedCallback(evt => 
            {
                if (_mockResult != null) _mockResult.LowerBound = evt.newValue;
                UpdatePreview();
                OnValuesChanged?.Invoke();
            });
            
            _hasUpperBoundToggle.RegisterValueChangedCallback(evt => 
            {
                if (_mockResult != null) _mockResult.HasUpperBound = evt.newValue;
                UpdateUpperBoundFieldState();
                UpdatePreview();
                OnValuesChanged?.Invoke();
            });
            
            _upperBoundField.RegisterValueChangedCallback(evt => 
            {
                if (_mockResult != null) _mockResult.UpperBound = evt.newValue;
                UpdatePreview();
                OnValuesChanged?.Invoke();
            });
            
            _ageDeclarationField.RegisterValueChangedCallback(evt => 
            {
                if (_mockResult != null) _mockResult.AgeDeclaration = (AgeDeclaration)evt.newValue;
                UpdatePreview();
                OnValuesChanged?.Invoke();
            });

            // Initial state
            UpdateLowerBoundFieldState();
            UpdateUpperBoundFieldState();
        }

        private void UpdateLowerBoundFieldState()
        {
            _lowerBoundField.SetEnabled(_hasLowerBoundToggle.value);
            _lowerBoundField.style.opacity = _hasLowerBoundToggle.value ? 1.0f : 0.5f;
        }

        private void UpdateUpperBoundFieldState()
        {
            _upperBoundField.SetEnabled(_hasUpperBoundToggle.value);
            _upperBoundField.style.opacity = _hasUpperBoundToggle.value ? 1.0f : 0.5f;
        }

        private void UpdatePreview()
        {
            if (_mockResult == null) return;

            var result = _mockResult.CreateResult();

            _previewStatusLabel.text = $"Status: {result.Status}";
            _previewLowerBoundLabel.text = $"Lower Bound: {(result.LowerBound?.ToString() ?? "null")}";
            _previewUpperBoundLabel.text = $"Upper Bound: {(result.UpperBound?.ToString() ?? "null")}";
            _previewAgeDeclarationLabel.text = $"Age Declaration: {result.AgeDeclaration}";

            var json = result.ToJson(prettyPrint: true);
            _jsonPreviewField.value = json;
        }

        private void SetPreset(AgeRangeResultStatus status, bool hasLower, int lower, bool hasUpper, int upper, AgeDeclaration declaration)
        {
            if (_mockResult == null) return;

            // Update the MockResult data
            _mockResult.Status = status;
            _mockResult.HasLowerBound = hasLower;
            _mockResult.LowerBound = lower;
            _mockResult.HasUpperBound = hasUpper;
            _mockResult.UpperBound = upper;
            _mockResult.AgeDeclaration = declaration;

            // Update UI elements to reflect the new values
            _statusField.value = status;
            _hasLowerBoundToggle.value = hasLower;
            _lowerBoundField.value = lower;
            _hasUpperBoundToggle.value = hasUpper;
            _upperBoundField.value = upper;
            _ageDeclarationField.value = declaration;

            UpdateLowerBoundFieldState();
            UpdateUpperBoundFieldState();
            UpdatePreview();
            OnValuesChanged?.Invoke();
        }

        public void RefreshFromMockResult()
        {
            if (_mockResult == null) return;
            
            InitializeValues();
            UpdatePreview();
        }

        public AgeRangeResult GetCurrentResult()
        {
            return _mockResult?.CreateResult();
        }
    }
}