using IPTech.AgeVerification.Debugging;
using UnityEngine;
using UnityEngine.UIElements;

namespace IPTech.AgeVerification.iOS.Debugging
{
    public class MockResultUI : VisualElement
    {
        public event System.Action OnValuesChanged;
        public VisualElement PreviewVisualElement { get; private set; }

        
        private MockResult _mockResult;
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

        
        public MockResultUI(AgeRangeResult result) : this(new MockResult(result))
        {
        }

        public MockResultUI(MockResult mockResult)
        {
            _mockResult = mockResult;
            
            var b = new MockPopupBuilder();
            this.Add(b.Root);
            
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

            Refresh(false);

            RegisterCallbacks();
        }

        private void CreatePreviewSection(MockPopupBuilder b)
        {
            PreviewVisualElement = b.BeginGroup("Preview");
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
                        b.AddButton("Success (13-17)", () => SetValues(AgeRangeResultStatus.Success, true, 13, true, 17, AgeDeclaration.SelfDeclared));
                        break;
                    case 1:
                        b.AddButton("Success (5-12)", () => SetValues(AgeRangeResultStatus.Success, true, 5, true, 12, AgeDeclaration.GuardianDeclared));
                        break;
                    case 2:
                        b.AddButton("Success (18+)", () => SetValues(AgeRangeResultStatus.Success, true, 18, false, 0, AgeDeclaration.SelfDeclared));
                        break;
                    case 3:
                        b.AddButton("User Declined", () => SetValues(AgeRangeResultStatus.UserDeclined, false, 0, false, 0, AgeDeclaration.Unknown));
                        break;
                    case 4:
                        b.AddButton("Unsupported Platform", () => SetValues(AgeRangeResultStatus.UnsupportedPlatformVersion, false, 0, false, 0, AgeDeclaration.Unknown));
                        break;
                    default:
                        // Add more cases as needed
                        break;
                }
            }
            b.EndColumns();
            b.EndSection();
        }

        
        private void RegisterCallbacks()
        {
            _statusField.RegisterValueChangedCallback(evt => 
            {
                _mockResult.Status = (AgeRangeResultStatus)evt.newValue;
                Refresh(fireValuesChangedEvent: true);
            });
            
            _hasLowerBoundToggle.RegisterValueChangedCallback(evt => 
            {
                _mockResult.HasLowerBound = evt.newValue;
                Refresh(fireValuesChangedEvent: true);
            });
            
            _lowerBoundField.RegisterValueChangedCallback(evt => 
            {
                _mockResult.LowerBound = evt.newValue;
                Refresh(fireValuesChangedEvent: true);
            });
            
            _hasUpperBoundToggle.RegisterValueChangedCallback(evt => 
            {
                _mockResult.HasUpperBound = evt.newValue;
                Refresh(fireValuesChangedEvent: true);
            });
            
            _upperBoundField.RegisterValueChangedCallback(evt => 
            {
                _mockResult.UpperBound = evt.newValue;
                Refresh(fireValuesChangedEvent: true);
            });
            
            _ageDeclarationField.RegisterValueChangedCallback(evt => 
            {
                _mockResult.AgeDeclaration = (AgeDeclaration)evt.newValue;
                Refresh(fireValuesChangedEvent: true);
            });
        }

        private void Refresh(bool fireValuesChangedEvent = false)
        {
            _statusField.value = _mockResult.Status;
            _hasLowerBoundToggle.value = _mockResult.HasLowerBound;
            _lowerBoundField.value = _mockResult.LowerBound;
            _hasUpperBoundToggle.value = _mockResult.HasUpperBound;
            _upperBoundField.value = _mockResult.UpperBound;
            _ageDeclarationField.value = _mockResult.AgeDeclaration;

            UpdateLowerBoundFieldState();
            UpdateUpperBoundFieldState();
            UpdatePreview();

            if(fireValuesChangedEvent)
            {
                OnValuesChanged?.Invoke();
            }
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

            var result = _mockResult.ToAgeRangeResult();

            _previewStatusLabel.text = $"Status: {result.Status}";
            _previewLowerBoundLabel.text = $"Lower Bound: {(result.LowerBound?.ToString() ?? "null")}";
            _previewUpperBoundLabel.text = $"Upper Bound: {(result.UpperBound?.ToString() ?? "null")}";
            _previewAgeDeclarationLabel.text = $"Age Declaration: {result.AgeDeclaration}";

            var json = result.ToJson(prettyPrint: true);
            _jsonPreviewField.value = json;
        }

        private void SetValues(AgeRangeResultStatus status, bool hasLower, int lower, bool hasUpper, int upper, AgeDeclaration declaration)
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

        public AgeRangeResult GetCurrentResult()
        {
            return _mockResult?.ToAgeRangeResult();
        }
    }
}