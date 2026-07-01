
using System.Threading;
using System.Threading.Tasks;

namespace IPTech.AgeVerification
{
    public interface IAgeVerification
    {
        Task<AgeVerificationResult> VerifyAge(int requiredMinAge, CancellationToken ct, int additionalMinAge1 = 0, int additionalMinAge2 = 0);
    }
}