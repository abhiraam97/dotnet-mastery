using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using ShopSystem.Data;
using ShopSystem.Models;
using ShopSystem.Services;

var builder = WebApplication.CreateBuilder(args);

// --- Dependency injection ---
// Register ShopContext with the DI container, backed by a local SQLite file.
// AddDbContext registers it as SCOPED: one instance per HTTP request, disposed
// automatically when the request ends (DbContext is IDisposable - rung 07).
builder.Services.AddDbContext<ShopContext>(options =>
    options.UseSqlite("Data Source=shop.db"));
builder.Services.AddHostedService<SensorSimulator>();
var app = builder.Build();

// --- Startup: make sure the SQLite file + schema exist ---
// EnsureCreated is a quick MVP shortcut. We replace it with real EF Core
// migrations in slice 3.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ShopContext>();
    db.Database.Migrate();
}

// --- Endpoints ---

// POST /readings : ingest one reading and store it.
// Note the async all the way down (rung 06): SaveChangesAsync, awaited.
app.MapPost("/readings", async (Reading reading, ShopContext db) =>
{
    reading.Id = 0;                       // ignore any client-supplied id
    reading.TimestampUtc = DateTime.UtcNow;
    db.Readings.Add(reading);
    await db.SaveChangesAsync();
    return Results.Created($"/readings/{reading.Id}", reading);
});

// GET /readings : list everything, newest first.
app.MapGet("/readings", async (ShopContext db) =>
    await db.Readings
            .OrderByDescending(r => r.TimestampUtc)
            .ToListAsync());

// GET /readings/latest : the newest reading for each sensor.
// We group in MEMORY here (client-side): EF Core can't cleanly translate
// "first row per group" to SQL, and would throw if forced. Fine at this scale.
app.MapGet("/readings/latest", async (ShopContext db) =>
{
    var all = await db.Readings.ToListAsync();
    return all.GroupBy(r => r.SensorId)
              .Select(g => g.OrderByDescending(r => r.TimestampUtc).First())
              .ToList();
});

// GET /readings/{sensorId}?since=<utc> : one sensor, optional time window.
// This one DOES translate to SQL and runs inside the database.
app.MapGet("/readings/{sensorId}", async (string sensorId, DateTime? since, ShopContext db) =>
    await db.Readings
            .Where(r => r.SensorId == sensorId && (since == null || r.TimestampUtc >= since))
            .OrderByDescending(r => r.TimestampUtc)
            .ToListAsync());

app.Run("http://localhost:5080");
