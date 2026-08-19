
using IPTech.AgeVerification.Debugging;
using UnityEngine;
using UnityEngine.UIElements;

namespace IPTech.AgeVerification.iOS.Debugging
{
    public class CachedErrorUI : VisualElement
    {
        private CachedError _cachedError;
        private bool _isPreview;

        public CachedErrorUI(CachedError cachedError, bool isPreview = false)
        {
            _cachedError = cachedError;
            _isPreview = isPreview;
            ComposeUI();
        }

        private void ComposeUI()
        {
            this.Clear();
            var b = new MockPopupBuilder();

            if(!_isPreview) {
                var errorType = b.AddEnumProperty("Error Type", _cachedError.Type);
                errorType.RegisterValueChangedCallback(evt =>
                {
                    _cachedError.Type = (CachedError.ErrorType)evt.newValue;
                });
                
                var errorMessage = b.AddTextFieldProperty("Error Message", _cachedError.Message);
                errorMessage.RegisterValueChangedCallback(evt =>
                {
                    _cachedError.Message = evt.newValue;
                });
            } 
            else
            {
                b.BeginSection("Exception");
                b.AddLabelProperty($"Type: {_cachedError.Type}");
                b.AddLabelProperty($"Message: {_cachedError.Message}");
                b.EndSection();
            }

            this.Add(b.Root);
        }
    }
}