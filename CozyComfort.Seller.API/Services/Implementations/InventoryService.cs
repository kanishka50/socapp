using CozyComfort.Seller.API.Data;
using CozyComfort.Seller.API.Models.Entities;
using CozyComfort.Seller.API.Services.Interfaces;
using CozyComfort.Shared.DTOs;
using CozyComfort.Shared.DTOs.Seller;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CozyComfort.Seller.API.Services.Implementations
{
    public class InventoryService : IInventoryService
    {
        private readonly SellerDbContext _context;
        private readonly ILogger<InventoryService> _logger;

        public InventoryService(SellerDbContext context, ILogger<InventoryService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> UpdateStockAfterOrderAsync(int productId, int quantity)
        {
            try
            {
                var product = await _context.SellerProducts.FindAsync(productId);
                if (product == null)
                {
                    return false;
                }

                // Reduce stock for customer orders
                if (product.CurrentStock < quantity)
                {
                    return false;
                }

                product.CurrentStock -= quantity;
                product.DisplayStock = product.CurrentStock;

                // Create inventory transaction
                var transaction = new SellerInventoryTransaction
                {
                    ProductId = productId,
                    TransactionType = "OUT",
                    Quantity = quantity,
                    Reference = "CUSTOMER_ORDER",
                    Notes = "Stock reduced for customer order",
                    TransactionDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                _context.InventoryTransactions.Add(transaction);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating stock for product {ProductId}", productId);
                return false;
            }
        }

        public async Task<bool> CheckStockAvailabilityAsync(int productId, int quantity)
        {
            var product = await _context.SellerProducts.FindAsync(productId);
            return product != null && product.CurrentStock >= quantity;
        }

        public async Task<ApiResponse<bool>> UpdateStockBulkAsync(UpdateSellerStockBulkDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                foreach (var item in dto.Items)
                {
                    var product = await _context.SellerProducts.FindAsync(item.ProductId);
                    if (product == null)
                    {
                        await transaction.RollbackAsync();
                        return ApiResponse<bool>.FailureResult($"Product {item.ProductId} not found");
                    }

                    // Update stock based on transaction type
                    if (item.TransactionType == "IN")
                    {
                        product.CurrentStock += item.Quantity;
                        product.DisplayStock = product.CurrentStock; // Update display stock
                    }
                    else if (item.TransactionType == "OUT")
                    {
                        if (product.CurrentStock < item.Quantity)
                        {
                            await transaction.RollbackAsync();
                            return ApiResponse<bool>.FailureResult($"Insufficient stock for product {product.ProductName}");
                        }
                        product.CurrentStock -= item.Quantity;
                        product.DisplayStock = product.CurrentStock;
                    }

                    // Create inventory transaction
                    var inventoryTx = new SellerInventoryTransaction
                    {
                        ProductId = product.Id,
                        TransactionType = item.TransactionType,
                        Quantity = item.Quantity,
                        Reference = dto.OrderNumber,
                        Notes = $"Stock update from distributor order {dto.OrderNumber}",
                        TransactionDate = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.InventoryTransactions.Add(inventoryTx);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Stock updated successfully for order {OrderNumber}", dto.OrderNumber);
                return ApiResponse<bool>.SuccessResult(true, "Stock updated successfully");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error updating stock bulk for order {OrderNumber}", dto.OrderNumber);
                return ApiResponse<bool>.FailureResult("Error updating stock");
            }
        }
    }
}