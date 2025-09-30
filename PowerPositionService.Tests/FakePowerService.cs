using PowerPositionService.Services;
using Services;

namespace PowerPositionService.Tests;

internal class FakePowerService : IPowerServiceAdapter
{
    public Task<IEnumerable<PowerTrade>> GetTradesAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        var trade1 = PowerTrade.Create(new DateTime(2015, 04, 01), 24);
        foreach (var powerPeriod in trade1.Periods) powerPeriod.Volume = 100;
        
        var trade2 = PowerTrade.Create(new DateTime(2015, 04, 01), 24);
        foreach (var powerPeriod in trade2.Periods)
        {
            powerPeriod.Volume = powerPeriod.Period switch
            {
                < 12 => 50,
                >= 12 => -20
            };
        }
        
        return Task.FromResult<IEnumerable<PowerTrade>>([trade1, trade2]);
    }
}