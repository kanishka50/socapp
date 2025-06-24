using Microsoft.EntityFrameworkCore;
using CozyComfort.Shared.Models;
using CozyComfort.Shared.Enums;
using CozyComfort.Seller.API.Models.Entities;

namespace CozyComfort.Seller.API.Data
{
    public class SellerDbContext : DbContext
    {
        public SellerDbContext(DbContextOptions<SellerDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<SellerProduct> SellerProducts { get; set; }
        public DbSet<CustomerOrder> Orders { get; set; }
        public DbSet<CustomerOrderItem> OrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Configure SellerProduct entity
            modelBuilder.Entity<SellerProduct>()
                .Property(p => p.PurchasePrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<SellerProduct>()
                .Property(p => p.SellingPrice)
                .HasPrecision(18, 2);

            // Configure CustomerOrder entity
            modelBuilder.Entity<CustomerOrder>()
                .Property(o => o.TotalAmount)
                .HasPrecision(18, 2);

            // Configure CustomerOrderItem entity
            modelBuilder.Entity<CustomerOrderItem>()
                .Property(oi => oi.UnitPrice)
                .HasPrecision(18, 2);

            // Configure relationships
            modelBuilder.Entity<CustomerOrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId);

            modelBuilder.Entity<CustomerOrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(oi => oi.ProductId);

            // Seed initial data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed seller users
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Email = "admin@seller.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                    FirstName = "Admin",
                    LastName = "Seller",
                    Role = UserRole.Administrator,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    Id = 2,
                    Email = "seller@cozycomfort.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Seller123!"),
                    FirstName = "Bob",
                    LastName = "Seller",
                    Role = UserRole.Seller,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            );

            // Seed sample products for sellers - using CurrentStock property
            modelBuilder.Entity<SellerProduct>().HasData(
                new SellerProduct
                {
                    Id = 1,
                    DistributorProductId = 1,
                    ProductName = "Office Chair - Retail",
                    Description = "Comfortable office chair for retail",
                    SKU = "CHAIR-RETAIL-001",
                    Category = "Chairs",
                    PurchasePrice = 250.00M,
                    SellingPrice = 399.99M,
                    CurrentStock = 25,  // Changed from Stock to CurrentStock
                    DisplayStock = 25,
                    IsAvailable = true,
                    ImageUrl = "/images/office-chair-retail.jpg",
                    CreatedAt = DateTime.UtcNow
                },
                new SellerProduct
                {
                    Id = 2,
                    DistributorProductId = 2,
                    ProductName = "Standing Desk - Retail",
                    Description = "Adjustable standing desk for retail",
                    SKU = "DESK-RETAIL-001",
                    Category = "Desks",
                    PurchasePrice = 500.00M,
                    SellingPrice = 799.99M,
                    CurrentStock = 15,  // Changed from Stock to CurrentStock
                    DisplayStock = 15,
                    IsAvailable = true,
                    ImageUrl = "/images/standing-desk-retail.jpg",
                    CreatedAt = DateTime.UtcNow
                }
            );
        }
    }
}