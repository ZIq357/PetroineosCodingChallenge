using CsvHelper.Configuration;

namespace PowerPositionService.CSV;

public sealed class CsvDataMap : ClassMap<(TimeOnly LocalTime, double Volume)>
{
    public CsvDataMap()
    {
        Map(x => x.LocalTime).Name("Local Time").Index(0);
        Map(x => x.Volume).Name("Volume").Index(1);
    }
}