using PowerPositionService.Services;

namespace PowerPositionService;

public class PowerTradeAggregator(
    IPowerServiceAdapter powerService,
    ILogger<IPowerTradeAggregator> logger) : IPowerTradeAggregator
{
    public async Task<IReadOnlyList<(TimeOnly LocalTime, double Volume)>> AggregateTradesAsync(
        DateOnly tradeDate,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Fetching Power Trades for Date: {date} at {time}", tradeDate, DateTime.UtcNow);
        var trades = await powerService.GetTradesAsync(
            new DateTime(tradeDate.Year, tradeDate.Month, tradeDate.Day),
            cancellationToken);

        logger.LogDebug("Aggregating trade volumes");
        var powerPeriods = new double[24];
        foreach (var trade in trades)
        {
            foreach (var powerPeriod in trade.Periods)
            {
                if (powerPeriod.Period is < 1 or > 24)
                    throw new Exception($"Power Period: {powerPeriod.Period} Out-of-Range");

                powerPeriods[powerPeriod.Period - 1] += powerPeriod.Volume;
            }
        }

        var results = new List<(TimeOnly LocalTime, double Volume)>();
        for (var period = 1; period <= 24; period++)
        {
            var hour = ConvertPeriodToHour(period);
            results.Add((new TimeOnly(hour, 0), powerPeriods[period - 1]));
        }

        return results;
    }

    private int ConvertPeriodToHour(int period) => period == 1 ? 23 : period - 2;
}