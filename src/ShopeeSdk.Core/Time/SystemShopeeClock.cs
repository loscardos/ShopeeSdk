namespace Loscardos.ShopeeSdk.Core.Time;

public sealed class SystemShopeeClock : IShopeeClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
    public long UnixTimeSeconds => UtcNow.ToUnixTimeSeconds();
}
