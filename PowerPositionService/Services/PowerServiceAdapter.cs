using Services;

namespace PowerPositionService.Services;

public class PowerServiceAdapter : IPowerServiceAdapter
{
    private readonly PowerService _powerService = new();
    
    public async Task<IEnumerable<PowerTrade>> GetTradesAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        // Task wrapper to support cancellation
        return await Task.Run(() => _powerService.GetTradesAsync(date), cancellationToken);
    }
}