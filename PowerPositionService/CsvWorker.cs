using Microsoft.Extensions.Options;
using PowerPositionService.CSV;
using PowerPositionService.Settings;

namespace PowerPositionService;

public class CsvWorker(
    IPowerTradeAggregator powerTradeAggregator,
    ITimeProvider timeProvider,
    ICsvGenerator csvGenerator,
    IOptions<CsvGeneratorSettings> csvGeneratorSettings,
    ILogger<CsvWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var csvIntervalMinutes = csvGeneratorSettings.Value.IntervalMinutes;
        logger.LogInformation("Power Position Service starting. Interval {interval} minutes.", csvIntervalMinutes);

        var interval = TimeSpan.FromMinutes(csvIntervalMinutes);

        await GenerateCsv(cancellationToken);
        
        var now = DateTimeOffset.Now;
        var next = now + interval;
        
        while (!cancellationToken.IsCancellationRequested)
        {
            var current = timeProvider.UtcNow;
            if (current < next)
            {
                var delay = next - current;
                try
                {
                    await Task.Delay(delay, cancellationToken);
                }
                catch (TaskCanceledException e)
                {
                    break;
                }
                continue;
            }

            while (current >= next && !cancellationToken.IsCancellationRequested)
            {
                await GenerateCsv(cancellationToken);
                next += interval;
                current = timeProvider.UtcNow;
            }
        }
        
        logger.LogInformation("Power Position Service stopped.");
    }

    private async Task GenerateCsv(CancellationToken cancellationToken)
    {
        logger.LogInformation("Running CSV Generation Task");

        try
        {
            var now = timeProvider.UtcNow;
            var tz = timeProvider.TimeZoneInfo;
            var localNow = TimeZoneInfo.ConvertTime(now, tz);
            var tradeDate = DateOnly.FromDateTime(localNow.Date);

            var maxAttempts = csvGeneratorSettings.Value.RetryAttempts;
            var delay = TimeSpan.FromSeconds(csvGeneratorSettings.Value.RetryDelaySeconds);

            var currentAttempt = 0;
            while (currentAttempt <= maxAttempts)
            {
                logger.LogDebug("Running CSV Task Attempt: {attempt} at {now}", currentAttempt + 1, timeProvider.UtcNow);
                var success = await GenerateCsvInternal(tradeDate, now, tz, cancellationToken);

                if (success)
                {
                    logger.LogInformation("CSV Generation Task succeeded");
                    break;
                }
                
                currentAttempt++;
                await Task.Delay(delay, cancellationToken);
            }

            if (currentAttempt > maxAttempts)
            {
                logger.LogError("CSV Generation Task failed after {maxAttempts} attempts", maxAttempts);
                throw new Exception("CSV Generation Task failed after {maxAttempts} attempts");
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("CSV Generation Task was cancelled.");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error generating CSV");
        }
    }

    private async Task<bool> GenerateCsvInternal(DateOnly tradeDate, DateTimeOffset now, TimeZoneInfo tz, CancellationToken cancellationToken)
    {
        try
        {
            var data = await powerTradeAggregator.AggregateTradesAsync(tradeDate, cancellationToken);
            await csvGenerator.WriteCsvAsync(data, now, tz, cancellationToken);

            return true;
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "CSV Generation attempt failed. Retrying");
            return false;
        }
    }
}