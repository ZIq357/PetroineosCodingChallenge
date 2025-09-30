using PowerPositionService;

namespace PowerPositionService.Tests;

internal class StaticPowerTradeAggregator : IPowerTradeAggregator
{
    private readonly IReadOnlyList<(TimeOnly, double)> _data = Enumerable.Range(1, 24)
        .Select(i => (new TimeOnly(i switch
        {
            1 => 23,
            _ => i-2
        },0), 1.0))
        .ToList();
    
    public Task<IReadOnlyList<(TimeOnly LocalTime, double Volume)>> AggregateTradesAsync(
        DateOnly tradeDate,
        CancellationToken cancellationToken) => Task.FromResult(_data);
}