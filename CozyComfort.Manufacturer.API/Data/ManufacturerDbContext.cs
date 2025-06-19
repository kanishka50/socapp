using Microsoft.EntityFrameworkCore;
using CozyComfort.Manufacturer.API.Models.Entities;
using CozyComfort.Shared.Models;
using CozyComfort.Shared.Enums;

namespace CozyComfort.Manufacturer.API.Data
{
    public class ManufacturerDbContext : DbContext
    {
        public ManufacturerDbContext(DbContextOptions<ManufacturerDbContext> options)
            : base(options)
        {
        }

        public DbSet<ManufacturerProduct> Products { get; set; }
        public DbSet<InventoryTransaction> InventoryTransactions { get; set; }
        public DbSet<ProductMaterial> ProductMaterials { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure ManufacturerProduct
            modelBuilder.Entity<ManufacturerProduct>(entity =>
            {
                entity.ToTable("Products");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.SKU).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.SKU).IsUnique();
                entity.Property(e => e.Price).HasPrecision(18, 2);
                entity.Property(e => e.ManufacturingCost).HasPrecision(18, 2);
            });

            // Configure InventoryTransaction
            modelBuilder.Entity<InventoryTransaction>(entity =>
            {
                entity.ToTable("InventoryTransactions");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TransactionType).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Reference).HasMaxLength(100);

                entity.HasOne(e => e.Product)
                    .WithMany(p => p.InventoryTransactions)
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure ProductMaterial
            modelBuilder.Entity<ProductMaterial>(entity =>
            {
                entity.ToTable("ProductMaterials");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.MaterialName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Unit).IsRequired().HasMaxLength(20);
                entity.Property(e => e.CostPerUnit).HasPrecision(18, 2);

                entity.HasOne(e => e.Product)
                    .WithMany(p => p.ProductMaterials)
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure User
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            });

            // Seed initial data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed admin user
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Email = "admin@cozycomfort.com",
                    FirstName = "System",
                    LastName = "Administrator",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                    Role = UserRole.Administrator,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                },
                new User
                {
                    Id = 2,
                    Email = "manufacturer@cozycomfort.com",
                    FirstName = "John",
                    LastName = "Manufacturer",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Manufacturer123!"),
                    Role = UserRole.Manufacturer,
                    CompanyName = "Cozy Comfort Manufacturing",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                }
            );

            // Seed products
            modelBuilder.Entity<ManufacturerProduct>().HasData(
                new ManufacturerProduct
                {
                    Id = 1,
                    Name = "Luxury Wool Blanket - Queen Size",
                    Description = "Premium 100% Merino wool blanket, perfect for cold nights",
                    Material = "100% Merino Wool",
                    Size = "Queen (90x90 inches)",
                    Price = 199.99m,
                    SKU = "LWB-Q-001",
                    Category = "Wool Blankets",
                    MinStockLevel = 20,
                    CurrentStock = 50,
                    ReservedStock = 0,
                    ProductionCapacityPerDay = 10,
                    ManufacturingCost = 75.00m,
                    LeadTimeDays = 3,
                    ImageUrl = "/images/luxury-wool-queen.jpg",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                },
                new ManufacturerProduct
                {
                    Id = 2,
                    Name = "Cotton Comfort Blanket - King Size",
                    Description = "Soft and breathable 100% organic cotton blanket",
                    Material = "100% Organic Cotton",
                    Size = "King (108x90 inches)",
                    Price = 149.99m,
                    SKU = "CCB-K-001",
                    Category = "Cotton Blankets",
                    MinStockLevel = 25,
                    CurrentStock = 75,
                    ReservedStock = 5,
                    ProductionCapacityPerDay = 15,
                    ManufacturingCost = 55.00m,
                    LeadTimeDays = 2,
                    ImageUrl = "/images/cotton-comfort-king.jpg",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                }
            );
        }
    }
}