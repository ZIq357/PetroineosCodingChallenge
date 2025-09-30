namespace PowerPositionService;

public interface IPowerTradeAggregator
{
    Task<IReadOnlyList<(TimeOnly LocalTime, double Volume)>> AggregateTradesAsync(DateOnly tradeDate, CancellationToken cancellationToken);
}   