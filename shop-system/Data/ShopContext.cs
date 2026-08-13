using Microsoft.EntityFrameworkCore;
using ShopSystem.Models;

namespace ShopSystem.Data;

/// <summary>
/// The EF Core DbContext: your gateway to the database.
/// Note it derives from DbContext, which implements IDisposable (rung 07) -
/// that's why DI hands you a fresh, scoped one per request and disposes it
/// for you at the end of the request.
/// </summary>
public class ShopContext : DbContext
{
    public ShopContext(DbContextOptions<ShopContext> options) : base(options) { }

    // Each DbSet<T> maps to a table. Generics (rung 05) everywhere.
    public DbSet<Reading> Readings => Set<Reading>();
}
