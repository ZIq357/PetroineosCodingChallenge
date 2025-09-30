namespace PowerPositionService;

public class LondonTimeProvider : ITimeProvider
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
    public TimeZoneInfo TimeZoneInfo => TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
}