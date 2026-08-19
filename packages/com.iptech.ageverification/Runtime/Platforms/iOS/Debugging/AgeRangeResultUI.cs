using IPTech.AgeVerification.Debugging;
using UnityEngine;
using UnityEngine.UIElements;

namespace IPTech.AgeVerification.iOS.Debugging
{
    public class AgeRangeResultUI : VisualElement
    {
        public event System.Action OnValuesChanged;
        
        private bool _isPreview;
        private AgeRangeResult _ageRangeResult;
        private IAgeRangeResultBackingFields _ageRangeResultBackingFields => _ageRangeResult;
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

        
        public AgeRangeResultUI(AgeRangeResult result, bool isPreview = false) 
        {
            _ageRangeResult = result;
            _isPreview = isPreview;
            
            var b = new MockPopupBuilder();
            this.Add(b.Root);
            
            if(!_isPreview)
            {
                b.BeginSection("Result Status");
                _statusField = b.AddEnumProperty("Status", _ageRangeResultBackingFields.Status);
                b.EndSection();

                b.BeginSection("Age Bounds");
                _hasLowerBoundToggle = b.AddToggleProperty("Has Lower Bound", _ageRangeResultBackingFields._lowerBoundHasValue);
                _lowerBoundField = b.AddIntegerProperty("Lower Bound", _ageRangeResultBackingFields._lowerBound);
                _hasUpperBoundToggle = b.AddToggleProperty("Has Upper Bound", _ageRangeResultBackingFields._upperBoundHasValue);
                _upperBoundField = b.AddIntegerProperty("Upper Bound", _ageRangeResultBackingFields._upperBound);
                b.EndSection();

                b.BeginSection("Age Declaration");
                _ageDeclarationField = b.AddEnumProperty("Age Declaration", _ageRangeResultBackingFields.AgeDeclaration);
                b.EndSection();

                CreatePresets(b);
            }
            else
            {
                CreatePreviewSection(b);
            }

            Refresh(false);
            RegisterCallbacks();
        }

        private void CreatePreviewSection(MockPopupBuilder b)
        {
            b.BeginSection("Age Range Result");
            _previewStatusLabel = b.AddLabelProperty("Status");
            _previewLowerBoundLabel = b.AddLabelProperty("Lower Bound");
            _previewUpperBoundLabel = b.AddLabelProperty("Upper Bound");
            _previewAgeDeclarationLabel = b.AddLabelProperty("Age Declaration");
            b.EndSection();
            b.BeginSection("JSON Output");
            _jsonPreviewField = b.AddTextFieldProperty("JSON Preview", "");
            b.EndSection();
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
                        b.AddButton("Success (13-17)", () => SetValues(AgeRangeResultStatus.Success, 13, 17, AgeDeclaration.SelfDeclared));
                        break;
                    case 1:
                        b.AddButton("Success (5-12)", () => SetValues(AgeRangeResultStatus.Success, 5, 12, AgeDeclaration.GuardianDeclared));
                        break;
                    case 2:
                        b.AddButton("Success (18+)", () => SetValues(AgeRangeResultStatus.Success, 18, 0, AgeDeclaration.SelfDeclared));
                        break;
                    case 3:
                        b.AddButton("User Declined", () => SetValues(AgeRangeResultStatus.UserDeclined, null, null, AgeDeclaration.Unknown));
                        break;
                    case 4:
                        b.AddButton("Unsupported Platform", () => SetValues(AgeRangeResultStatus.UnsupportedPlatformVersion, null, null, AgeDeclaration.Unknown));
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
                _ageRangeResultBackingFields.Status = (AgeRangeResultStatus)evt.newValue;
                Refresh(fireValuesChangedEvent: true);
            });
            
            _hasLowerBoundToggle.RegisterValueChangedCallback(evt => 
            {
                _ageRangeResultBackingFields._lowerBoundHasValue = evt.newValue;
                Refresh(fireValuesChangedEvent: true);
            });
            
            _lowerBoundField.RegisterValueChangedCallback(evt => 
            {
                _ageRangeResultBackingFields._lowerBound = evt.newValue;
                Refresh(fireValuesChangedEvent: true);
            });
            
            _hasUpperBoundToggle.RegisterValueChangedCallback(evt => 
            {
                _ageRangeResultBackingFields._upperBoundHasValue = evt.newValue;
                Refresh(fireValuesChangedEvent: true);
            });
            
            _upperBoundField.RegisterValueChangedCallback(evt => 
            {
                _ageRangeResultBackingFields._upperBound = evt.newValue;
                Refresh(fireValuesChangedEvent: true);
            });
            
            _ageDeclarationField.RegisterValueChangedCallback(evt => 
            {
                _ageRangeResultBackingFields.AgeDeclaration = (AgeDeclaration)evt.newValue;
                Refresh(fireValuesChangedEvent: true);
            });
        }

        private void Refresh(bool fireValuesChangedEvent = false)
        {
            if(!_isPreview)
            {
                _statusField.SetValueWithoutNotify(_ageRangeResultBackingFields.Status);
                _hasLowerBoundToggle.SetValueWithoutNotify(_ageRangeResultBackingFields._lowerBoundHasValue);
                _lowerBoundField.SetValueWithoutNotify(_ageRangeResultBackingFields._lowerBound);
                _hasUpperBoundToggle.SetValueWithoutNotify(_ageRangeResultBackingFields._upperBoundHasValue);
                _upperBoundField.SetValueWithoutNotify(_ageRangeResultBackingFields._upperBound);
                _ageDeclarationField.SetValueWithoutNotify(_ageRangeResultBackingFields.AgeDeclaration);
                
                _lowerBoundField.SetEnabled(_hasLowerBoundToggle.value);
                _lowerBoundField.style.opacity = _hasLowerBoundToggle.value ? 1.0f : 0.5f;
                
                _upperBoundField.SetEnabled(_hasUpperBoundToggle.value);
                _upperBoundField.style.opacity = _hasUpperBoundToggle.value ? 1.0f : 0.5f;
            }

            UpdatePreview();

            if(fireValuesChangedEvent)
            {
                OnValuesChanged?.Invoke();
            }
        }

    
        private void UpdatePreview()
        {
            if (_ageRangeResultBackingFields == null) return;
            if (!_isPreview) return;

            var result = _ageRangeResult;

            _previewStatusLabel.text = $"Status: {result.Status}";
            _previewLowerBoundLabel.text = $"Lower Bound: {(result.LowerBound?.ToString() ?? "null")}";
            _previewUpperBoundLabel.text = $"Upper Bound: {(result.UpperBound?.ToString() ?? "null")}";
            _previewAgeDeclarationLabel.text = $"Age Declaration: {result.AgeDeclaration}";

            var json = result.ToJson(prettyPrint: true);
            _jsonPreviewField.value = json;
        }

        private void SetValues(AgeRangeResultStatus status, int? lower, int? upper, AgeDeclaration declaration)
        {
            if (_ageRangeResultBackingFields == null) return;

            _ageRangeResultBackingFields.Status = status;
            _ageRangeResultBackingFields._lowerBoundHasValue = lower.HasValue;
            _ageRangeResultBackingFields._lowerBound = lower ?? _ageRangeResultBackingFields._lowerBound;
            _ageRangeResultBackingFields._upperBoundHasValue = upper.HasValue;
            _ageRangeResultBackingFields._upperBound = upper ?? _ageRangeResultBackingFields._upperBound;
            _ageRangeResultBackingFields.AgeDeclaration = declaration;

            Refresh(true);
        }
    }
}