using DemoAPI.Enums;
using Microsoft.EntityFrameworkCore;
using DemoAPI.Models;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DemoAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }

        public DbSet<User> Users { get; set; }

        public DbSet<Order> Orders { get; set; }

        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var orderStatusConverter = new ValueConverter<OrderStatus, string>(
                status => status.ToString(),
                value => Enum.Parse<OrderStatus>(value, true));

            modelBuilder.Entity<Order>()
                .Property(order => order.Status)
                .HasConversion(orderStatusConverter);

            base.OnModelCreating(modelBuilder);
        }
    }
}
