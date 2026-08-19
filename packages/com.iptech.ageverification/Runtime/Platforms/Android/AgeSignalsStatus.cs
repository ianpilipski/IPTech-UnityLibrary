
namespace IPTech.AgeVerification.Android.AgeSignals
{
    // these match the android AgeSignalsStatus api values
    public enum AgeSignalsAccessStatus
    {
        UNSPECIFIED = 0,
        SHARED = 1,
        NOT_SHARED = 2,
        VERIFICATION_REQUIRED = 3 // in a region that is required otherwise would be shared, verification required
    }
}