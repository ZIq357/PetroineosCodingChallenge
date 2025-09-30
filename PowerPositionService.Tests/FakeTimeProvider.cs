using PowerPositionService;

namespace PowerPositionService.Tests;

internal class FakeTimeProvider : ITimeProvider
{
    private DateTimeOffset NowValue { get; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UtcNow => NowValue;
    public TimeZoneInfo TimeZoneInfo => TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
}