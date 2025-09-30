using PowerPositionService;
using PowerPositionService.CSV;
using PowerPositionService.Services;
using PowerPositionService.Settings;
using Serilog;
using Serilog.Events;
using Services;

var builder = Host.CreateApplicationBuilder(args);

// Configure Serilog
var logPath = builder.Configuration["LogPath"] ?? "logs/powerPositionService-.log";
var logLevelString = builder.Configuration["Logging:LogLevel:Default"] ?? "Information";
var level = Enum.TryParse<LogEventLevel>(logLevelString, true, out var parsedLevel)
    ? parsedLevel
    : LogEventLevel.Information;

const string logMessageTemplate =
    "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level:u3}] [T{ThreadId}] [{MachineName}] {SourceContext} {Message:lj}{NewLine}{Exception}";

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Is(level)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .WriteTo.Async(l => l.Console(outputTemplate: logMessageTemplate))
    .WriteTo.Async(l =>
        l.File(logPath, rollingInterval: RollingInterval.Day, shared: true, outputTemplate: logMessageTemplate))
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog(Log.Logger, dispose: true);

builder.Services.AddHostedService<CsvWorker>();

builder.Services.AddSingleton<ITimeProvider, LondonTimeProvider>();

builder.Services.AddSingleton<PowerService>();
builder.Services.AddSingleton<IPowerServiceAdapter, PowerServiceAdapter>();
builder.Services.AddSingleton<IPowerTradeAggregator, PowerTradeAggregator>();
builder.Services.AddSingleton<ICsvGenerator, CsvGenerator>();

builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "Power Position Service";
});

builder.Services.Configure<CsvGeneratorSettings>(builder.Configuration.GetSection("CsvGeneratorSettings"));

var host = builder.Build();
await host.RunAsync();