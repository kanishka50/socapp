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

        // Add new DbSets
        public DbSet<SellerDistributorOrder> DistributorOrders { get; set; }
        public DbSet<SellerDistributorOrderItem> DistributorOrderItems { get; set; }
        public DbSet<SellerInventoryTransaction> InventoryTransactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Existing configurations...
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

            // Configure CustomerOrder entity
            modelBuilder.Entity<CustomerOrder>(entity =>
            {
                entity.ToTable("CustomerOrders");
                entity.Property(o => o.SubTotal).HasPrecision(18, 2);
                entity.Property(o => o.Tax).HasPrecision(18, 2);
                entity.Property(o => o.ShippingCost).HasPrecision(18, 2);
                entity.Property(o => o.TotalAmount).HasPrecision(18, 2);
            });

            // Configure CustomerOrderItem entity
            modelBuilder.Entity<CustomerOrderItem>(entity =>
            {
                entity.ToTable("CustomerOrderItems");
                entity.Property(oi => oi.UnitPrice).HasPrecision(18, 2);

                entity.HasOne(oi => oi.Order)
                    .WithMany(o => o.OrderItems)
                    .HasForeignKey(oi => oi.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(oi => oi.Product)
                    .WithMany(p => p.OrderItems)
                    .HasForeignKey(oi => oi.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // NEW CONFIGURATIONS
            // Configure SellerDistributorOrder entity
            modelBuilder.Entity<SellerDistributorOrder>(entity =>
            {
                entity.ToTable("SellerDistributorOrders");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.OrderNumber).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.OrderNumber).IsUnique();
                entity.Property(e => e.DistributorOrderNumber).HasMaxLength(50);
                entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
                entity.Property(e => e.ShippingAddress).HasMaxLength(500);
                entity.Property(e => e.Notes).HasMaxLength(1000);
            });

            // Configure SellerDistributorOrderItem entity
            modelBuilder.Entity<SellerDistributorOrderItem>(entity =>
            {
                entity.ToTable("SellerDistributorOrderItems");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UnitPrice).HasPrecision(18, 2);

                entity.HasOne(e => e.Order)
                    .WithMany(o => o.OrderItems)
                    .HasForeignKey(e => e.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Product)
                    .WithMany()
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure SellerInventoryTransaction entity
            modelBuilder.Entity<SellerInventoryTransaction>(entity =>
            {
                entity.ToTable("SellerInventoryTransactions");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TransactionType).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Reference).HasMaxLength(100);
                entity.Property(e => e.Notes).HasMaxLength(500);
                entity.Property(e => e.UnitCost).HasPrecision(18, 2);
                entity.HasIndex(e => e.TransactionDate);
                entity.HasIndex(e => new { e.ProductId, e.TransactionDate });

                entity.HasOne(e => e.Product)
                    .WithMany()
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Seed initial data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Existing seed data remains the same...
            // Seed seller users
            modelBuilder.Entity<User>().HasData(
                 new User
                 {
                     Id = 1,
                     Email = "admin@seller.com",
                     // Static hash for "Admin123!"
                     PasswordHash = "$2a$11$RZvBwJL7e8OadjLPNgW7x.Km7NkkdVNoLSbEVmNGbxSu.Pnw8dXLa",
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
                    // Static hash for "Seller123!"
                    PasswordHash = "$2a$11$5VBm7OKqiM6XGQqATcQTb.SZeh7mr8fVyVuVC.M8FQ8g1jLOSGGBu",
                    FirstName = "Bob",
                    LastName = "Seller",
                    Role = UserRole.Seller,
                    IsActive = true,
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );

            // Existing product seed data...
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