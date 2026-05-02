using ShopeeSdk.Generated.Payment;

namespace ShopeeSdk.Abstractions.Managers;

public interface IPaymentApi
{
    Task<GetEscrowDetailResponse> GetEscrowDetailAsync(long shopId, GetEscrowDetailRequest request, CancellationToken cancellationToken);
}
