using IPTech.AgeVerification.Debugging;
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
            if(_mockResult == null)
            {
                return;
            }

            var b = new MockPopupBuilder();
            
            b.BeginSection("Result Status");
            _statusField = b.AddEnumProperty("Status", _mockResult.Status);
            b.EndSection();

            b.BeginSection("Age Bounds");
            _hasLowerBoundToggle = b.AddToggleProperty("Has Lower Bound", _mockResult.HasLowerBound);
            _lowerBoundField = b.AddIntegerProperty("Lower Bound", _mockResult.LowerBound);
            _hasUpperBoundToggle = b.AddToggleProperty("Has Upper Bound", _mockResult.HasUpperBound);
            _upperBoundField = b.AddIntegerProperty("Upper Bound", _mockResult.UpperBound);
            b.EndSection();

            b.BeginSection("Age Declaration");
            _ageDeclarationField = b.AddEnumProperty("Age Declaration", _mockResult.AgeDeclaration);
            b.EndSection();

            CreatePresets(b);
            CreatePreviewSection(b);

            // Initialize values from MockResult
            InitializeValues();

            // Update preview initially and register callbacks
            UpdatePreview();
            RegisterCallbacks();

            _root = b.Root;
        }

        public VisualElement GetRootElement()
        {
            return _root;
        }
        
        public VisualElement GetPreviewElement() 
        {
            return _previewContainer;
        }

        private void CreatePreviewSection(MockPopupBuilder b)
        {
            _previewContainer = b.BeginGroup("Preview");
            b.BeginSection("Preview");
            _previewStatusLabel = b.AddLabelProperty("Status");
            _previewLowerBoundLabel = b.AddLabelProperty("Lower Bound");
            _previewUpperBoundLabel = b.AddLabelProperty("Upper Bound");
            _previewAgeDeclarationLabel = b.AddLabelProperty("Age Declaration");
            b.EndSection();
            b.BeginSection("JSON Output");
            _jsonPreviewField = b.AddTextFieldProperty("JSON Preview", "");
            b.EndSection();
            b.EndGroup();
        }

        private void CreatePresets(MockPopupBuilder b)
        {
            b.BeginSection("Presets");
            b.BeginColumns("Presets");
            for(int i = 0; i <= 4; i++)
            {
                switch(i)
                {
                    case 0:
                        b.AddButton("Success (13-17)", () => SetPreset(AgeRangeResultStatus.Success, true, 13, true, 17, AgeDeclaration.SelfDeclared));
                        break;
                    case 1:
                        b.AddButton("Success (5-12)", () => SetPreset(AgeRangeResultStatus.Success, true, 5, true, 12, AgeDeclaration.GuardianDeclared));
                        break;
                    case 2:
                        b.AddButton("Success (18+)", () => SetPreset(AgeRangeResultStatus.Success, true, 18, false, 0, AgeDeclaration.SelfDeclared));
                        break;
                    case 3:
                        b.AddButton("User Declined", () => SetPreset(AgeRangeResultStatus.UserDeclined, false, 0, false, 0, AgeDeclaration.Unknown));
                        break;
                    case 4:
                        b.AddButton("Unsupported Platform", () => SetPreset(AgeRangeResultStatus.UnsupportedPlatformVersion, false, 0, false, 0, AgeDeclaration.Unknown));
                        break;
                    default:
                        // Add more cases as needed
                        break;
                }
            }
            b.EndColumns();
            b.EndSection();
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