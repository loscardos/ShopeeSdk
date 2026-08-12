using Loscardos.ShopeeSdk.Core.Time;

namespace Loscardos.ShopeeSdk.Tests.TestDoubles;

public sealed class FixedShopeeClock(DateTimeOffset utcNow) : IShopeeClock
{
    public DateTimeOffset UtcNow { get; } = utcNow;
    public long UnixTimeSeconds => UtcNow.ToUnixTimeSeconds();
}
