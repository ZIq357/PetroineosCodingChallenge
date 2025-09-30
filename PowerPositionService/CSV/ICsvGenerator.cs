namespace PowerPositionService.CSV;

public interface ICsvGenerator
{
    Task WriteCsvAsync(IEnumerable<(TimeOnly LocalTime, double Volume)> data, DateTimeOffset now, TimeZoneInfo tz, CancellationToken cancellationToken = default);
}