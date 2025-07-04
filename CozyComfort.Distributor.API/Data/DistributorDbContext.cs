﻿using Microsoft.EntityFrameworkCore;
using CozyComfort.Distributor.API.Models.Entities;
using CozyComfort.Shared.Models;
using CozyComfort.Shared.Enums;

namespace CozyComfort.Distributor.API.Data
{
    public class DistributorDbContext : DbContext
    {
        public DistributorDbContext(DbContextOptions<DistributorDbContext> options)
            : base(options)
        {
        }

        public DbSet<DistributorProduct> Products { get; set; }
        public DbSet<DistributorOrder> Orders { get; set; }
        public DbSet<DistributorOrderItem> OrderItems { get; set; }
        public DbSet<DistributorInventoryTransaction> InventoryTransactions { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure DistributorProduct
            modelBuilder.Entity<DistributorProduct>(entity =>
            {
                entity.ToTable("DistributorProducts");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ProductName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.SKU).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.SKU).IsUnique();
                entity.Property(e => e.PurchasePrice).HasPrecision(18, 2);
                entity.Property(e => e.SellingPrice).HasPrecision(18, 2);

                // ADD THIS LINE for ManufacturerProductId
                entity.Property(e => e.ManufacturerProductId).IsRequired(false);

                // Add missing BaseEntity configurations
                entity.Property(e => e.CreatedBy).HasMaxLength(100);
                entity.Property(e => e.UpdatedBy).HasMaxLength(100);
                entity.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);
            });

            // Configure DistributorOrder - ADD ALL MISSING FIELD CONFIGURATIONS
            modelBuilder.Entity<DistributorOrder>(entity =>
            {
                entity.ToTable("DistributorOrders");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.OrderNumber).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.OrderNumber).IsUnique();
                entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
                entity.Property(e => e.ShippingAddress).IsRequired().HasMaxLength(500);

                // ADD MISSING FIELD CONFIGURATIONS:
                entity.Property(e => e.OrderType).HasConversion<int>().IsRequired();
                entity.Property(e => e.ManufacturerId).IsRequired(false);
                entity.Property(e => e.SellerId).IsRequired(false);
                entity.Property(e => e.CustomerId).IsRequired(false);
                entity.Property(e => e.CustomerOrderNumber).HasMaxLength(100);
                entity.Property(e => e.CustomerName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Status).HasConversion<int>().IsRequired();
                entity.Property(e => e.OrderDate).IsRequired();
                entity.Property(e => e.ExpectedDeliveryDate).IsRequired(false);
                entity.Property(e => e.ActualDeliveryDate).IsRequired(false);
                entity.Property(e => e.Notes).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.CreatedBy).HasMaxLength(100);
                entity.Property(e => e.UpdatedBy).HasMaxLength(100);
                entity.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);
            });

            // Configure DistributorOrderItem
            modelBuilder.Entity<DistributorOrderItem>(entity =>
            {
                entity.ToTable("DistributorOrderItems");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UnitPrice).HasPrecision(18, 2);
                entity.Property(e => e.Notes).HasMaxLength(500);
                entity.Property(e => e.CreatedBy).HasMaxLength(100);
                entity.Property(e => e.UpdatedBy).HasMaxLength(100);
                entity.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);

                entity.HasOne(e => e.Order)
                    .WithMany(o => o.OrderItems)
                    .HasForeignKey(e => e.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Product)
                    .WithMany(p => p.OrderItems)
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure DistributorInventoryTransaction
            modelBuilder.Entity<DistributorInventoryTransaction>(entity =>
            {
                entity.ToTable("DistributorInventoryTransactions");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TransactionType).IsRequired().HasMaxLength(20);
                entity.Property(e => e.UnitCost).HasPrecision(18, 2);
                entity.Property(e => e.Reference).HasMaxLength(100);
                entity.Property(e => e.Notes).HasMaxLength(500);
                entity.Property(e => e.CreatedBy).HasMaxLength(100);
                entity.Property(e => e.UpdatedBy).HasMaxLength(100);
                entity.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);

                entity.HasOne(e => e.Product)
                    .WithMany(p => p.InventoryTransactions)
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
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
                entity.Property(e => e.CompanyName).HasMaxLength(200);
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.Property(e => e.CreatedBy).HasMaxLength(100);
                entity.Property(e => e.UpdatedBy).HasMaxLength(100);
                entity.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);
            });

            // Seed data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // STATIC HASH for "Distributor123!" - This will work for login!
            // Generated using: BCrypt.Net.BCrypt.HashPassword("Distributor123!")
            const string distributorPasswordHash = "$2a$11$QJk7fvgHt5KnrHcE9dGhF.8MJYl4ZzPu1CeQq2AvBnIiYHXM4QNYi";

            // Seed distributor users
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Email = "distributor@cozycomfort.com",
                    FirstName = "David",
                    LastName = "Distributor",
                    PasswordHash = distributorPasswordHash, // Static hash that will work
                    Role = UserRole.Distributor,
                    CompanyName = "Central Distribution Ltd",
                    Phone = "1234567890",
                    CreatedAt = new DateTime(2024, 1, 1),
                    IsActive = true
                }
            );

            // Seed distributor products (matching manufacturer products)
            modelBuilder.Entity<DistributorProduct>().HasData(
                new DistributorProduct
                {
                    Id = 1,
                    ManufacturerProductId = 1,
                    ProductName = "Luxury Wool Blanket - Queen Size",
                    SKU = "LWB-Q-001",
                    PurchasePrice = 199.99m,
                    SellingPrice = 259.99m,
                    CurrentStock = 25,
                    ReservedStock = 0,
                    MinStockLevel = 10,
                    ReorderPoint = 15,
                    ReorderQuantity = 30,
                    CreatedAt = new DateTime(2024, 1, 1),
                    IsActive = true
                },
                new DistributorProduct
                {
                    Id = 2,
                    ManufacturerProductId = 2,
                    ProductName = "Cotton Comfort Blanket - King Size",
                    SKU = "CCB-K-001",
                    PurchasePrice = 149.99m,
                    SellingPrice = 199.99m,
                    CurrentStock = 40,
                    ReservedStock = 5,
                    MinStockLevel = 15,
                    ReorderPoint = 20,
                    ReorderQuantity = 40,
                    CreatedAt = new DateTime(2024, 1, 1),
                    IsActive = true
                }
            );
        }
    }
}