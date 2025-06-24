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
                .ToTable("Users")
                .HasIndex(u => u.Email)
                .IsUnique();

            // Configure SellerProduct entity
            modelBuilder.Entity<SellerProduct>(entity =>
            {
                entity.ToTable("SellerProducts");
                entity.Property(p => p.PurchasePrice).HasPrecision(18, 2);
                entity.Property(p => p.SellingPrice).HasPrecision(18, 2);
            });

            // Configure CustomerOrder entity - THIS IS THE KEY FIX
            modelBuilder.Entity<CustomerOrder>(entity =>
            {
                entity.ToTable("CustomerOrders"); // Explicitly set table name
                entity.Property(o => o.SubTotal).HasPrecision(18, 2);
                entity.Property(o => o.Tax).HasPrecision(18, 2);
                entity.Property(o => o.ShippingCost).HasPrecision(18, 2);
                entity.Property(o => o.TotalAmount).HasPrecision(18, 2);
            });

            // Configure CustomerOrderItem entity
            modelBuilder.Entity<CustomerOrderItem>(entity =>
            {
                entity.ToTable("CustomerOrderItems"); // Explicitly set table name
                entity.Property(oi => oi.UnitPrice).HasPrecision(18, 2);

                // Configure relationships
                entity.HasOne(oi => oi.Order)
                    .WithMany(o => o.OrderItems)
                    .HasForeignKey(oi => oi.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(oi => oi.Product)
                    .WithMany(p => p.OrderItems)
                    .HasForeignKey(oi => oi.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

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
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
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
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );

            // Seed sample products for sellers
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
                    CurrentStock = 25,
                    DisplayStock = 25,
                    IsAvailable = true,
                    ImageUrl = "/images/office-chair-retail.jpg",
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
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
                    CurrentStock = 15,
                    DisplayStock = 15,
                    IsAvailable = true,
                    ImageUrl = "/images/standing-desk-retail.jpg",
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );
        }
    }
}