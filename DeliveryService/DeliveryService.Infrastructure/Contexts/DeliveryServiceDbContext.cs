using DeliveryService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DeliveryService.Infrastructure.Contexts;

public class DeliveryServiceDbContext : DbContext
{
    public DeliveryServiceDbContext(DbContextOptions<DeliveryServiceDbContext> options) : base(options)
    {
    }

    public DbSet<Delivery> Deliveries { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Delivery>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.OrderId, x.CorrelationId }).IsUnique();
        });
    }
}

