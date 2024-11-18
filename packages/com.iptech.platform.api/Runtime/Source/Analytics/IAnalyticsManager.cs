using System.Collections.Generic;

namespace IPTech.Platform {
    public interface IAnalyticsManager {
        void CustomData(string eventName);
        void CustomData(string eventName, IDictionary<string, object> eventParams);
        void AdImpression(AdImpressionParameters adImpression);
    }
}