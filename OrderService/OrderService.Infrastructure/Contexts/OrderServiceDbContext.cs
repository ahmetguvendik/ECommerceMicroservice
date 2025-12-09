using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entities;

namespace OrderService.Infrastructure.Contexts;

public class OrderServiceDbContext : DbContext
{
    public OrderServiceDbContext(DbContextOptions<OrderServiceDbContext> options) : base(options)
    {
        
    }

    public DbSet<Order> Type { get; set; }
    public DbSet<OrderItem> OrderItem { get; set; }
}