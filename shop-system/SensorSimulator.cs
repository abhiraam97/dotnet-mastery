using ShopSystem.Data;
using ShopSystem.Models;

namespace ShopSystem.Services;

// A BackgroundService acting as your sensors: every few seconds it emits a
// fridge temperature and (sometimes) a door-open event, and stores them itself.
//
// KEY GOTCHA (rung 07 DI lifetimes): a BackgroundService is a SINGLETON, but
// ShopContext is SCOPED. Injecting ShopContext straight into a singleton would
// capture ONE context for the whole app life (a "captive dependency" bug).
// Instead inject IServiceScopeFactory and open a FRESH scope each loop.
public class SensorSimulator : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SensorSimulator> _logger;
    private readonly Random _rng = new();

    public SensorSimulator(IServiceScopeFactory scopeFactory, ILogger<SensorSimulator> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Sensor simulator started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var readings = new List<Reading>
            {
                new()
                {
                    SensorId = "fridge-1",
                    Metric = "temperature_c",
                    Value = Math.Round(3.5 + _rng.NextDouble() * 3.0, 2), // ~3.5-6.5 C
                    TimestampUtc = DateTime.UtcNow
                }
            };

            if (_rng.Next(0, 5) == 0)   // ~1 tick in 5, the door opens
            {
                readings.Add(new Reading
                {
                    SensorId = "front-door",
                    Metric = "door_open",
                    Value = 1,
                    TimestampUtc = DateTime.UtcNow
                });
            }

            using (var scope = _scopeFactory.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ShopContext>();
                db.Readings.AddRange(readings);
                await db.SaveChangesAsync(stoppingToken);
            }

            _logger.LogInformation("Emitted {Count} reading(s).", readings.Count);

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
            }
            catch (TaskCanceledException)
            {
                break; // Ctrl+C cancels the delay; exit cleanly
            }
        }

        _logger.LogInformation("Sensor simulator stopping.");
    }
}