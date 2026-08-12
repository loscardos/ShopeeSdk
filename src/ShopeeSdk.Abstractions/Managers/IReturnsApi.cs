using Loscardos.ShopeeSdk.Generated.Returns;

namespace Loscardos.ShopeeSdk.Abstractions.Managers;

public interface IReturnsApi
{
    Task<GetReturnListResponse> GetReturnListAsync(long shopId, GetReturnListRequest request, CancellationToken cancellationToken);
}
