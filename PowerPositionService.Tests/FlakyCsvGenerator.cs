using PowerPositionService.CSV;

namespace PowerPositionService.Tests;

internal class FlakyCsvGenerator(int failuresBeforeSuccess) : ICsvGenerator
{
    public int Attempts { get; private set; }
    
    public Task WriteCsvAsync(
        IEnumerable<(TimeOnly LocalTime, double Volume)> data,
        DateTimeOffset now,
        TimeZoneInfo tz,
        CancellationToken cancellationToken = default)
    {
        Attempts++;
        if (Attempts <= failuresBeforeSuccess) throw new Exception("Test CSV Generation Failed");
        
        return Task.CompletedTask;
    }
}