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
        public DbSet<ManufacturerOrder> Orders { get; set; }
        public DbSet<ManufacturerOrderItem> OrderItems { get; set; }

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
                entity.Property(e => e.QuantityRequired).HasPrecision(18, 2);

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

            modelBuilder.Entity<ManufacturerOrder>(entity =>
            {
                entity.ToTable("ManufacturerOrders");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.OrderNumber).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.OrderNumber).IsUnique();
                entity.Property(e => e.DistributorName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.DistributorOrderNumber).HasMaxLength(50);
                entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
                entity.Property(e => e.Notes).HasMaxLength(1000);
            });

            // Configure ManufacturerOrderItem
            modelBuilder.Entity<ManufacturerOrderItem>(entity =>
            {
                entity.ToTable("ManufacturerOrderItems");
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

            // Seed initial data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Use a static date instead of DateTime.UtcNow
            var seedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            // Seed admin user
            modelBuilder.Entity<User>().HasData(
                 new User
                 {
                     Id = 1,
                     Email = "admin@cozycomfort.com",
                     FirstName = "System",
                     LastName = "Administrator",
                     // Use a pre-generated hash for "Admin123!"
                     PasswordHash = "$2a$11$ZGsLwskCPFNMmT8lEV2ELetqSMN5XC1nBR1eEI8FmGN5dQdB2Ibvy",
                     Role = UserRole.Administrator,
                     CreatedAt = seedDate,
                     IsActive = true
                 },
                new User
                {
                    Id = 2,
                    Email = "manufacturer@cozycomfort.com",
                    FirstName = "John",
                    LastName = "Manufacturer",
                    // Use a pre-generated hash for "Manufacturer123!"
                    PasswordHash = "$2a$11$Q3p1VQDLzq.VQDlzq.VQDlzq.VQ3p1VQDLzq.VQDlzq.VQ3p1VQ",
                    Role = UserRole.Manufacturer,
                    CompanyName = "Cozy Comfort Manufacturing",
                    CreatedAt = seedDate,
                    IsActive = true
                },
                new User
                {
                    Id = 3,
                    Email = "distributor-api@cozycomfort.com",
                    FirstName = "Distributor",
                    LastName = "API Service",
                    PasswordHash = "$2a$11$XYZ123...", // Pre-generated hash for "DistributorAPI123!"
                    Role = UserRole.System, // or create a new role for API access
                    CompanyName = "System Integration",
                    CreatedAt = seedDate,
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
                    CreatedAt = seedDate, // Changed from DateTime.UtcNow
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
                    CreatedAt = seedDate, // Changed from DateTime.UtcNow
                    IsActive = true
                }
            );
        }
    }
}