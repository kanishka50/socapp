using CozyComfort.Seller.API.Data;
using CozyComfort.Seller.API.Models.Entities;
using CozyComfort.Seller.API.Services.Interfaces;
using CozyComfort.Shared.DTOs;
using CozyComfort.Shared.DTOs.Distributor;
using CozyComfort.Shared.DTOs.Seller;
using CozyComfort.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace CozyComfort.Seller.API.Services.Implementations
{
    public class CustomerOrderService : ICustomerOrderService
    {
        private readonly SellerDbContext _context;
        private readonly ICartService _cartService;
        private readonly ILogger<CustomerOrderService> _logger;

        public CustomerOrderService(SellerDbContext context, ICartService cartService, ILogger<CustomerOrderService> logger)
        {
            _context = context;
            _cartService = cartService;
            _logger = logger;
        }

        public async Task<ApiResponse<CustomerOrderDto>> CreateOrderAsync(CreateCustomerOrderDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Get cart
                var cartResult = await _cartService.GetCartAsync(dto.SessionId);
                if (!cartResult.Success || !cartResult.Data.Items.Any())
                {
                    return ApiResponse<CustomerOrderDto>.FailureResult("Cart is empty");
                }

                var cart = cartResult.Data;

                // Create order
                var order = new CustomerOrder
                {
                    OrderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}",
                    CustomerName = dto.CustomerName,
                    CustomerEmail = dto.CustomerEmail,
                    CustomerPhone = dto.CustomerPhone,
                    Status = OrderStatus.Pending,
                    OrderDate = DateTime.UtcNow,
                    SubTotal = cart.SubTotal,
                    Tax = cart.Tax,
                    ShippingCost = cart.ShippingCost,
                    TotalAmount = cart.Total,
                    ShippingAddress = dto.ShippingAddress,
                    BillingAddress = dto.BillingAddress,
                    PaymentMethod = dto.PaymentMethod,
                    CreatedAt = DateTime.UtcNow
                };

                // Add order items
                foreach (var cartItem in cart.Items)
                {
                    order.OrderItems.Add(new CustomerOrderItem
                    {
                        ProductId = cartItem.ProductId,
                        Quantity = cartItem.Quantity,
                        UnitPrice = cartItem.UnitPrice
                    });
                }

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // Clear cart
                await _cartService.ClearCartAsync(dto.SessionId);

                await transaction.CommitAsync();

                var orderDto = new CustomerOrderDto
                {
                    Id = order.Id,
                    OrderNumber = order.OrderNumber,
                    CustomerName = order.CustomerName,
                    CustomerEmail = order.CustomerEmail,
                    Status = order.Status.ToString(),
                    OrderDate = order.OrderDate,
                    TotalAmount = order.TotalAmount,
                    ShippingAddress = order.ShippingAddress,
                    IsPaid = order.IsPaid
                };

                return ApiResponse<CustomerOrderDto>.SuccessResult(orderDto, "Order created successfully");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating order");
                return ApiResponse<CustomerOrderDto>.FailureResult("Error creating order");
            }
        }

        public async Task<ApiResponse<PagedResult<CustomerOrderDto>>> GetOrdersAsync(PagedRequest request)
        {
            try
            {
                var query = _context.Orders
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                    .AsQueryable();

                // Apply search filter
                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    query = query.Where(o =>
                        o.OrderNumber.Contains(request.SearchTerm) ||
                        o.CustomerName.Contains(request.SearchTerm) ||
                        o.CustomerEmail.Contains(request.SearchTerm));
                }

                // Get total count before pagination
                var totalCount = await query.CountAsync();

                // Apply sorting
                query = request.SortBy?.ToLower() switch
                {
                    "ordernumber" => request.IsDescending ?
                        query.OrderByDescending(o => o.OrderNumber) :
                        query.OrderBy(o => o.OrderNumber),
                    "customername" => request.IsDescending ?
                        query.OrderByDescending(o => o.CustomerName) :
                        query.OrderBy(o => o.CustomerName),
                    "totalamount" => request.IsDescending ?
                        query.OrderByDescending(o => o.TotalAmount) :
                        query.OrderBy(o => o.TotalAmount),
                    "status" => request.IsDescending ?
                        query.OrderByDescending(o => o.Status) :
                        query.OrderBy(o => o.Status),
                    _ => request.IsDescending ?
                        query.OrderByDescending(o => o.OrderDate) :
                        query.OrderBy(o => o.OrderDate)
                };

                // Apply pagination
                var orders = await query
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToListAsync();

                // Map to DTOs
                var orderDtos = orders.Select(o => new CustomerOrderDto
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    CustomerName = o.CustomerName,
                    CustomerEmail = o.CustomerEmail,
                    Status = o.Status.ToString(),
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    ShippingAddress = o.ShippingAddress,
                    IsPaid = o.IsPaid,
                    Items = o.OrderItems.Select(oi => new OrderItemDto
                    {
                        ProductId = oi.ProductId,
                        ProductName = oi.Product?.ProductName ?? "Unknown",
                        SKU = oi.Product?.SKU ?? "N/A",
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        TotalPrice = oi.TotalPrice
                    }).ToList()
                }).ToList();

                var pagedResult = new PagedResult<CustomerOrderDto>
                {
                    Items = orderDtos,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };

                return ApiResponse<PagedResult<CustomerOrderDto>>.SuccessResult(pagedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching orders");
                return ApiResponse<PagedResult<CustomerOrderDto>>.FailureResult("Error fetching orders",
                    new List<string> { ex.Message });
            }
        }

        public async Task<ApiResponse<CustomerOrderDto>> GetOrderByIdAsync(int id)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (order == null)
                {
                    return ApiResponse<CustomerOrderDto>.FailureResult("Order not found");
                }

                var orderDto = new CustomerOrderDto
                {
                    Id = order.Id,
                    OrderNumber = order.OrderNumber,
                    CustomerName = order.CustomerName,
                    CustomerEmail = order.CustomerEmail,
                    Status = order.Status.ToString(),
                    OrderDate = order.OrderDate,
                    TotalAmount = order.TotalAmount,
                    ShippingAddress = order.ShippingAddress,
                    IsPaid = order.IsPaid,
                    Items = order.OrderItems.Select(oi => new OrderItemDto
                    {
                        ProductId = oi.ProductId,
                        ProductName = oi.Product?.ProductName ?? "Unknown",
                        SKU = oi.Product?.SKU ?? "N/A",
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        TotalPrice = oi.TotalPrice
                    }).ToList()
                };

                return ApiResponse<CustomerOrderDto>.SuccessResult(orderDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching order {OrderId}", id);
                return ApiResponse<CustomerOrderDto>.FailureResult("Error fetching order",
                    new List<string> { ex.Message });
            }
        }

        public async Task<ApiResponse<bool>> UpdateOrderStatusAsync(int id, string status)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (order == null)
                {
                    return ApiResponse<bool>.FailureResult("Order not found");
                }

                // Store the old status for comparison
                var oldStatus = order.Status;

                // Parse the string status to enum
                if (!Enum.TryParse<OrderStatus>(status, true, out var newStatus))
                {
                    return ApiResponse<bool>.FailureResult($"Invalid status: {status}");
                }

                // Validate status transition
                if (!IsValidStatusTransition(oldStatus, newStatus))
                {
                    return ApiResponse<bool>.FailureResult($"Cannot change status from {oldStatus} to {newStatus}");
                }

                // Update order status
                order.Status = newStatus;
                order.UpdatedAt = DateTime.UtcNow;

                // If status is changed to "Accepted", decrease inventory
                if (newStatus == OrderStatus.Accepted && oldStatus != OrderStatus.Accepted)
                {
                    foreach (var orderItem in order.OrderItems)
                    {
                        var product = await _context.SellerProducts.FindAsync(orderItem.ProductId);
                        if (product == null)
                        {
                            await transaction.RollbackAsync();
                            return ApiResponse<bool>.FailureResult($"Product with ID {orderItem.ProductId} not found");
                        }

                        // Check if we have enough stock
                        if (product.CurrentStock < orderItem.Quantity)
                        {
                            await transaction.RollbackAsync();
                            return ApiResponse<bool>.FailureResult($"Insufficient stock for product {product.ProductName}. Available: {product.CurrentStock}, Required: {orderItem.Quantity}");
                        }

                        // Decrease the inventory
                        product.CurrentStock -= orderItem.Quantity;
                        product.DisplayStock = Math.Min(product.DisplayStock, product.CurrentStock);
                        product.UpdatedAt = DateTime.UtcNow;

                        _logger.LogInformation($"Decreased stock for product {product.ProductName} by {orderItem.Quantity}. New stock: {product.CurrentStock}");
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation($"Order {order.OrderNumber} status updated from {oldStatus} to {newStatus}");
                return ApiResponse<bool>.SuccessResult(true, $"Order status updated to {status}");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error updating order status");
                return ApiResponse<bool>.FailureResult("Error updating order status");
            }
        }

        private bool IsValidStatusTransition(OrderStatus currentStatus, OrderStatus newStatus)
        {
            // From Pending, can go to Accepted or Cancelled
            if (currentStatus == OrderStatus.Pending)
            {
                return newStatus == OrderStatus.Accepted || newStatus == OrderStatus.Cancelled;
            }

            // Cannot change from Accepted or Cancelled to anything else
            if (currentStatus == OrderStatus.Accepted || currentStatus == OrderStatus.Cancelled)
            {
                return false;
            }

            return false;
        }

        public async Task<ApiResponse<List<CustomerOrderDto>>> GetCustomerOrdersAsync(string customerEmail)
        {
            try
            {
                var orders = await _context.Orders
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                    .Where(o => o.CustomerEmail == customerEmail)
                    .OrderByDescending(o => o.OrderDate)
                    .ToListAsync();

                var orderDtos = orders.Select(o => new CustomerOrderDto
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    CustomerName = o.CustomerName,
                    CustomerEmail = o.CustomerEmail,
                    Status = o.Status.ToString(),
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    ShippingAddress = o.ShippingAddress,
                    IsPaid = o.IsPaid,
                    Items = o.OrderItems.Select(oi => new OrderItemDto
                    {
                        ProductId = oi.ProductId,
                        ProductName = oi.Product?.ProductName ?? "Unknown",
                        SKU = oi.Product?.SKU ?? "N/A",
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        TotalPrice = oi.TotalPrice
                    }).ToList()
                }).ToList();

                return ApiResponse<List<CustomerOrderDto>>.SuccessResult(orderDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching customer orders for {CustomerEmail}", customerEmail);
                return ApiResponse<List<CustomerOrderDto>>.FailureResult("Error fetching customer orders",
                    new List<string> { ex.Message });
            }
        }



        public async Task<ApiResponse<SellerDistributorOrderDto>> CreateDistributorOrderAsync(CreateSellerDistributorOrderDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Create order
                var order = new SellerDistributorOrder
                {
                    OrderNumber = $"SDO-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}",
                    DistributorId = 1, // You might want to make this configurable
                    Status = OrderStatus.Pending,
                    OrderDate = DateTime.UtcNow,
                    ShippingAddress = dto.ShippingAddress,
                    Notes = dto.Notes,
                    CreatedAt = DateTime.UtcNow
                };

                decimal totalAmount = 0;

                // Add order items and check stock
                foreach (var item in dto.Items)
                {
                    var product = await _context.SellerProducts.FindAsync(item.ProductId);
                    if (product == null)
                    {
                        await transaction.RollbackAsync();
                        return ApiResponse<SellerDistributorOrderDto>.FailureResult($"Product {item.ProductId} not found");
                    }

                    var orderItem = new SellerDistributorOrderItem
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = product.PurchasePrice // Use purchase price for distributor orders
                    };

                    totalAmount += orderItem.TotalPrice;
                    order.OrderItems.Add(orderItem);
                }

                order.TotalAmount = totalAmount;
                _context.DistributorOrders.Add(order);
                await _context.SaveChangesAsync();

                // Call Distributor API to create order
                var distributorItems = order.OrderItems.Select(i => new SellerOrderItem
                {
                    DistributorProductId = i.Product.DistributorProductId,
                    Quantity = i.Quantity,
                    RequestedPrice = i.UnitPrice
                }).ToList();

                var processOrderDto = new ProcessSellerOrderDto
                {
                    SellerId = 1, // Configure this
                    SellerOrderNumber = order.OrderNumber,
                    Items = distributorItems,
                    ShippingAddress = order.ShippingAddress
                };

                var distributorResult = await _distributorApiService.CreateDistributorOrderAsync(processOrderDto);

                if (distributorResult.Success && distributorResult.Data != null)
                {
                    order.DistributorOrderNumber = distributorResult.Data.OrderNumber;
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();

                // Map to DTO
                var orderDto = MapToSellerDistributorOrderDto(order);
                return ApiResponse<SellerDistributorOrderDto>.SuccessResult(orderDto);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating distributor order");
                return ApiResponse<SellerDistributorOrderDto>.FailureResult("Error creating order");
            }
        }

        public async Task<ApiResponse<List<SellerDistributorOrderDto>>> GetDistributorOrdersAsync()
        {
            try
            {
                var orders = await _context.DistributorOrders
                    .Include(o => o.OrderItems)
                    .ThenInclude(i => i.Product)
                    .OrderByDescending(o => o.OrderDate)
                    .ToListAsync();

                var orderDtos = orders.Select(MapToSellerDistributorOrderDto).ToList();
                return ApiResponse<List<SellerDistributorOrderDto>>.SuccessResult(orderDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving distributor orders");
                return ApiResponse<List<SellerDistributorOrderDto>>.FailureResult("Error retrieving orders");
            }
        }

        public async Task<ApiResponse<SellerDistributorOrderDto>> GetDistributorOrderByIdAsync(int id)
        {
            try
            {
                var order = await _context.DistributorOrders
                    .Include(o => o.OrderItems)
                    .ThenInclude(i => i.Product)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (order == null)
                {
                    return ApiResponse<SellerDistributorOrderDto>.FailureResult("Order not found");
                }

                var orderDto = MapToSellerDistributorOrderDto(order);
                return ApiResponse<SellerDistributorOrderDto>.SuccessResult(orderDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving distributor order");
                return ApiResponse<SellerDistributorOrderDto>.FailureResult("Error retrieving order");
            }
        }

        private SellerDistributorOrderDto MapToSellerDistributorOrderDto(SellerDistributorOrder order)
        {
            return new SellerDistributorOrderDto
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                DistributorOrderNumber = order.DistributorOrderNumber,
                Status = order.Status.ToString(),
                OrderDate = order.OrderDate,
                ExpectedDeliveryDate = order.ExpectedDeliveryDate,
                TotalAmount = order.TotalAmount,
                Items = order.OrderItems.Select(i => new SellerDistributorOrderItemDto
                {
                    ProductId = i.ProductId,
                    ProductName = i.Product?.ProductName ?? "",
                    SKU = i.Product?.SKU ?? "",
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    TotalPrice = i.TotalPrice
                }).ToList()
            };
        }
    }
}