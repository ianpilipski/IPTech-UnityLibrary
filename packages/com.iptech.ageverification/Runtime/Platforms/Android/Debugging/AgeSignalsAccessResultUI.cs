using UnityEngine;
using UnityEngine.UIElements;
using System;
using IPTech.AgeVerification.Debugging;

namespace IPTech.AgeVerification.Android.AgeSignals.Debugging
{
    public class AgeSignalsAccessResultUI : VisualElement
    {
        public event System.Action OnValuesChanged;
        
        private AgeSignalsAccessResult _ageSignalsAccessResult;
        private IAgeSignalsAccessResultBackingFields _ageSignalsAccessResultBackingFields => _ageSignalsAccessResult;
        private bool _isPreview;
        private Toggle _hasAgeSignalsAccessStatusToggle;
        private EnumField _ageSignalsAccessStatusField;
        private TextField _jsonPreviewField;
        private Label _previewAccessStatusLabel;
        
        public AgeSignalsAccessResultUI(AgeSignalsAccessResult ageSignalsAccessResult, bool isPreview = false)
        {
            this.name = "age-signals-access-result-ui";
            
            _ageSignalsAccessResult = ageSignalsAccessResult;
            _isPreview = isPreview;

            var b = new MockPopupBuilder();
            this.Add(b.Root);

            if(!_isPreview)
            {
                CreateStatusSection(b);
            }
            else 
            {
                CreatePreviewSection(b);
            }

            UpdateValues();
            RegisterCallbacks();
        }

        private void CreateStatusSection(MockPopupBuilder b)
        {
            b.BeginSection("AgeSignals Access Result");
            _hasAgeSignalsAccessStatusToggle = b.AddToggleProperty("Has Status", true);
            _ageSignalsAccessStatusField = b.AddEnumProperty("Status", AgeSignalsAccessStatus.UNSPECIFIED);
            b.EndSection();
        }

        private void CreatePreviewSection(MockPopupBuilder b)
        {
            b.BeginSection("");

            b.BeginGroup("Generated Result");
            _previewAccessStatusLabel = b.AddLabelProperty("ageSignalsAccessStatus");
            b.EndGroup();

            b.BeginGroup("JSON Output:");
            _jsonPreviewField = b.AddTextFieldProperty("", "");
            b.EndGroup();

            b.EndSection();
        }

        private void UpdateValues()
        {
            if (_ageSignalsAccessResultBackingFields == null) return;

            if(!_isPreview)
            {
                _hasAgeSignalsAccessStatusToggle.SetValueWithoutNotify(_ageSignalsAccessResultBackingFields._hasAgeSignalsAccessStatus);
                _ageSignalsAccessStatusField.SetValueWithoutNotify(_ageSignalsAccessResultBackingFields._ageSignalsAccessStatus);
                _ageSignalsAccessStatusField.SetEnabled(_hasAgeSignalsAccessStatusToggle.value);
                _ageSignalsAccessStatusField.style.opacity = _hasAgeSignalsAccessStatusToggle.value ? 1.0f : 0.5f;
            }
            UpdatePreview();
        }

        private void RegisterCallbacks()
        {
            _hasAgeSignalsAccessStatusToggle.RegisterValueChangedCallback(evt => 
            {
                if (_ageSignalsAccessResultBackingFields != null) _ageSignalsAccessResultBackingFields._hasAgeSignalsAccessStatus = evt.newValue;
                UpdateValues();
                OnValuesChanged?.Invoke();
            });

            _ageSignalsAccessStatusField.RegisterValueChangedCallback(evt => 
            {
                if (_ageSignalsAccessResultBackingFields != null) _ageSignalsAccessResultBackingFields._ageSignalsAccessStatus = (AgeSignalsAccessStatus)evt.newValue;
                UpdateValues();
                OnValuesChanged?.Invoke();
            });
        }

        
        private void UpdatePreview()
        {
            if (_ageSignalsAccessResultBackingFields == null) return;
            if (!_isPreview) return;

            _previewAccessStatusLabel.text = $"Age Signals Access Status: {(_ageSignalsAccessResult.AgeSignalsAccessStatus?.ToString() ?? "null")}";
            
            var json = _ageSignalsAccessResult.ToJson(prettyPrint: true);
            _jsonPreviewField.value = json;
        }
    }
}