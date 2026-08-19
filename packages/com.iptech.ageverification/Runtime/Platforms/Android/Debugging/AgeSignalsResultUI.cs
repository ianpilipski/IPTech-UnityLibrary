using UnityEngine;
using UnityEngine.UIElements;
using System;
using IPTech.AgeVerification.Debugging;
using System.Globalization;

namespace IPTech.AgeVerification.Android.AgeSignals.Debugging
{
    public class AgeSignalsResultUI : VisualElement
    {
        public event System.Action OnValuesChanged;
        
        private AgeSignalsResult _ageSignalsResult;
        private IAgeSignalsResultBackingFields _ageSignalsResultUIAccess => _ageSignalsResult;
        private bool _isPreview;
        private Toggle _hasUserStatusToggle;
        private EnumField _userStatusField;
        private Toggle _hasAgeLowerToggle;
        private IntegerField _ageLowerField;
        private Toggle _hasAgeUpperToggle;
        private IntegerField _ageUpperField;
        private Toggle _hasMostRecentApprovalDateToggle;
        private TextField _mostRecentApprovalDateField;
        private TextField _installIdField;
        private Label _previewUserStatusLabel;
        private Label _previewAgeLowerLabel;
        private Label _previewAgeUpperLabel;
        private Label _previewMostRecentApprovalDateLabel;
        private Label _previewInstallIdLabel;
        private TextField _jsonPreviewField;

        public AgeSignalsResultUI(AgeSignalsResult ageSignalsResult, bool isPreview) 
        {
            _ageSignalsResult = ageSignalsResult;
            _isPreview = isPreview;

            var b = new MockPopupBuilder();
            this.Add(b.Root);

            if(!_isPreview)
            {
                CreateUserStatusSection(b);
                CreateAgeBoundsSection(b);
                CreateApprovalDateSection(b);
                CreateInstallIdSection(b);
                CreatePresetsSection(b);
            }
            else 
            {
                CreatePreviewSection(b);
            }

            // Initialize values from MockResult
            UpdateValues();

            RegisterCallbacks();
        }

        private void CreateUserStatusSection(MockPopupBuilder b)
        {
            b.BeginSection("User Status");
            _hasUserStatusToggle = b.AddToggleProperty("Has User Status", true);
            _userStatusField = b.AddEnumProperty("User Status", AgeSignalsVerificationStatus.VERIFIED);
            b.EndSection();
        }

        private void CreateAgeBoundsSection(MockPopupBuilder b)
        {
            b.BeginSection("Age Bounds");
            
            _hasAgeLowerToggle = b.AddToggleProperty("Has Age Lower", true);
            _ageLowerField = b.AddIntegerProperty("Age Lower", 0);

            _hasAgeUpperToggle = b.AddToggleProperty("Has Age Upper", true);
            _ageUpperField = b.AddIntegerProperty("Age Upper", 0);
            b.EndSection();
        }

        private void CreateApprovalDateSection(MockPopupBuilder b)
        {
            b.BeginSection("Most Recent Approval Date");
            
            _hasMostRecentApprovalDateToggle = b.AddToggleProperty("Has Approval Date", true);
            _mostRecentApprovalDateField = b.AddTextFieldProperty("Most Recent Approval Date", "");
            b.EndSection();
        }

        private void CreateInstallIdSection(MockPopupBuilder b)
        {
            b.BeginSection("Install ID");
            _installIdField = b.AddTextFieldProperty("Install ID", "");
            b.EndSection();
        }
            

        private void CreatePreviewSection(MockPopupBuilder b)
        {
            b.BeginSection("");

            b.BeginGroup("Generated Result");
            _previewUserStatusLabel = b.AddLabelProperty("userStatus");
            _previewAgeLowerLabel = b.AddLabelProperty("ageLower");
            _previewAgeUpperLabel = b.AddLabelProperty("ageUpper");
            _previewMostRecentApprovalDateLabel = b.AddLabelProperty("mostRecentApprovalDate");
            _previewInstallIdLabel = b.AddLabelProperty("installId");
            b.EndGroup();

            var jsonLabel = b.BeginGroup("JSON Output:");
            _jsonPreviewField = b.AddTextFieldProperty("", "");
            b.EndGroup();

            b.EndSection();
        }

        private void CreatePresetsSection(MockPopupBuilder b)
        {
            var presetsContainer = b.BeginSection("Quick Presets");
            
            var approvalDate = DateTime.UtcNow.AddMonths(-6);
            for(int i=0; i<=5; i++)
            {
                switch(i)
                {
                    case 0:
                        b.AddButton("Verified (13-17)", () => SetPreset(AgeSignalsVerificationStatus.VERIFIED, 13, 17, null, "test-installid-123"));
                        break;
                    case 1:
                        b.AddButton("Supervised (5-12)", () => SetPreset(AgeSignalsVerificationStatus.SUPERVISED, 5, 12, approvalDate, "test-installid-456"));
                        break;
                    case 2:
                        b.AddButton("Approval Pending", () => SetPreset(AgeSignalsVerificationStatus.SUPERVISED_APPROVAL_PENDING, null, null, null, "test-installid-789"));
                        break;
                    case 3:
                        b.AddButton("Approval Denied", () => SetPreset(AgeSignalsVerificationStatus.SUPERVISED_APPROVAL_DENIED, null, null, null, "test-installid-000"));
                        break;
                    case 4:
                        b.AddButton("Verified (18+)", () => SetPreset(AgeSignalsVerificationStatus.VERIFIED, 18, null, approvalDate, "test-installid-adult"));
                        break;
                    case 5:
                        b.AddButton("Unknown Status", () => SetPreset(AgeSignalsVerificationStatus.UNKNOWN, null, null, null, "unknown-installid"));
                        break;
                    // Add more cases as needed
                }
            }
            b.EndSection();
        }

        private void UpdateValues()
        {
            if (_ageSignalsResultUIAccess == null) return;

            if(!_isPreview)
            {
                _hasUserStatusToggle.SetValueWithoutNotify(_ageSignalsResultUIAccess._userStatusHasValue);
                _userStatusField.SetValueWithoutNotify(_ageSignalsResultUIAccess._userStatus);
                _hasAgeLowerToggle.SetValueWithoutNotify(_ageSignalsResultUIAccess._ageLowerHasValue);
                _ageLowerField.SetValueWithoutNotify(_ageSignalsResultUIAccess._ageLower);
                _hasAgeUpperToggle.SetValueWithoutNotify(_ageSignalsResultUIAccess._ageUppderHasValue);
                _ageUpperField.SetValueWithoutNotify(_ageSignalsResultUIAccess._ageUpper);
                _hasMostRecentApprovalDateToggle.SetValueWithoutNotify(_ageSignalsResultUIAccess._mostRecentApprovalDateHasValue);
                _mostRecentApprovalDateField.SetValueWithoutNotify(_ageSignalsResultUIAccess.MostRecentApprovalDate.ToString("yyyy-MM-dd"));
                _installIdField.SetValueWithoutNotify(_ageSignalsResultUIAccess.InstallId);

                _userStatusField.SetEnabled(_hasUserStatusToggle.value);
                _userStatusField.style.opacity = _hasUserStatusToggle.value ? 1.0f : 0.5f;
                _ageLowerField.SetEnabled(_hasAgeLowerToggle.value);
                _ageLowerField.style.opacity = _hasAgeLowerToggle.value ? 1.0f : 0.5f;
                _ageUpperField.SetEnabled(_hasAgeUpperToggle.value);
                _ageUpperField.style.opacity = _hasAgeUpperToggle.value ? 1.0f : 0.5f;
                _mostRecentApprovalDateField.SetEnabled(_hasMostRecentApprovalDateToggle.value);
                _mostRecentApprovalDateField.style.opacity = _hasMostRecentApprovalDateToggle.value ? 1.0f : 0.5f;
            }
            else
            {
                UpdatePreview();    
            }
        }

        private void RegisterCallbacks()
        {
            if(_isPreview) return;

            _hasUserStatusToggle.RegisterValueChangedCallback(evt => 
            {
                if (_ageSignalsResultUIAccess != null) _ageSignalsResultUIAccess._userStatusHasValue = evt.newValue;
                UpdateValues();
                OnValuesChanged?.Invoke();
            });

            _userStatusField.RegisterValueChangedCallback(evt => 
            {
                if (_ageSignalsResultUIAccess != null) _ageSignalsResultUIAccess._userStatus = (AgeSignalsVerificationStatus)evt.newValue;
                UpdateValues();
                OnValuesChanged?.Invoke();
            });
            
            _hasAgeLowerToggle.RegisterValueChangedCallback(evt => 
            {
                if (_ageSignalsResultUIAccess != null) _ageSignalsResultUIAccess._ageLowerHasValue = evt.newValue;
                UpdateValues();
                OnValuesChanged?.Invoke();
            });
            
            _ageLowerField.RegisterValueChangedCallback(evt => 
            {
                if (_ageSignalsResultUIAccess != null) _ageSignalsResultUIAccess._ageLower = evt.newValue;
                UpdateValues();
                OnValuesChanged?.Invoke();
            });
            
            _hasAgeUpperToggle.RegisterValueChangedCallback(evt => 
            {
                if (_ageSignalsResultUIAccess != null) _ageSignalsResultUIAccess._ageUppderHasValue = evt.newValue;
                UpdateValues();
                OnValuesChanged?.Invoke();
            });
            
            _ageUpperField.RegisterValueChangedCallback(evt => 
            {
                if (_ageSignalsResultUIAccess != null) _ageSignalsResultUIAccess._ageUpper = evt.newValue;
                UpdateValues();
                OnValuesChanged?.Invoke();
            });

            _hasMostRecentApprovalDateToggle.RegisterValueChangedCallback(evt => 
            {
                if (_ageSignalsResultUIAccess != null) _ageSignalsResultUIAccess._mostRecentApprovalDateHasValue = evt.newValue;
                UpdateValues();
                OnValuesChanged?.Invoke();
            });

            _mostRecentApprovalDateField.RegisterValueChangedCallback(evt => 
            {
                if(DateTime.TryParse(evt.newValue, out var approvalDate))
                {
                    if(_mostRecentApprovalDateField.ClassListContains("ageui-invalid-field"))
                    {
                        _mostRecentApprovalDateField.RemoveFromClassList("ageui-invalid-field");
                    }
                    if (_ageSignalsResultUIAccess != null) _ageSignalsResultUIAccess.MostRecentApprovalDate = approvalDate;
                    UpdateValues();
                    OnValuesChanged?.Invoke();
                }
                else
                {
                    if(!_mostRecentApprovalDateField.ClassListContains("ageui-invalid-field"))
                    {
                        _mostRecentApprovalDateField.AddToClassList("ageui-invalid-field");
                    }
                }
            });

            _installIdField.RegisterValueChangedCallback(evt => 
            {
                if (_ageSignalsResultUIAccess != null) _ageSignalsResultUIAccess.InstallId = evt.newValue;
                UpdateValues();
                OnValuesChanged?.Invoke();
            });
        }

        
        private void UpdatePreview()
        {
            if (_ageSignalsResultUIAccess == null) return;
            if (!_isPreview) return;

            var result = _ageSignalsResult;

            _previewUserStatusLabel.text = $"User Status: {(result.UserStatus?.ToString() ?? "null")}";
            _previewAgeLowerLabel.text = $"Age Lower: {(result.AgeLower?.ToString() ?? "null")}";
            _previewAgeUpperLabel.text = $"Age Upper: {(result.AgeUpper?.ToString() ?? "null")}";
            _previewMostRecentApprovalDateLabel.text = $"Most Recent Approval Date: {(result.MostRecentApprovalDate?.ToString("o") ?? "null")}";
            _previewInstallIdLabel.text = $"Install ID: {result.InstallId ?? "null"}";

            var json = result.ToJson(prettyPrint: true);
            _jsonPreviewField.value = json;
        }

        private void SetPreset(AgeSignalsVerificationStatus userStatus, int? ageLower, int? ageUpper, DateTime? approvalDate, string installId)
        {
            if (_ageSignalsResultUIAccess == null) return;

            _ageSignalsResultUIAccess._userStatusHasValue = true;
            _ageSignalsResultUIAccess._userStatus = userStatus;
            _ageSignalsResultUIAccess._ageLowerHasValue = ageLower.HasValue;
            if(ageLower.HasValue)
            {
                _ageSignalsResultUIAccess._ageLower = ageLower.Value;
            }
            _ageSignalsResultUIAccess._ageUppderHasValue = ageUpper.HasValue;
            if(ageUpper.HasValue)
            {
                _ageSignalsResultUIAccess._ageUpper = ageUpper.Value;
            }
            _ageSignalsResultUIAccess._mostRecentApprovalDateHasValue = approvalDate.HasValue;
            if(approvalDate.HasValue)
            {
                _ageSignalsResultUIAccess.MostRecentApprovalDate = approvalDate.Value;
            }
            _ageSignalsResultUIAccess.InstallId = installId;

            UpdateValues();
            OnValuesChanged?.Invoke();
        }
    }
}