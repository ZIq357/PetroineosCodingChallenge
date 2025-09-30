namespace PowerPositionService;

public interface ITimeProvider
{
    DateTimeOffset UtcNow { get; }
    TimeZoneInfo TimeZoneInfo { get; }
}