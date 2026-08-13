using Microsoft.EntityFrameworkCore;
using ShopSystem.Data;
using ShopSystem.Models;

var builder = WebApplication.CreateBuilder(args);

// --- Dependency injection ---
// Register ShopContext with the DI container, backed by a local SQLite file.
// AddDbContext registers it as SCOPED: one instance per HTTP request, disposed
// automatically when the request ends (DbContext is IDisposable - rung 07).
builder.Services.AddDbContext<ShopContext>(options =>
    options.UseSqlite("Data Source=shop.db"));

var app = builder.Build();

// --- Startup: make sure the SQLite file + schema exist ---
// EnsureCreated is a quick MVP shortcut. We replace it with real EF Core
// migrations in slice 3.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ShopContext>();
    db.Database.EnsureCreated();
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

app.Run("http://localhost:5080");
