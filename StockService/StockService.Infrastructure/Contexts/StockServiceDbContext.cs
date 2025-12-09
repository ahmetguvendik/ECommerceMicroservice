using Microsoft.EntityFrameworkCore;
using StockService.Domain.Entities;

namespace StockService.Infrastructure.Contexts;

public class StockServiceDbContext : DbContext
{
    public StockServiceDbContext(DbContextOptions<StockServiceDbContext> options) : base(options)
    {
        
    }

    public DbSet<Stock> Stocks { get; set; }
}
