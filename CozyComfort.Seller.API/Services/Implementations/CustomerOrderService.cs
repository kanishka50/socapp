using Microsoft.EntityFrameworkCore;
using CozyComfort.Seller.API.Data;
using CozyComfort.Seller.API.Models.DTOs;
using CozyComfort.Seller.API.Models.Entities;
using CozyComfort.Seller.API.Services.Interfaces;
using CozyComfort.Shared.DTOs;
using CozyComfort.Shared.Enums;

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
                    ShippingAddress = order.ShippingAddress
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
            // Implementation
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<CustomerOrderDto>> GetOrderByIdAsync(int id)
        {
            // Implementation
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<bool>> UpdateOrderStatusAsync(int id, string status)
        {
            // Implementation
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<List<CustomerOrderDto>>> GetCustomerOrdersAsync(string customerEmail)
        {
            // Implementation
            throw new NotImplementedException();
        }
    }
}