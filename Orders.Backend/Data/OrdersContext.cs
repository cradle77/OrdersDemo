using Microsoft.EntityFrameworkCore;
using Orders.Shared;

namespace Orders.Backend.Data
{
    public class OrdersContext : DbContext
    {
        public OrdersContext(DbContextOptions<OrdersContext> options)
            : base(options)
        { }

        public DbSet<Product> Products { get; set; } 
    }
}
