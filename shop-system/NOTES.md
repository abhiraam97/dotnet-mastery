# ShopSense - Phase 2 build

A monitoring system for a shop. Simulated sensors first; real hardware plugs
into the same ingest endpoint later.

## Build slices

1. **Data layer + ingest** (this slice): `Reading`, `ShopContext`, `POST /readings`,
   `GET /readings`, over SQLite.
2. Simulated sensor as a `BackgroundService` feeding readings automatically.
3. Query endpoints (latest per sensor, time filter) + EF Core **migrations**
   (retire `EnsureCreated`).
4. Alerting rule (fridge over threshold, door open too long).
5. Blazor dashboard.

## Slice 1: what's here and why

- **`Models/Reading.cs`** - one measurement (sensor, metric, value, timestamp).
- **`Data/ShopContext.cs`** - the EF Core `DbContext`. `DbSet<Reading>` maps to a
  table. `DbContext` is `IDisposable` (rung 07).
- **`Program.cs`** - ASP.NET Core minimal API:
  - `AddDbContext` registers `ShopContext` in DI as **scoped** (one per request,
    auto-disposed).
  - `EnsureCreated()` builds the SQLite file/schema on startup (MVP shortcut).
  - `POST /readings` stores a reading; `GET /readings` lists them. Both **async**
    all the way (rung 06).

## Run it

```
cd shop-system
dotnet run
```

App listens on http://localhost:5080. A `shop.db` SQLite file appears in the
folder (git-ignored).

## Test it

Use `requests.http` (VS Code REST Client / Rider), or curl:

```
curl -X POST http://localhost:5080/readings ^
  -H "Content-Type: application/json" ^
  -d "{\"sensorId\":\"fridge-1\",\"metric\":\"temperature_c\",\"value\":4.5}"

curl http://localhost:5080/readings
```

(The `^` is the Windows line-continuation; on one line you can drop it.)

## Fundamentals showing up in real code

- Rung 05 generics: `DbSet<Reading>`, `List<Reading>` everywhere, no boxing.
- Rung 06 async: `SaveChangesAsync`, `ToListAsync`, awaited end to end.
- Rung 07 IDisposable: `DbContext` is disposable; DI scopes and disposes it per
  request so you never write `using` around it in an endpoint.
