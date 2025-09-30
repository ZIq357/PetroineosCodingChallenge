using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using PowerPositionService;

namespace PowerPositionService.Tests;

public class Tests
{
    [Test]
    public async Task AggregateAsync_MapsPeriodsAndSumsCorrectly()
    {
        var powerTradeAggregator =
            new PowerTradeAggregator(new FakePowerService(), new NullLogger<IPowerTradeAggregator>());
        var data = await powerTradeAggregator
            .AggregateTradesAsync(new DateOnly(2015, 04, 01), CancellationToken.None);

        data.Should().HaveCount(24);
        data[0].LocalTime.Should().Be(new TimeOnly(23, 0));
        data[1].LocalTime.Should().Be(new TimeOnly(0, 0));
        data[10].LocalTime.Should().Be(new TimeOnly(9, 0));
        data[23].LocalTime.Should().Be(new TimeOnly(22, 0));

        foreach (var i in Enumerable.Range(0, 11)) data[i].Volume.Should().Be(150);  // 23:00–09:00
        foreach (var i in Enumerable.Range(11, 13)) data[i].Volume.Should().Be(80);  // 10:00–22:00
    }
}