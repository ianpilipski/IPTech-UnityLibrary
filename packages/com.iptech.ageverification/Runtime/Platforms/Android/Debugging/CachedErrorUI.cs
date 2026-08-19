using IPTech.AgeVerification.Debugging;
using UnityEngine;
using UnityEngine.UIElements;

namespace IPTech.AgeVerification.Android.AgeSignals.Debugging
{
    public class CachedErrorUI : VisualElement
    {
        private CachedError _cachedError;
        private bool _isPreview;
        
        public CachedErrorUI(CachedError error, bool isPreview = false)
        {
            _cachedError = error;
            _isPreview = isPreview;
            ComposeUI();
        }

        private void ComposeUI()
        {
            this.Clear();

            var b = new MockPopupBuilder();
            this.Add(b.Root);

            if(!_isPreview) {
                var errorCodeField = b.AddEnumProperty("Error Code", AgeSignalsException.KnownErrorCodes.NO_ERROR);
                
                var errorTypeField = b.AddEnumProperty("Error Type", _cachedError.Type);
                errorTypeField.RegisterValueChangedCallback(evt =>
                {
                    _cachedError.Type = (CachedError.ErrorType)evt.newValue;
                    ToggleErrorCodeFieldVisibility(errorCodeField);
                });
                
                errorCodeField.value = AgeSignalsException.KnownErrorCodes.NO_ERROR;
                if(AgeSignalsException.TryGetKnownErrorCode(_cachedError.ErrorCode, out var knownError))
                {
                    errorCodeField.value = knownError;
                }
                errorCodeField.RegisterValueChangedCallback(evt =>
                {
                    _cachedError.ErrorCode = (int)(AgeSignalsException.KnownErrorCodes)evt.newValue;
                });
                var errorMessageField = b.AddTextFieldProperty("Error Message", _cachedError.Message);
                errorMessageField.RegisterValueChangedCallback(evt =>
                {
                    _cachedError.Message = evt.newValue;
                });
                
                ToggleErrorCodeFieldVisibility(errorCodeField);
            } 
            else
            {
                b.BeginSection("Generated Result:");
                b.AddLabelProperty($"Exception: {_cachedError.Type}");
                if(_cachedError.Type == CachedError.ErrorType.AgeSignalsException)
                {
                    var knownErrorStr = "Unknown";
                    if(_cachedError.ErrorCode!=0 && AgeSignalsException.TryGetKnownErrorCode(_cachedError.ErrorCode, out var knownError))
                    {
                       knownErrorStr = knownError.ToString();
                    }
                    b.AddLabelProperty($"Error Code: {knownErrorStr} ({_cachedError.ErrorCode})");
                }
                b.AddLabelProperty($"Exception Message: {_cachedError.Message}");
                b.EndSection();
            }
        }

        private void ToggleErrorCodeFieldVisibility(VisualElement errorCodeField)
        {
            if(_cachedError.Type == CachedError.ErrorType.AgeSignalsException)
            {
                errorCodeField.style.display = DisplayStyle.Flex;
            }
            else
            {
                errorCodeField.style.display = DisplayStyle.None;
            }
        }
    }
}