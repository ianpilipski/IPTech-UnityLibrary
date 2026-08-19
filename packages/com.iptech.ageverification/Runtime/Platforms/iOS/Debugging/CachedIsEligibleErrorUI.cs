using System;
using IPTech.AgeVerification.Debugging;
using UnityEngine;
using UnityEngine.UIElements;

namespace IPTech.AgeVerification.iOS.Debugging
{
    public class CachedIsEligibleErrorUI : VisualElement
    {
        public event System.Action OnValuesChanged;

        public CachedIsEligibleErrorUI(CachedIsEligibleError cachedError, bool isPreview = true)
        {
            var b = new MockPopupBuilder();
            this.Add(b.Root);

            if (!isPreview)
            {
                b.AddEnumProperty("Error Type", cachedError.Type)
                    .RegisterValueChangedCallback(evt => {
                        cachedError.Type = (CachedIsEligibleError.ErrorType)evt.newValue;
                        OnValuesChanged?.Invoke();
                    });

                b.AddTextFieldProperty("Error Message", cachedError.Message)
                    .RegisterValueChangedCallback(evt => {
                        cachedError.Message = evt.newValue;
                        OnValuesChanged?.Invoke();
                    });
            }
            else
            {
                b.BeginSection("Exception");
                b.AddLabelProperty($"Type: {cachedError.Type}");
                b.AddLabelProperty($"Message: {cachedError.Message}");
                b.EndSection();
            }
        }
    }
}