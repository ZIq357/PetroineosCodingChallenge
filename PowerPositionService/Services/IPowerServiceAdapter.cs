using Services;

namespace PowerPositionService.Services;

public interface IPowerServiceAdapter
{
    Task<IEnumerable<PowerTrade>> GetTradesAsync(
        DateTime date,
        CancellationToken cancellationToken = default);
}