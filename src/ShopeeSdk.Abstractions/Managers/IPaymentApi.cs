using Loscardos.ShopeeSdk.Generated.Payment;

namespace Loscardos.ShopeeSdk.Abstractions.Managers;

public interface IPaymentApi
{
    Task<GetEscrowDetailResponse> GetEscrowDetailAsync(long shopId, GetEscrowDetailRequest request, CancellationToken cancellationToken);
}
