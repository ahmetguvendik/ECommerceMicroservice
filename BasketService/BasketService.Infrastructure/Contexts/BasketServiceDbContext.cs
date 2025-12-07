using Microsoft.EntityFrameworkCore;
using BasketService.Domain.Entities;

namespace BasketService.Infrastructure.Contexts;

public class BasketServiceDbContext : DbContext
{
    public BasketServiceDbContext(DbContextOptions<BasketServiceDbContext> options) : base(options)
    {
        
    }

    public DbSet<Basket> Baskets { get; set; }
    public DbSet<BasketItem> BasketItems { get; set; }
    
}

