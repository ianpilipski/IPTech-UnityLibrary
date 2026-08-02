
using System.Threading;
using System.Threading.Tasks;

namespace IPTech.AgeVerification
{
    public interface IAgeVerificationService
    {
        Task<AgeVerificationResult> RequestAgeData(int requiredMinAge, CancellationToken ct, int additionalMinAge1 = 0, int additionalMinAge2 = 0);
    }
}