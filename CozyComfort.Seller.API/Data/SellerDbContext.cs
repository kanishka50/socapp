using Microsoft.EntityFrameworkCore;
using CozyComfort.Seller.API.Models.Entities;
using CozyComfort.Shared.Models;
using CozyComfort.Shared.Enums;

namespace CozyComfort.Seller.API.Data
{
    public class SellerDbContext : DbContext
    {
        public SellerDbContext(DbContextOptions<SellerDbContext> options)
            : base(options)
        {
        }

        public DbSet<SellerProduct> Products { get; set; }
        public DbSet<CustomerOrder> Orders { get; set; }
        public DbSet<CustomerOrderItem> OrderItems { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure SellerProduct
            modelBuilder.Entity<SellerProduct>(entity =>
            {
                entity.ToTable("SellerProducts");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ProductName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.SKU).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.SKU).IsUnique();
                entity.Property(e => e.PurchasePrice).HasPrecision(18, 2);
                entity.Property(e => e.SellingPrice).HasPrecision(18, 2);
            });

            // Configure CustomerOrder
            modelBuilder.Entity<CustomerOrder>(entity =>
            {
                entity.ToTable("CustomerOrders");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.OrderNumber).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.OrderNumber).IsUnique();
                entity.Property(e => e.SubTotal).HasPrecision(18, 2);
                entity.Property(e => e.Tax).HasPrecision(18, 2);
                entity.Property(e => e.ShippingCost).HasPrecision(18, 2);
                entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
            });

            // Configure CustomerOrderItem
            modelBuilder.Entity<CustomerOrderItem>(entity =>
            {
                entity.ToTable("CustomerOrderItems");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UnitPrice).HasPrecision(18, 2);

                entity.HasOne(e => e.Order)
                    .WithMany(o => o.OrderItems)
                    .HasForeignKey(e => e.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Product)
                    .WithMany(p => p.OrderItems)
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Seed data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed seller user
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Email = "seller@cozycomfort.com",
                    FirstName = "Sarah",
                    LastName = "Seller",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Seller123!"),
                    Role = UserRole.Seller,
                    CompanyName = "Comfort Retail Store",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                }
            );

            // Seed seller products
            modelBuilder.Entity<SellerProduct>().HasData(
                new SellerProduct
                {
                    Id = 1,
                    DistributorProductId = 1,
                    ProductName = "Luxury Wool Blanket - Queen Size",
                    SKU = "LWB-Q-001",
                    Description = "Experience ultimate comfort with our premium Merino wool blanket",
                    Category = "Premium Blankets",
                    PurchasePrice = 259.99m,
                    SellingPrice = 349.99m,
                    CurrentStock = 10,
                    DisplayStock = 10,
                    IsAvailable = true,
                    ImageUrl = "/images/luxury-wool-queen.jpg",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                },
                new SellerProduct
                {
                    Id = 2,
                    DistributorProductId = 2,
                    ProductName = "Cotton Comfort Blanket - King Size",
                    SKU = "CCB-K-001",
                    Description = "Breathable organic cotton blanket perfect for all seasons",
                    Category = "Cotton Blankets",
                    PurchasePrice = 199.99m,
                    SellingPrice = 279.99m,
                    CurrentStock = 15,
                    DisplayStock = 15,
                    IsAvailable = true,
                    ImageUrl = "/images/cotton-comfort-king.jpg",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                }
            );
        }
    }
}