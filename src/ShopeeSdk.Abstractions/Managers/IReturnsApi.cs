using ShopeeSdk.Generated.Returns;

namespace ShopeeSdk.Abstractions.Managers;

public interface IReturnsApi
{
    Task<GetReturnListResponse> GetReturnListAsync(long shopId, GetReturnListRequest request, CancellationToken cancellationToken);
}
