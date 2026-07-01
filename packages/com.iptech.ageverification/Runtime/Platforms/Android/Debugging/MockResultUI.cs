using UnityEngine;
using UnityEngine.UIElements;
using System;

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

            _root = new VisualElement();

            // Add some styling
            _root.style.paddingTop = 10;
            _root.style.paddingBottom = 10;
            _root.style.paddingLeft = 5;
            _root.style.paddingRight = 5;

            CreateHeader();
            CreateUserStatusSection();
            CreateAgeBoundsSection();
            CreateApprovalDateSection();
            CreateInstallIdSection();
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
            var header = new Label("Mock Age Signals Result Configuration");
            header.style.fontSize = 16;
            header.style.unityFontStyleAndWeight = FontStyle.Bold;
            header.style.marginBottom = 10;
            _root.Add(header);
        }

        private void CreateUserStatusSection()
        {
            var statusContainer = new VisualElement();
            statusContainer.style.marginBottom = 15;

            var statusLabel = new Label("User Status");
            statusLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            statusLabel.style.marginBottom = 5;
            statusContainer.Add(statusLabel);

            var userStatusContainer = new VisualElement();
            userStatusContainer.style.flexDirection = FlexDirection.Row;
            userStatusContainer.style.alignItems = Align.Center;
            userStatusContainer.style.marginLeft = 15;

            _hasUserStatusToggle = new Toggle("Has User Status");
            userStatusContainer.Add(_hasUserStatusToggle);

            _userStatusField = new EnumField(AgeSignalsVerificationStatus.VERIFIED);
            _userStatusField.style.flexGrow = 1;
            _userStatusField.style.flexShrink = 1;
            _userStatusField.style.marginLeft = 10;
            userStatusContainer.Add(_userStatusField);

            statusContainer.Add(userStatusContainer);
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

            // Age Lower container
            var ageLowerContainer = new VisualElement();
            ageLowerContainer.style.flexDirection = FlexDirection.Row;
            ageLowerContainer.style.alignItems = Align.Center;
            ageLowerContainer.style.marginLeft = 15;
            ageLowerContainer.style.marginBottom = 5;

            _hasAgeLowerToggle = new Toggle("Has Age Lower");
            ageLowerContainer.Add(_hasAgeLowerToggle);

            _ageLowerField = new IntegerField();
            _ageLowerField.style.flexGrow = 1;
            _ageLowerField.style.flexShrink = 1;
            _ageLowerField.style.marginLeft = 10;
            ageLowerContainer.Add(_ageLowerField);

            boundsContainer.Add(ageLowerContainer);

            // Age Upper container
            var ageUpperContainer = new VisualElement();
            ageUpperContainer.style.flexDirection = FlexDirection.Row;
            ageUpperContainer.style.alignItems = Align.Center;
            ageUpperContainer.style.marginLeft = 15;

            _hasAgeUpperToggle = new Toggle("Has Age Upper");
            ageUpperContainer.Add(_hasAgeUpperToggle);

            _ageUpperField = new IntegerField();
            _ageUpperField.style.flexGrow = 1;
            _ageUpperField.style.flexShrink = 1;
            _ageUpperField.style.marginLeft = 10;
            ageUpperContainer.Add(_ageUpperField);

            boundsContainer.Add(ageUpperContainer);

            _root.Add(boundsContainer);
        }

        private void CreateApprovalDateSection()
        {
            var approvalContainer = new VisualElement();
            approvalContainer.style.marginBottom = 15;

            var approvalLabel = new Label("Most Recent Approval Date");
            approvalLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            approvalLabel.style.marginBottom = 5;
            approvalContainer.Add(approvalLabel);

            var dateContainer = new VisualElement();
            dateContainer.style.flexDirection = FlexDirection.Row;
            dateContainer.style.alignItems = Align.Center;
            dateContainer.style.marginLeft = 15;

            _hasMostRecentApprovalDateToggle = new Toggle("Has Approval Date");
            dateContainer.Add(_hasMostRecentApprovalDateToggle);

            _mostRecentApprovalDateField = new TextField();
            _mostRecentApprovalDateField.style.flexGrow = 1;
            _mostRecentApprovalDateField.style.flexShrink = 1;
            _mostRecentApprovalDateField.style.marginLeft = 10;
            approvalContainer.Add(dateContainer);
            approvalContainer.Add(_mostRecentApprovalDateField);

            _root.Add(approvalContainer);
        }

        private void CreateInstallIdSection()
        {
            var installIdContainer = new VisualElement();
            installIdContainer.style.marginBottom = 15;

            var installIdLabel = new Label("Install ID");
            installIdLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            installIdLabel.style.marginBottom = 5;
            installIdContainer.Add(installIdLabel);

            _installIdField = new TextField();
            _installIdField.style.marginLeft = 15;
            installIdContainer.Add(_installIdField);

            _root.Add(installIdContainer);
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

            _previewUserStatusLabel = new Label();
            _previewUserStatusLabel.style.marginLeft = 15;
            resultContainer.Add(_previewUserStatusLabel);

            _previewAgeLowerLabel = new Label();
            _previewAgeLowerLabel.style.marginLeft = 15;
            resultContainer.Add(_previewAgeLowerLabel);

            _previewAgeUpperLabel = new Label();
            _previewAgeUpperLabel.style.marginLeft = 15;
            resultContainer.Add(_previewAgeUpperLabel);

            _previewMostRecentApprovalDateLabel = new Label();
            _previewMostRecentApprovalDateLabel.style.marginLeft = 15;
            resultContainer.Add(_previewMostRecentApprovalDateLabel);

            _previewInstallIdLabel = new Label();
            _previewInstallIdLabel.style.marginLeft = 15;
            _previewInstallIdLabel.style.marginBottom = 10;
            resultContainer.Add(_previewInstallIdLabel);

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
            presetsContainer.style.marginBottom = 15;

            var presetsLabel = new Label("Quick Presets");
            presetsLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            presetsLabel.style.marginBottom = 10;
            presetsContainer.Add(presetsLabel);

            // First row of buttons
            var buttonRow1 = new VisualElement();
            buttonRow1.style.flexDirection = FlexDirection.Row;
            buttonRow1.style.marginBottom = 5;

            var verifiedButton = new Button(() => SetPreset(true, AgeSignalsVerificationStatus.VERIFIED, true, 13, true, 17, false, "", "test-install-123"))
            {
                text = "Verified (13-17)"
            };
            verifiedButton.style.flexGrow = 1;
            verifiedButton.style.marginRight = 5;
            buttonRow1.Add(verifiedButton);

            var supervisedButton = new Button(() => SetPreset(true, AgeSignalsVerificationStatus.SUPERVISED, true, 5, true, 12, true, DateTime.Now.ToString("o"), "test-install-456"))
            {
                text = "Supervised (5-12)"
            };
            supervisedButton.style.flexGrow = 1;
            buttonRow1.Add(supervisedButton);

            presetsContainer.Add(buttonRow1);

            // Second row of buttons
            var buttonRow2 = new VisualElement();
            buttonRow2.style.flexDirection = FlexDirection.Row;
            buttonRow2.style.marginBottom = 5;

            var pendingButton = new Button(() => SetPreset(true, AgeSignalsVerificationStatus.SUPERVISED_APPROVAL_PENDING, false, 0, false, 0, false, "", "test-install-789"))
            {
                text = "Approval Pending"
            };
            pendingButton.style.flexGrow = 1;
            pendingButton.style.marginRight = 5;
            buttonRow2.Add(pendingButton);

            var deniedButton = new Button(() => SetPreset(true, AgeSignalsVerificationStatus.SUPERVISED_APPROVAL_DENIED, false, 0, false, 0, false, "", "test-install-000"))
            {
                text = "Approval Denied"
            };
            deniedButton.style.flexGrow = 1;
            buttonRow2.Add(deniedButton);

            presetsContainer.Add(buttonRow2);

            // Third row
            var buttonRow3 = new VisualElement();
            buttonRow3.style.flexDirection = FlexDirection.Row;

            var verified18PlusButton = new Button(() => SetPreset(true, AgeSignalsVerificationStatus.VERIFIED, true, 18, false, 0, false, "", "test-install-adult"))
            {
                text = "Verified (18+)"
            };
            verified18PlusButton.style.flexGrow = 1;
            verified18PlusButton.style.marginRight = 5;
            buttonRow3.Add(verified18PlusButton);

            var unknownButton = new Button(() => SetPreset(false, AgeSignalsVerificationStatus.UNKNOWN, false, 0, false, 0, false, "", "unknown-install"))
            {
                text = "Unknown Status"
            };
            unknownButton.style.flexGrow = 1;
            buttonRow3.Add(unknownButton);

            presetsContainer.Add(buttonRow3);

            _root.Add(presetsContainer);
        }

        private void InitializeValues()
        {
            if (_mockResult == null) return;

            _hasUserStatusToggle.value = _mockResult.HasUserStatus;
            _userStatusField.value = _mockResult.UserStatus;
            _hasAgeLowerToggle.value = _mockResult.HasAgeLower;
            _ageLowerField.value = _mockResult.AgeLower;
            _hasAgeUpperToggle.value = _mockResult.HasAgeUpper;
            _ageUpperField.value = _mockResult.AgeUpper;
            _hasMostRecentApprovalDateToggle.value = _mockResult.HasMostRecentApprovalDate;
            _mostRecentApprovalDateField.value = _mockResult.MostRecentApprovalDateString;
            _installIdField.value = _mockResult.InstallId;

            UpdateUserStatusFieldState();
            UpdateAgeLowerFieldState();
            UpdateAgeUpperFieldState();
            UpdateApprovalDateFieldState();
        }

        private void RegisterCallbacks()
        {
            _hasUserStatusToggle.RegisterValueChangedCallback(evt => 
            {
                if (_mockResult != null) _mockResult.HasUserStatus = evt.newValue;
                UpdateUserStatusFieldState();
                UpdatePreview();
                OnValuesChanged?.Invoke();
            });

            _userStatusField.RegisterValueChangedCallback(evt => 
            {
                if (_mockResult != null) _mockResult.UserStatus = (AgeSignalsVerificationStatus)evt.newValue;
                UpdatePreview();
                OnValuesChanged?.Invoke();
            });
            
            _hasAgeLowerToggle.RegisterValueChangedCallback(evt => 
            {
                if (_mockResult != null) _mockResult.HasAgeLower = evt.newValue;
                UpdateAgeLowerFieldState();
                UpdatePreview();
                OnValuesChanged?.Invoke();
            });
            
            _ageLowerField.RegisterValueChangedCallback(evt => 
            {
                if (_mockResult != null) _mockResult.AgeLower = evt.newValue;
                UpdatePreview();
                OnValuesChanged?.Invoke();
            });
            
            _hasAgeUpperToggle.RegisterValueChangedCallback(evt => 
            {
                if (_mockResult != null) _mockResult.HasAgeUpper = evt.newValue;
                UpdateAgeUpperFieldState();
                UpdatePreview();
                OnValuesChanged?.Invoke();
            });
            
            _ageUpperField.RegisterValueChangedCallback(evt => 
            {
                if (_mockResult != null) _mockResult.AgeUpper = evt.newValue;
                UpdatePreview();
                OnValuesChanged?.Invoke();
            });

            _hasMostRecentApprovalDateToggle.RegisterValueChangedCallback(evt => 
            {
                if (_mockResult != null) _mockResult.HasMostRecentApprovalDate = evt.newValue;
                UpdateApprovalDateFieldState();
                UpdatePreview();
                OnValuesChanged?.Invoke();
            });

            _mostRecentApprovalDateField.RegisterValueChangedCallback(evt => 
            {
                if (_mockResult != null) _mockResult.MostRecentApprovalDateString = evt.newValue;
                UpdatePreview();
                OnValuesChanged?.Invoke();
            });

            _installIdField.RegisterValueChangedCallback(evt => 
            {
                if (_mockResult != null) _mockResult.InstallId = evt.newValue;
                UpdatePreview();
                OnValuesChanged?.Invoke();
            });

            // Initial state
            UpdateUserStatusFieldState();
            UpdateAgeLowerFieldState();
            UpdateAgeUpperFieldState();
            UpdateApprovalDateFieldState();
        }

        private void UpdateUserStatusFieldState()
        {
            _userStatusField.SetEnabled(_hasUserStatusToggle.value);
            _userStatusField.style.opacity = _hasUserStatusToggle.value ? 1.0f : 0.5f;
        }

        private void UpdateAgeLowerFieldState()
        {
            _ageLowerField.SetEnabled(_hasAgeLowerToggle.value);
            _ageLowerField.style.opacity = _hasAgeLowerToggle.value ? 1.0f : 0.5f;
        }

        private void UpdateAgeUpperFieldState()
        {
            _ageUpperField.SetEnabled(_hasAgeUpperToggle.value);
            _ageUpperField.style.opacity = _hasAgeUpperToggle.value ? 1.0f : 0.5f;
        }

        private void UpdateApprovalDateFieldState()
        {
            _mostRecentApprovalDateField.SetEnabled(_hasMostRecentApprovalDateToggle.value);
            _mostRecentApprovalDateField.style.opacity = _hasMostRecentApprovalDateToggle.value ? 1.0f : 0.5f;
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

            UpdateUserStatusFieldState();
            UpdateAgeLowerFieldState();
            UpdateAgeUpperFieldState();
            UpdateApprovalDateFieldState();
            UpdatePreview();
            OnValuesChanged?.Invoke();
        }

        public void RefreshFromMockResult()
        {
            if (_mockResult == null) return;
            
            InitializeValues();
            UpdatePreview();
        }

        public AgeSignalsResult GetCurrentResult()
        {
            return _mockResult?.CreateResult();
        }
    }
}