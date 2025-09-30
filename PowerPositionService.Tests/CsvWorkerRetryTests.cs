using System.Reflection;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using PowerPositionService;
using PowerPositionService.Settings;

namespace PowerPositionService.Tests;

public class CsvWorkerRetryTests
{
    private IOptions<CsvGeneratorSettings> GetSettings(int retryCount, int retryDelaySeconds) => Options.Create(
        new CsvGeneratorSettings
        {
            IntervalMinutes = 5,
            RetryAttempts = retryCount,
            RetryDelaySeconds = retryDelaySeconds
        });

    private async Task InvokeGenerateCsvTask(CsvWorker csvWorker, CancellationToken cancellationToken)
    {
        var methodInfo = csvWorker.GetType().GetMethod("GenerateCsv", BindingFlags.Instance | BindingFlags.NonPublic);
        var task = (Task)methodInfo?.Invoke(csvWorker, [cancellationToken])!;

        await task;
    }

    [Test]
    public async Task TestGenerateCsvRetryAndSucceed()
    {
        var flakyCsvGenerator = new FlakyCsvGenerator(2);
        
        var csvWorker = new CsvWorker(
            new StaticPowerTradeAggregator(),
            new FakeTimeProvider(),
            flakyCsvGenerator,
            GetSettings(2, 0),
            new NullLogger<CsvWorker>());
        
        await InvokeGenerateCsvTask(csvWorker, CancellationToken.None);

        flakyCsvGenerator.Attempts.Should().Be(3);
    }

    [Test]
    public async Task TestGenerateCsvRetryAndFailAfterAllAttempts()
    {
        var flakyCsvGenerator = new FlakyCsvGenerator(10);
        
        var csvWorker = new CsvWorker(
            new StaticPowerTradeAggregator(),
            new FakeTimeProvider(),
            flakyCsvGenerator,
            GetSettings(3, 0),
            new NullLogger<CsvWorker>());
        
        await InvokeGenerateCsvTask(csvWorker, CancellationToken.None);

        flakyCsvGenerator.Attempts.Should().Be(4);
    }
}