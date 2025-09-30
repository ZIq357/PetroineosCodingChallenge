using System.Globalization;
using CsvHelper;

namespace PowerPositionService.CSV;

public class CsvGenerator(IConfiguration configuration, ILogger<ICsvGenerator> logger) : ICsvGenerator
{
    private readonly string _directory = configuration.GetValue<string>("CsvOutputDirectory") 
                                         ?? throw new Exception("CSV output directory not set");

    public async Task WriteCsvAsync(
        IEnumerable<(TimeOnly LocalTime, double Volume)> data, 
        DateTimeOffset now,
        TimeZoneInfo tz, 
        CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(_directory);
        
        var local = TimeZoneInfo.ConvertTime(now, tz);
        var filename = $"PowerPosition_{local:yyyyMMdd}_{local:HHmm}.csv";

        var path = Path.Combine(_directory, filename);
        logger.LogInformation("Writing csv to {path}", path);

        await using var writer = new StreamWriter(path);
        await using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

        csv.Context.RegisterClassMap<CsvDataMap>();

        await csv.WriteRecordsAsync(data, cancellationToken);
    }
}