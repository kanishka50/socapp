using Microsoft.EntityFrameworkCore;
using CozyComfort.Seller.API.Data;
using CozyComfort.Seller.API.Models.Entities;
using CozyComfort.Seller.API.Services.Interfaces;
using CozyComfort.Shared.DTOs;
using CozyComfort.Shared.DTOs.Seller;
using CozyComfort.Shared.DTOs.Distributor;
using CozyComfort.Shared.Enums;

namespace CozyComfort.Seller.API.Services.Implementations
{
    public class SellerOrderService : ISellerOrderService
    {
        private readonly SellerDbContext _context;
        private readonly IDistributorApiService _distributorApiService;
        private readonly ILogger<SellerOrderService> _logger;

        public SellerOrderService(
            SellerDbContext context,
            IDistributorApiService distributorApiService,
            ILogger<SellerOrderService> logger)
        {
            _context = context;
            _distributorApiService = distributorApiService;
            _logger = logger;
        }

        public async Task<ApiResponse<SellerDistributorOrderDto>> CreateDistributorOrderAsync(CreateSellerDistributorOrderDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Create seller's order record
                var order = new SellerDistributorOrder
                {
                    OrderNumber = $"SEL-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}",
                    DistributorId = 1, // Default distributor
                    Status = OrderStatus.Pending,
                    OrderDate = DateTime.UtcNow,
                    ShippingAddress = dto.ShippingAddress,
                    Notes = dto.Notes,
                    TotalAmount = 0,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                _context.DistributorOrders.Add(order);
                await _context.SaveChangesAsync();

                decimal totalAmount = 0;
                var distributorOrderItems = new List<DistributorOrderItemDto>();

                foreach (var item in dto.Items)
                {
                    // Check if product exists in seller inventory
                    var existingProduct = await _context.SellerProducts
                        .FirstOrDefaultAsync(p => p.DistributorProductId == item.DistributorProductId);

                    if (existingProduct == null)
                    {
                        // Get distributor product details
                        var distributorProduct = await _distributorApiService.GetDistributorProductByIdAsync(item.DistributorProductId);

                        if (!distributorProduct.Success || distributorProduct.Data == null)
                        {
                            await transaction.RollbackAsync();
                            return ApiResponse<SellerDistributorOrderDto>.FailureResult($"Distributor product {item.DistributorProductId} not found");
                        }

                        // Create new seller product with 5% markup
                        existingProduct = new SellerProduct
                        {
                            DistributorProductId = item.DistributorProductId,
                            ProductName = distributorProduct.Data.ProductName,
                            Description = distributorProduct.Data.Description,
                            SKU = $"SEL-{distributorProduct.Data.SKU}",
                            Category = distributorProduct.Data.Category,
                            PurchasePrice = distributorProduct.Data.SellingPrice,
                            SellingPrice = distributorProduct.Data.SellingPrice * 1.05m, // 5% markup
                            CurrentStock = 0, // No stock yet
                            DisplayStock = 0,
                            IsAvailable = false, // Not available until stock arrives
                            ImageUrl = distributorProduct.Data.ImageUrl,
                            CreatedAt = DateTime.UtcNow,
                            IsActive = true
                        };

                        _context.SellerProducts.Add(existingProduct);
                        await _context.SaveChangesAsync();

                        _logger.LogInformation($"Created new seller product: {existingProduct.ProductName}");
                    }

                    // Create order item
                    var orderItem = new SellerDistributorOrderItem
                    {
                        OrderId = order.Id,
                        ProductId = existingProduct.Id,
                        Quantity = item.Quantity,
                        UnitPrice = existingProduct.PurchasePrice,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    };

                    _context.DistributorOrderItems.Add(orderItem);
                    totalAmount += orderItem.Quantity * orderItem.UnitPrice;

                    // Prepare distributor order item
                    distributorOrderItems.Add(new DistributorOrderItemDto
                    {
                        DistributorProductId = item.DistributorProductId,
                        Quantity = item.Quantity,
                        RequestedPrice = existingProduct.PurchasePrice
                    });
                }

                // Update order total
                order.TotalAmount = totalAmount;
                await _context.SaveChangesAsync();

                // Create order with distributor
                var distributorResponse = await _distributorApiService.CreateDistributorOrderAsync(distributorOrderItems);

                if (distributorResponse.Success && distributorResponse.Data != null)
                {
                    // Update with distributor order number
                    order.DistributorOrderNumber = distributorResponse.Data.OrderNumber;
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    // Map to DTO
                    var orderDto = MapToDto(order);
                    return ApiResponse<SellerDistributorOrderDto>.SuccessResult(orderDto, "Order created successfully");
                }
                else
                {
                    await transaction.RollbackAsync();
                    return ApiResponse<SellerDistributorOrderDto>.FailureResult("Failed to create order with distributor");
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating distributor order");
                return ApiResponse<SellerDistributorOrderDto>.FailureResult($"Error creating order: {ex.Message}");
            }
        }

        public async Task<ApiResponse<PagedResult<SellerDistributorOrderDto>>> GetDistributorOrdersAsync(PagedRequest request)
        {
            try
            {
                var query = _context.DistributorOrders
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                    .AsQueryable();

                // Apply search filter
                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    query = query.Where(o =>
                        o.OrderNumber.Contains(request.SearchTerm) ||
                        o.DistributorOrderNumber.Contains(request.SearchTerm));
                }

                // Apply sorting
                query = request.SortBy?.ToLower() switch
                {
                    "date" => request.IsDescending ? query.OrderByDescending(o => o.OrderDate) : query.OrderBy(o => o.OrderDate),
                    "amount" => request.IsDescending ? query.OrderByDescending(o => o.TotalAmount) : query.OrderBy(o => o.TotalAmount),
                    _ => query.OrderByDescending(o => o.OrderDate)
                };

                var totalCount = await query.CountAsync();

                var orders = await query
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .Select(o => new SellerDistributorOrderDto
                    {
                        Id = o.Id,
                        OrderNumber = o.OrderNumber,
                        DistributorOrderNumber = o.DistributorOrderNumber,
                        Status = o.Status.ToString(),
                        OrderDate = o.OrderDate,
                        TotalAmount = o.TotalAmount,
                        ItemCount = o.OrderItems.Count
                    })
                    .ToListAsync();

                var result = new PagedResult<SellerDistributorOrderDto>
                {
                    Items = orders,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };

                return ApiResponse<PagedResult<SellerDistributorOrderDto>>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting distributor orders");
                return ApiResponse<PagedResult<SellerDistributorOrderDto>>.FailureResult("Error retrieving orders");
            }
        }

        public async Task<ApiResponse<SellerDistributorOrderDto>> GetDistributorOrderByIdAsync(int orderId)
        {
            try
            {
                var order = await _context.DistributorOrders
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                {
                    return ApiResponse<SellerDistributorOrderDto>.FailureResult("Order not found");
                }

                var orderDto = MapToDto(order);
                return ApiResponse<SellerDistributorOrderDto>.SuccessResult(orderDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting distributor order");
                return ApiResponse<SellerDistributorOrderDto>.FailureResult("Error retrieving order");
            }
        }

        public async Task<ApiResponse<bool>> UpdateOrderFromDistributorAcceptanceAsync(string distributorOrderNumber)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Find the order
                var order = await _context.DistributorOrders
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                    .FirstOrDefaultAsync(o => o.DistributorOrderNumber == distributorOrderNumber);

                if (order == null)
                {
                    return ApiResponse<bool>.FailureResult("Order not found");
                }

                // Update order status
                order.Status = OrderStatus.Accepted;
                order.UpdatedAt = DateTime.UtcNow;

                // Update stock for each item
                foreach (var item in order.OrderItems)
                {
                    var product = item.Product;

                    // Increase stock
                    product.CurrentStock += item.Quantity;
                    product.DisplayStock = product.CurrentStock;
                    product.IsAvailable = product.CurrentStock > 0;
                    product.UpdatedAt = DateTime.UtcNow;

                    // Create inventory transaction
                    var inventoryTransaction = new SellerInventoryTransaction
                    {
                        ProductId = product.Id,
                        TransactionType = "IN",
                        Quantity = item.Quantity,
                        Reference = order.OrderNumber,
                        Notes = $"Stock received from distributor order {distributorOrderNumber}",
                        TransactionDate = DateTime.UtcNow,
                        UnitCost = item.UnitPrice,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    };

                    _context.InventoryTransactions.Add(inventoryTransaction);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation($"Successfully updated stock for order {distributorOrderNumber}");
                return ApiResponse<bool>.SuccessResult(true, "Stock updated successfully");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error updating stock from distributor acceptance");
                return ApiResponse<bool>.FailureResult($"Error updating stock: {ex.Message}");
            }
        }

        public async Task<ApiResponse<CombinedOrdersDto>> GetCombinedOrdersAsync(PagedRequest request)
        {
            try
            {
                // Get customer orders
                var customerOrdersQuery = _context.Orders
                    .Include(o => o.OrderItems)
                    .AsQueryable();

                // Get distributor orders
                var distributorOrdersQuery = _context.DistributorOrders
                    .Include(o => o.OrderItems)
                    .AsQueryable();

                // Apply filters if needed
                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    customerOrdersQuery = customerOrdersQuery.Where(o =>
                        o.OrderNumber.Contains(request.SearchTerm) ||
                        o.CustomerName.Contains(request.SearchTerm));

                    distributorOrdersQuery = distributorOrdersQuery.Where(o =>
                        o.OrderNumber.Contains(request.SearchTerm) ||
                        o.DistributorOrderNumber.Contains(request.SearchTerm));
                }

                // Get recent orders
                var customerOrders = await customerOrdersQuery
                    .OrderByDescending(o => o.OrderDate)
                    .Take(10)
                    .Select(o => new CustomerOrderDto
                    {
                        Id = o.Id,
                        OrderNumber = o.OrderNumber,
                        CustomerName = o.CustomerName,
                        CustomerEmail = o.CustomerEmail,
                        Status = o.Status.ToString(),
                        OrderDate = o.OrderDate,
                        TotalAmount = o.TotalAmount,
                        ItemCount = o.OrderItems.Count
                    })
                    .ToListAsync();

                var distributorOrders = await distributorOrdersQuery
                    .OrderByDescending(o => o.OrderDate)
                    .Take(10)
                    .Select(o => new SellerDistributorOrderDto
                    {
                        Id = o.Id,
                        OrderNumber = o.OrderNumber,
                        DistributorOrderNumber = o.DistributorOrderNumber,
                        Status = o.Status.ToString(),
                        OrderDate = o.OrderDate,
                        TotalAmount = o.TotalAmount,
                        ItemCount = o.OrderItems.Count
                    })
                    .ToListAsync();

                var result = new CombinedOrdersDto
                {
                    CustomerOrders = customerOrders,
                    DistributorOrders = distributorOrders,
                    TotalCustomerOrders = await customerOrdersQuery.CountAsync(),
                    TotalDistributorOrders = await distributorOrdersQuery.CountAsync()
                };

                return ApiResponse<CombinedOrdersDto>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting combined orders");
                return ApiResponse<CombinedOrdersDto>.FailureResult("Error retrieving orders");
            }
        }

        private SellerDistributorOrderDto MapToDto(SellerDistributorOrder order)
        {
            return new SellerDistributorOrderDto
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                DistributorOrderNumber = order.DistributorOrderNumber,
                Status = order.Status.ToString(),
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                ShippingAddress = order.ShippingAddress,
                Notes = order.Notes,
                ItemCount = order.OrderItems?.Count ?? 0,
                Items = order.OrderItems?.Select(oi => new SellerDistributorOrderItemDto
                {
                    ProductId = oi.ProductId,
                    ProductName = oi.Product?.ProductName ?? "",
                    SKU = oi.Product?.SKU ?? "",
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    TotalPrice = oi.TotalPrice
                }).ToList() ?? new List<SellerDistributorOrderItemDto>()
            };
        }
    }
}