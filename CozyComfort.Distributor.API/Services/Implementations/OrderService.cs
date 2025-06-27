using CozyComfort.Distributor.API.Data;
using CozyComfort.Distributor.API.Models.Entities;
using CozyComfort.Distributor.API.Services.Interfaces;
using CozyComfort.Shared.DTOs;
using CozyComfort.Shared.DTOs.Distributor;
using CozyComfort.Shared.DTOs.Manufacturer;
using CozyComfort.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.Net.Http.Json;
using SellerDTOs = CozyComfort.Shared.DTOs.Seller;

namespace CozyComfort.Distributor.API.Services.Implementations
{
    public class OrderService : IOrderService
    {
        private readonly DistributorDbContext _context;
        private readonly ILogger<OrderService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public OrderService(DistributorDbContext context, ILogger<OrderService> logger , IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<ApiResponse<OrderDto>> ProcessSellerOrderAsync(ProcessSellerOrderDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Create distributor order with PENDING status
                var order = new DistributorOrder
                {
                    OrderNumber = $"DIST-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}",
                    OrderType = OrderType.FromSeller,
                    CustomerId = dto.SellerId,
                    CustomerOrderNumber = dto.SellerOrderNumber,
                    Status = OrderStatus.Pending, // Always start as Pending
                    OrderDate = DateTime.UtcNow,
                    ShippingAddress = dto.ShippingAddress,
                    CreatedAt = DateTime.UtcNow,
                    TotalAmount = 0
                };

                decimal totalAmount = 0;

                // Add order items without affecting inventory yet
                foreach (var item in dto.Items)
                {
                    var product = await _context.Products.FindAsync(item.DistributorProductId);
                    if (product == null)
                    {
                        await transaction.RollbackAsync();
                        return ApiResponse<OrderDto>.FailureResult($"Product {item.DistributorProductId} not found");
                    }

                    var orderItem = new DistributorOrderItem
                    {
                        ProductId = product.Id,
                        Quantity = item.Quantity,
                        UnitPrice = product.SellingPrice,
                        Product = product
                    };

                    order.OrderItems.Add(orderItem);
                    totalAmount += orderItem.Quantity * orderItem.UnitPrice;
                }

                order.TotalAmount = totalAmount;
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                var orderDto = new OrderDto
                {
                    Id = order.Id,
                    OrderNumber = order.OrderNumber,
                    CustomerName = $"Seller #{dto.SellerId}",
                    Status = order.Status.ToString(),
                    OrderDate = order.OrderDate,
                    TotalAmount = order.TotalAmount,
                    Items = order.OrderItems.Select(oi => new OrderItemDto
                    {
                        ProductId = oi.ProductId,
                        ProductName = oi.Product.ProductName,
                        SKU = oi.Product.SKU,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        TotalPrice = oi.Quantity * oi.UnitPrice
                    }).ToList()
                };

                _logger.LogInformation($"Seller order {order.OrderNumber} created successfully with PENDING status");
                return ApiResponse<OrderDto>.SuccessResult(orderDto, "Order created successfully");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error processing seller order");
                return ApiResponse<OrderDto>.FailureResult($"Error processing order: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> UpdateOrderStatusAsync(int id, string status)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (order == null)
                {
                    return ApiResponse<bool>.FailureResult("Order not found");
                }

                // Parse the new status
                if (!Enum.TryParse<OrderStatus>(status, true, out var newStatus))
                {
                    return ApiResponse<bool>.FailureResult("Invalid status");
                }

                var oldStatus = order.Status;
                order.Status = newStatus;

                // Handle FROM_SELLER orders - DECREASE inventory when accepted
                if (oldStatus == OrderStatus.Pending && newStatus == OrderStatus.Accepted && order.OrderType == OrderType.FromSeller)
                {
                    // Check if we have enough inventory
                    foreach (var item in order.OrderItems)
                    {
                        if (item.Product.CurrentStock < item.Quantity)
                        {
                            await transaction.RollbackAsync();
                            return ApiResponse<bool>.FailureResult($"Insufficient stock for {item.Product.ProductName}. Available: {item.Product.CurrentStock}, Required: {item.Quantity}");
                        }
                    }

                    // Decrease distributor inventory
                    foreach (var item in order.OrderItems)
                    {
                        item.Product.CurrentStock -= item.Quantity;

                        // Log inventory transaction
                        var inventoryTx = new DistributorInventoryTransaction
                        {
                            ProductId = item.ProductId,
                            TransactionType = "OUT",
                            Quantity = item.Quantity,
                            Reference = order.OrderNumber,
                            Notes = $"Order accepted - Stock decreased for seller",
                            TransactionDate = DateTime.UtcNow,
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.InventoryTransactions.Add(inventoryTx);
                    }

                    // **FIXED: Notify seller when distributor order is accepted**
                    try
                    {
                        // Prepare seller notification using alias
                        var sellerNotification = new SellerDTOs.DistributorOrderAcceptedDto
                        {
                            DistributorOrderNumber = order.OrderNumber,
                            Items = order.OrderItems.Select(item => new SellerDTOs.DistributorOrderAcceptedItemDto
                            {
                                DistributorProductId = item.ProductId,
                                Quantity = item.Quantity
                            }).ToList()
                        };

                        // Call Seller API with correct URL
                        using var httpClient = new HttpClient();
                        httpClient.Timeout = TimeSpan.FromSeconds(30);

                        var response = await httpClient.PostAsJsonAsync(
                            "https://localhost:7003/api/distributor-orders/accept-notification",
                            sellerNotification);

                        if (response.IsSuccessStatusCode)
                        {
                            _logger.LogInformation($"Successfully notified seller about order {order.OrderNumber} acceptance");
                        }
                        else
                        {
                            var errorContent = await response.Content.ReadAsStringAsync();
                            _logger.LogWarning($"Failed to notify seller about order {order.OrderNumber} acceptance. Status: {response.StatusCode}, Content: {errorContent}");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error notifying seller about order {order.OrderNumber} acceptance");
                        // Don't fail the transaction if seller notification fails
                    }
                }

                // Handle TO_MANUFACTURER orders - INCREASE inventory when accepted
                else if (newStatus == OrderStatus.Accepted && order.OrderType == OrderType.ToManufacturer)
                {
                    _logger.LogInformation($"Processing manufacturer order acceptance for {order.OrderNumber}");

                    // Increase distributor inventory for each item
                    foreach (var item in order.OrderItems)
                    {
                        var previousStock = item.Product.CurrentStock;
                        item.Product.CurrentStock += item.Quantity;
                        item.Product.UpdatedAt = DateTime.UtcNow;

                        _logger.LogInformation($"Updated stock for {item.Product.SKU}: {previousStock} -> {item.Product.CurrentStock} (+{item.Quantity})");

                        // Create inventory transaction
                        var inventoryTx = new DistributorInventoryTransaction
                        {
                            ProductId = item.ProductId,
                            TransactionType = "IN",
                            Quantity = item.Quantity,
                            UnitCost = item.UnitPrice,
                            Reference = order.OrderNumber,
                            Notes = $"Received from manufacturer - Order accepted",
                            TransactionDate = DateTime.UtcNow,
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.InventoryTransactions.Add(inventoryTx);
                    }
                }

                order.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation($"Order {order.OrderNumber} status updated from {oldStatus} to {newStatus}");
                return ApiResponse<bool>.SuccessResult(true, $"Order status updated to {newStatus}");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error updating order status");
                return ApiResponse<bool>.FailureResult($"Error updating order status: {ex.Message}");
            }
        }

        public async Task<ApiResponse<PagedResult<OrderDto>>> GetOrdersAsync(int pageNumber, int pageSize)
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
                    .Select(o => new OrderDto
                    {
                        Id = o.Id,
                        OrderNumber = o.OrderNumber,
                        CustomerName = o.OrderType == OrderType.FromSeller ? $"Seller #{o.SellerId ?? o.CustomerId}" : $"Manufacturer Order",
                        Status = o.Status.ToString(),
                        OrderDate = o.OrderDate,
                        TotalAmount = o.TotalAmount,
                        Items = o.OrderItems.Select(oi => new OrderItemDto
                        {
                            ProductId = oi.ProductId,
                            ProductName = oi.Product.ProductName,
                            SKU = oi.Product.SKU,
                            Quantity = oi.Quantity,
                            UnitPrice = oi.UnitPrice,
                            TotalPrice = oi.Quantity * oi.UnitPrice
                        }).ToList()
                    })
                    .ToListAsync();

                var pagedResult = new PagedResult<OrderDto>
                {
                    Items = orders,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                return ApiResponse<PagedResult<OrderDto>>.SuccessResult(pagedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving orders");
                return ApiResponse<PagedResult<OrderDto>>.FailureResult("Error retrieving orders");
            }
        }

        public async Task<ApiResponse<OrderDto>> GetOrderByIdAsync(int id)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (order == null)
                {
                    return ApiResponse<OrderDto>.FailureResult("Order not found");
                }

                var orderDto = new OrderDto
                {
                    Id = order.Id,
                    OrderNumber = order.OrderNumber,
                    CustomerName = order.OrderType == OrderType.FromSeller ? $"Seller #{order.SellerId}" : $"Manufacturer #{order.ManufacturerId}",
                    Status = order.Status.ToString(),
                    OrderDate = order.OrderDate,
                    TotalAmount = order.TotalAmount,
                    Items = order.OrderItems.Select(oi => new OrderItemDto
                    {
                        ProductId = oi.ProductId,
                        ProductName = oi.Product.ProductName,
                        SKU = oi.Product.SKU,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        TotalPrice = oi.Quantity * oi.UnitPrice
                    }).ToList()
                };

                return ApiResponse<OrderDto>.SuccessResult(orderDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order");
                return ApiResponse<OrderDto>.FailureResult("Error retrieving order");
            }
        }

        public async Task<ApiResponse<OrderDto>> CreateManufacturerOrderAsync(CreateManufacturerOrderDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Create distributor's internal order record
                var order = new DistributorOrder
                {
                    OrderNumber = $"DIST-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}",
                    OrderType = OrderType.ToManufacturer,
                    ManufacturerId = 1,
                    CustomerName = "Cozy Comfort Manufacturing",
                    Status = OrderStatus.Pending,
                    OrderDate = DateTime.UtcNow,
                    ShippingAddress = dto.ShippingAddress,
                    Notes = dto.Notes,
                    TotalAmount = 0,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System",
                    IsActive = true
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                decimal totalAmount = 0;
                var manufacturerApiService = _httpClientFactory.CreateClient("ManufacturerAPI");
                var authToken = await GetManufacturerAuthToken();
                if (!string.IsNullOrEmpty(authToken))
                {
                    manufacturerApiService.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);
                }

                // Add order items - create distributor products if they don't exist
                foreach (var item in dto.Items)
                {
                    // Get manufacturer product details
                    var manufacturerProductResponse = await manufacturerApiService.GetFromJsonAsync<ApiResponse<ProductDto>>($"api/products/{item.ManufacturerProductId}");

                    if (manufacturerProductResponse?.Success != true || manufacturerProductResponse.Data == null)
                    {
                        await transaction.RollbackAsync();
                        return ApiResponse<OrderDto>.FailureResult($"Manufacturer product {item.ManufacturerProductId} not found");
                    }

                    var manufacturerProduct = manufacturerProductResponse.Data;

                    // Check if distributor product exists, if not create it
                    var distributorProduct = await _context.Products
                        .FirstOrDefaultAsync(p => p.ManufacturerProductId == item.ManufacturerProductId);

                    if (distributorProduct == null)
                    {
                        // Create new distributor product with 0 stock
                        distributorProduct = new DistributorProduct
                        {
                            ManufacturerProductId = manufacturerProduct.Id,
                            ProductName = manufacturerProduct.Name,
                            SKU = manufacturerProduct.SKU,
                            PurchasePrice = manufacturerProduct.Price, // What we pay manufacturer
                            SellingPrice = manufacturerProduct.Price * 1.05m, // 5% markup
                            CurrentStock = 0,
                            ReservedStock = 0,
                            MinStockLevel = 5,
                            ReorderPoint = 10,
                            ReorderQuantity = 20,
                            CreatedAt = DateTime.UtcNow,
                            IsActive = true
                        };

                        _context.Products.Add(distributorProduct);
                        await _context.SaveChangesAsync();

                        _logger.LogInformation($"Auto-created distributor product {distributorProduct.SKU} for manufacturer order");
                    }

                    // Create order item
                    var orderItem = new DistributorOrderItem
                    {
                        OrderId = order.Id,
                        ProductId = distributorProduct.Id,
                        Quantity = item.Quantity,
                        UnitPrice = distributorProduct.PurchasePrice,
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

                // Call manufacturer API to create the order there
                var manufacturerOrderRequest = new
                {
                    DistributorId = 1,
                    DistributorName = "Central Distribution Ltd",
                    DistributorOrderNumber = order.OrderNumber,
                    Notes = dto.Notes,
                    Items = dto.Items.Select(i => new
                    {
                        ProductId = i.ManufacturerProductId,
                        Quantity = i.Quantity
                    }).ToList()
                };

                var response = await manufacturerApiService.PostAsJsonAsync(
                    "api/orders/from-distributor",
                    manufacturerOrderRequest);

                if (!response.IsSuccessStatusCode)
                {
                    await transaction.RollbackAsync();
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to create order in manufacturer API: {Error}", error);
                    return ApiResponse<OrderDto>.FailureResult("Failed to create order with manufacturer");
                }

                await transaction.CommitAsync();

                // Return the created order with proper item details
                var orderDto = new OrderDto
                {
                    Id = order.Id,
                    OrderNumber = order.OrderNumber,
                    CustomerName = order.CustomerName,
                    Status = order.Status.ToString(),
                    OrderDate = order.OrderDate,
                    TotalAmount = order.TotalAmount,
                    Items = await _context.OrderItems
                        .Where(oi => oi.OrderId == order.Id)
                        .Include(oi => oi.Product)
                        .Select(oi => new OrderItemDto
                        {
                            ProductId = oi.ProductId,
                            ProductName = oi.Product.ProductName,
                            SKU = oi.Product.SKU,
                            Quantity = oi.Quantity,
                            UnitPrice = oi.UnitPrice,
                            TotalPrice = oi.Quantity * oi.UnitPrice
                        })
                        .ToListAsync()
                };

                _logger.LogInformation("Manufacturer order {OrderNumber} created successfully", order.OrderNumber);
                return ApiResponse<OrderDto>.SuccessResult(orderDto, "Order created successfully");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating manufacturer order");
                return ApiResponse<OrderDto>.FailureResult($"Error creating order: {ex.Message}");
            }
        }

        private async Task<string> GetManufacturerAuthToken()
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient("ManufacturerAPI");
                var loginRequest = new
                {
                    Email = "distributor-api@cozycomfort.com",
                    Password = "DistributorAPI123!"
                };

                var response = await httpClient.PostAsJsonAsync("api/auth/login", loginRequest);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<TokenDto>>();
                    return result?.Data?.Token ?? string.Empty;
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting manufacturer auth token");
                return string.Empty;
            }
        }



        public async Task<ApiResponse<bool>> UpdateOrderStatusAsync(string orderNumber, string status)
        {
            try
            {
                // Find order by order number
                var order = await _context.Orders
                    .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);

                if (order == null)
                {
                    return ApiResponse<bool>.FailureResult($"Order {orderNumber} not found");
                }

                // Call the existing method with the order ID
                return await UpdateOrderStatusAsync(order.Id, status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order status by order number");
                return ApiResponse<bool>.FailureResult($"Error: {ex.Message}");
            }
        }
    }
}