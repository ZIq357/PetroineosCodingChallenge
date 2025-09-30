namespace PowerPositionService.Settings;

public sealed class CsvGeneratorSettings
{
    public required int IntervalMinutes { get; init; }
    public required int RetryAttempts { get; init; }
    public required int RetryDelaySeconds { get; init; }
}