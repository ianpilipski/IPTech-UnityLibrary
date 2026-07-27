using UnityEngine;
using UnityEngine.UIElements;
using System;
using IPTech.AgeVerification.Debugging;

namespace IPTech.AgeVerification.Android.AgeSignals.Debugging
{
    public class MockResultUI
    {
        public event System.Action OnValuesChanged;
        
        private MockResult _mockResult;
        private VisualElement _root;
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
        private VisualElement _previewContainer;

        public MockResultUI(MockResult mockResult)
        {
            _mockResult = mockResult;

            var b = new MockPopupBuilder();
            _root = b.Root;

            CreateUserStatusSection(b);
            CreateAgeBoundsSection(b);
            CreateApprovalDateSection(b);
            CreateInstallIdSection(b);
            CreatePresetsSection(b);
            CreatePreviewSection(b);

            // Initialize values from MockResult
            UpdateValues();

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
            _previewContainer = b.BeginSection("Preview");

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
            
            
            for(int i=0; i<=5; i++)
            {
                switch(i)
                {
                    case 0:
                        b.AddButton("Verified (13-17)", () => SetPreset(true, AgeSignalsVerificationStatus.VERIFIED, true, 13, true, 17, false, "", "test-installid-123"));
                        break;
                    case 1:
                        b.AddButton("Supervised (5-12)", () => SetPreset(true, AgeSignalsVerificationStatus.SUPERVISED, true, 5, true, 12, true, DateTime.Now.ToString("o"), "test-installid-456"));
                        break;
                    case 2:
                        b.AddButton("Approval Pending", () => SetPreset(true, AgeSignalsVerificationStatus.SUPERVISED_APPROVAL_PENDING, false, 0, false, 0, false, "", "test-installid-789"));
                        break;
                    case 3:
                        b.AddButton("Approval Denied", () => SetPreset(true, AgeSignalsVerificationStatus.SUPERVISED_APPROVAL_DENIED, false, 0, false, 0, false, "", "test-installid-000"));
                        break;
                    case 4:
                        b.AddButton("Verified (18+)", () => SetPreset(true, AgeSignalsVerificationStatus.VERIFIED, true, 18, false, 0, false, "", "test-installid-adult"));
                        break;
                    case 5:
                        b.AddButton("Unknown Status", () => SetPreset(false, AgeSignalsVerificationStatus.UNKNOWN, false, 0, false, 0, false, "", "unknown-installid"));
                        break;
                    // Add more cases as needed
                }
            }
            b.EndSection();
        }

        private void UpdateValues()
        {
            if (_mockResult == null) return;

            if(_hasUserStatusToggle.value != _mockResult.HasUserStatus)
            {
                _hasUserStatusToggle.value = _mockResult.HasUserStatus;
            }
            if(_userStatusField.value != (Enum)_mockResult.UserStatus)
            {
                _userStatusField.value = _mockResult.UserStatus;
            }

            if(_hasAgeLowerToggle.value != _mockResult.HasAgeLower)
            {
                _hasAgeLowerToggle.value = _mockResult.HasAgeLower;
            }
            if(_ageLowerField.value != _mockResult.AgeLower)
            {
                _ageLowerField.value = _mockResult.AgeLower;
            }
            if(_hasAgeUpperToggle.value != _mockResult.HasAgeUpper)
            {
                _hasAgeUpperToggle.value = _mockResult.HasAgeUpper;
            }
            if(_ageUpperField.value != _mockResult.AgeUpper)
            {
                _ageUpperField.value = _mockResult.AgeUpper;
            }
            if(_hasMostRecentApprovalDateToggle.value != _mockResult.HasMostRecentApprovalDate)
            {
                _hasMostRecentApprovalDateToggle.value = _mockResult.HasMostRecentApprovalDate;
            }
            if(_mostRecentApprovalDateField.value != _mockResult.MostRecentApprovalDateString)
            {
                _mostRecentApprovalDateField.value = _mockResult.MostRecentApprovalDateString;
            }
            if(_installIdField.value != _mockResult.InstallId)
            {
                _installIdField.value = _mockResult.InstallId;
            }

            _userStatusField.SetEnabled(_hasUserStatusToggle.value);
            _userStatusField.style.opacity = _hasUserStatusToggle.value ? 1.0f : 0.5f;
            _ageLowerField.SetEnabled(_hasAgeLowerToggle.value);
            _ageLowerField.style.opacity = _hasAgeLowerToggle.value ? 1.0f : 0.5f;
            _ageUpperField.SetEnabled(_hasAgeUpperToggle.value);
            _ageUpperField.style.opacity = _hasAgeUpperToggle.value ? 1.0f : 0.5f;
            _mostRecentApprovalDateField.SetEnabled(_hasMostRecentApprovalDateToggle.value);
            _mostRecentApprovalDateField.style.opacity = _hasMostRecentApprovalDateToggle.value ? 1.0f : 0.5f;
            UpdatePreview();
        }

        private void RegisterCallbacks()
        {
            _hasUserStatusToggle.RegisterValueChangedCallback(evt => 
            {
                if (_mockResult != null) _mockResult.HasUserStatus = evt.newValue;
                UpdateValues();
                OnValuesChanged?.Invoke();
            });

            _userStatusField.RegisterValueChangedCallback(evt => 
            {
                if (_mockResult != null) _mockResult.UserStatus = (AgeSignalsVerificationStatus)evt.newValue;
                UpdateValues();
                OnValuesChanged?.Invoke();
            });
            
            _hasAgeLowerToggle.RegisterValueChangedCallback(evt => 
            {
                if (_mockResult != null) _mockResult.HasAgeLower = evt.newValue;
                UpdateValues();
                OnValuesChanged?.Invoke();
            });
            
            _ageLowerField.RegisterValueChangedCallback(evt => 
            {
                if (_mockResult != null) _mockResult.AgeLower = evt.newValue;
                UpdateValues();
                OnValuesChanged?.Invoke();
            });
            
            _hasAgeUpperToggle.RegisterValueChangedCallback(evt => 
            {
                if (_mockResult != null) _mockResult.HasAgeUpper = evt.newValue;
                UpdateValues();
                OnValuesChanged?.Invoke();
            });
            
            _ageUpperField.RegisterValueChangedCallback(evt => 
            {
                if (_mockResult != null) _mockResult.AgeUpper = evt.newValue;
                UpdateValues();
                OnValuesChanged?.Invoke();
            });

            _hasMostRecentApprovalDateToggle.RegisterValueChangedCallback(evt => 
            {
                if (_mockResult != null) _mockResult.HasMostRecentApprovalDate = evt.newValue;
                UpdateValues();
                OnValuesChanged?.Invoke();
            });

            _mostRecentApprovalDateField.RegisterValueChangedCallback(evt => 
            {
                if (_mockResult != null) _mockResult.MostRecentApprovalDateString = evt.newValue;
                UpdateValues();
                OnValuesChanged?.Invoke();
            });

            _installIdField.RegisterValueChangedCallback(evt => 
            {
                if (_mockResult != null) _mockResult.InstallId = evt.newValue;
                UpdateValues();
                OnValuesChanged?.Invoke();
            });
        }

        
        private void UpdatePreview()
        {
            if (_mockResult == null) return;

            var result = _mockResult.CreateResult();

            _previewUserStatusLabel.text = $"User Status: {(result.UserStatus?.ToString() ?? "null")}";
            _previewAgeLowerLabel.text = $"Age Lower: {(result.AgeLower?.ToString() ?? "null")}";
            _previewAgeUpperLabel.text = $"Age Upper: {(result.AgeUpper?.ToString() ?? "null")}";
            _previewMostRecentApprovalDateLabel.text = $"Most Recent Approval Date: {(result.MostRecentApprovalDate?.ToString("o") ?? "null")}";
            _previewInstallIdLabel.text = $"Install ID: {result.InstallId ?? "null"}";

            var json = result.ToJson(prettyPrint: true);
            _jsonPreviewField.value = json;
        }

        private void SetPreset(bool hasUserStatus, AgeSignalsVerificationStatus userStatus, bool hasAgeLower, int ageLower, bool hasAgeUpper, int ageUpper, bool hasApprovalDate, string approvalDateString, string installId)
        {
            if (_mockResult == null) return;

            // Update the MockResult data
            _mockResult.HasUserStatus = hasUserStatus;
            _mockResult.UserStatus = userStatus;
            _mockResult.HasAgeLower = hasAgeLower;
            _mockResult.AgeLower = ageLower;
            _mockResult.HasAgeUpper = hasAgeUpper;
            _mockResult.AgeUpper = ageUpper;
            _mockResult.HasMostRecentApprovalDate = hasApprovalDate;
            _mockResult.MostRecentApprovalDateString = approvalDateString;
            _mockResult.InstallId = installId;

            // Update UI elements to reflect the new values
            _hasUserStatusToggle.value = hasUserStatus;
            _userStatusField.value = userStatus;
            _hasAgeLowerToggle.value = hasAgeLower;
            _ageLowerField.value = ageLower;
            _hasAgeUpperToggle.value = hasAgeUpper;
            _ageUpperField.value = ageUpper;
            _hasMostRecentApprovalDateToggle.value = hasApprovalDate;
            _mostRecentApprovalDateField.value = approvalDateString;
            _installIdField.value = installId;

            UpdateValues();
            OnValuesChanged?.Invoke();
        }

        public AgeSignalsResult GetCurrentResult()
        {
            return _mockResult?.CreateResult();
        }
    }
}