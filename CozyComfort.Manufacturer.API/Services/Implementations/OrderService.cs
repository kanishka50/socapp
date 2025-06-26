using CozyComfort.Manufacturer.API.Data;
using CozyComfort.Manufacturer.API.Models.Entities;
using CozyComfort.Manufacturer.API.Services.Interfaces;
using CozyComfort.Shared.DTOs;
using CozyComfort.Shared.DTOs.Manufacturer;
using CozyComfort.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace CozyComfort.Manufacturer.API.Services.Implementations
{
    public class OrderService : IOrderService
    {
        private readonly ManufacturerDbContext _context;
        private readonly ILogger<OrderService> _logger;

        public OrderService(ManufacturerDbContext context, ILogger<OrderService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ApiResponse<PagedResult<ManufacturerOrderDto>>> GetOrdersAsync(int pageNumber, int pageSize)
        {
            try
            {
                var query = _context.Orders
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                    .OrderByDescending(o => o.CreatedAt);

                var totalCount = await query.CountAsync();

                var orders = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(o => new ManufacturerOrderDto
                    {
                        Id = o.Id,
                        OrderNumber = o.OrderNumber,
                        DistributorId = o.DistributorId,
                        DistributorName = o.DistributorName,
                        DistributorOrderNumber = o.DistributorOrderNumber,
                        Status = o.Status.ToString(),
                        OrderDate = o.OrderDate,
                        TotalAmount = o.TotalAmount,
                        Notes = o.Notes,
                        Items = o.OrderItems.Select(oi => new ManufacturerOrderItemDto
                        {
                            Id = oi.Id,
                            ProductId = oi.ProductId,
                            ProductName = oi.Product.Name,
                            SKU = oi.Product.SKU,
                            Quantity = oi.Quantity,
                            UnitPrice = oi.UnitPrice,
                            TotalPrice = oi.Quantity * oi.UnitPrice
                        }).ToList()
                    })
                    .ToListAsync();

                var pagedResult = new PagedResult<ManufacturerOrderDto>
                {
                    Items = orders,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                return ApiResponse<PagedResult<ManufacturerOrderDto>>.SuccessResult(pagedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving orders");
                return ApiResponse<PagedResult<ManufacturerOrderDto>>.FailureResult("Error retrieving orders");
            }
        }

        public async Task<ApiResponse<ManufacturerOrderDto>> GetOrderByIdAsync(int id)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (order == null)
                {
                    return ApiResponse<ManufacturerOrderDto>.FailureResult("Order not found");
                }

                var orderDto = new ManufacturerOrderDto
                {
                    Id = order.Id,
                    OrderNumber = order.OrderNumber,
                    DistributorId = order.DistributorId,
                    DistributorName = order.DistributorName,
                    DistributorOrderNumber = order.DistributorOrderNumber,
                    Status = order.Status.ToString(),
                    OrderDate = order.OrderDate,
                    TotalAmount = order.TotalAmount,
                    Notes = order.Notes,
                    Items = order.OrderItems.Select(oi => new ManufacturerOrderItemDto
                    {
                        Id = oi.Id,
                        ProductId = oi.ProductId,
                        ProductName = oi.Product.Name,
                        SKU = oi.Product.SKU,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        TotalPrice = oi.Quantity * oi.UnitPrice
                    }).ToList()
                };

                return ApiResponse<ManufacturerOrderDto>.SuccessResult(orderDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order {OrderId}", id);
                return ApiResponse<ManufacturerOrderDto>.FailureResult("Error retrieving order");
            }
        }

        public async Task<ApiResponse<ManufacturerOrderDto>> CreateOrderFromDistributorAsync(CreateManufacturerOrderFromDistributorDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Create order
                var order = new ManufacturerOrder
                {
                    OrderNumber = GenerateOrderNumber(),
                    DistributorId = dto.DistributorId,
                    DistributorName = dto.DistributorName,
                    DistributorOrderNumber = dto.DistributorOrderNumber,
                    Status = OrderStatus.Pending,
                    OrderDate = DateTime.UtcNow,
                    Notes = dto.Notes,
                    TotalAmount = 0,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System",
                    IsActive = true
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                decimal totalAmount = 0;

                // Add order items
                foreach (var item in dto.Items)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product == null)
                    {
                        throw new InvalidOperationException($"Product with ID {item.ProductId} not found");
                    }

                    var orderItem = new ManufacturerOrderItem
                    {
                        OrderId = order.Id,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = product.Price,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = "System",
                        IsActive = true
                    };

                    _context.OrderItems.Add(orderItem);
                    totalAmount += orderItem.Quantity * orderItem.UnitPrice;
                }

                // Update order total
                order.TotalAmount = totalAmount;
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                _logger.LogInformation("Order {OrderNumber} created from distributor {DistributorName}",
                    order.OrderNumber, order.DistributorName);

                return await GetOrderByIdAsync(order.Id);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating order from distributor");
                return ApiResponse<ManufacturerOrderDto>.FailureResult($"Error creating order: {ex.Message}");
            }
        }

        public async Task<ApiResponse<ManufacturerOrderDto>> UpdateOrderStatusAsync(int orderId, UpdateOrderStatusDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                {
                    return ApiResponse<ManufacturerOrderDto>.FailureResult("Order not found");
                }

                // Parse status
                if (!Enum.TryParse<OrderStatus>(dto.Status, true, out var newStatus))
                {
                    return ApiResponse<ManufacturerOrderDto>.FailureResult("Invalid status");
                }

                // Only allow status changes from Pending
                if (order.Status != OrderStatus.Pending)
                {
                    return ApiResponse<ManufacturerOrderDto>.FailureResult(
                        $"Cannot change status from {order.Status} to {newStatus}");
                }

                order.Status = newStatus;
                order.UpdatedAt = DateTime.UtcNow;
                order.UpdatedBy = "System";

                // If order is accepted, update inventory
                if (newStatus == OrderStatus.Accepted)
                {
                    foreach (var item in order.OrderItems)
                    {
                        var product = item.Product;

                        // Check stock availability
                        if (product.AvailableStock < item.Quantity)
                        {
                            await transaction.RollbackAsync();
                            return ApiResponse<ManufacturerOrderDto>.FailureResult(
                                $"Insufficient stock for {product.Name}. Available: {product.AvailableStock}, Required: {item.Quantity}");
                        }

                        // Decrease manufacturer stock
                        product.CurrentStock -= item.Quantity;

                        // Create inventory transaction
                        var inventoryTx = new InventoryTransaction
                        {
                            ProductId = product.Id,
                            TransactionType = "OUT",
                            Quantity = item.Quantity,
                            Reference = order.OrderNumber,
                            Notes = $"Order to distributor: {order.DistributorName}",
                            TransactionDate = DateTime.UtcNow,
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = "System",
                            IsActive = true
                        };

                        _context.InventoryTransactions.Add(inventoryTx);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Order {OrderNumber} status updated to {Status}",
                    order.OrderNumber, newStatus);

                // NEW CODE: Notify distributor when order is accepted
                if (newStatus == OrderStatus.Accepted && !string.IsNullOrEmpty(order.DistributorOrderNumber))
                {
                    try
                    {
                        using var httpClient = new HttpClient();
                        httpClient.DefaultRequestHeaders.Add("X-API-Key", "manufacturer-api-key-123");
                        httpClient.Timeout = TimeSpan.FromSeconds(30);

                        var response = await httpClient.PostAsync(
                            $"https://localhost:7002/api/orders/{order.DistributorOrderNumber}/manufacturer-accepted",
                            null); // No body needed

                        if (response.IsSuccessStatusCode)
                        {
                            _logger.LogInformation($"Successfully notified distributor about order {order.OrderNumber} acceptance");
                        }
                        else
                        {
                            _logger.LogWarning($"Failed to notify distributor. Status: {response.StatusCode}");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error notifying distributor about order {order.OrderNumber}");
                        // Don't throw - notification failure shouldn't affect the order status update
                    }
                }

                return await GetOrderByIdAsync(order.Id);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error updating order status");
                return ApiResponse<ManufacturerOrderDto>.FailureResult($"Error updating order: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<ManufacturerInventoryDto>>> GetInventoryAsync()
        {
            try
            {
                var inventory = await _context.Products
                    .Where(p => p.IsActive)
                    .Select(p => new ManufacturerInventoryDto
                    {
                        ProductId = p.Id,
                        ProductName = p.Name,
                        SKU = p.SKU,
                        CurrentStock = p.CurrentStock,
                        ReservedStock = p.ReservedStock,
                        AvailableStock = p.AvailableStock,
                        MinStockLevel = p.MinStockLevel,
                        ManufacturingCost = p.ManufacturingCost
                    })
                    .ToListAsync();

                return ApiResponse<List<ManufacturerInventoryDto>>.SuccessResult(inventory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving inventory");
                return ApiResponse<List<ManufacturerInventoryDto>>.FailureResult("Error retrieving inventory");
            }
        }

        private string GenerateOrderNumber()
        {
            var date = DateTime.UtcNow.ToString("yyyyMMdd");
            var guid = Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
            return $"MFG-{date}-{guid}";
        }
    }
}