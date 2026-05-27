using Microsoft.EntityFrameworkCore;
using RecuBackend.Api.Models;

namespace RecuBackend.Api.Data;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<BootstrapPing> BootstrapPings => Set<BootstrapPing>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<BootstrapPing>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Message).HasMaxLength(200);
            e.HasIndex(x => x.CreatedAtUtc);
        });
    }
}

