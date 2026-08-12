namespace Loscardos.ShopeeSdk.Core.Time;

public interface IShopeeClock
{
    DateTimeOffset UtcNow { get; }
    long UnixTimeSeconds { get; }
}
