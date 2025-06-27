using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CozyComfort.Seller.API.Data;
using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
using CozyComfort.Shared.Enums;

namespace CozyComfort.Seller.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly SellerDbContext _context;
        private readonly ILogger<TestController> _logger;

        public TestController(SellerDbContext context, ILogger<TestController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Test endpoint to check database connectivity and users
        /// </summary>
        [HttpGet("check-users")]
        [AllowAnonymous]
        public async Task<IActionResult> CheckUsers()
        {
            try
            {
                var users = await _context.Users
                    .Select(u => new
                    {
                        u.Id,
                        u.Email,
                        u.FirstName,
                        u.LastName,
                        u.Role,
                        u.IsActive,
                        u.CompanyName,
                        u.CreatedAt,
                        u.UpdatedAt
                    })
                    .ToListAsync();

                return Ok(new
                {
                    message = "Database connection successful",
                    userCount = users.Count,
                    users = users
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking users");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get all seller products with stock info
        /// </summary>
        [HttpGet("check-products")]
        [AllowAnonymous]
        public async Task<IActionResult> CheckProducts()
        {
            try
            {
                var products = await _context.SellerProducts
                    .Select(p => new
                    {
                        p.Id,
                        p.ProductName,
                        p.SKU,
                        p.Category,
                        p.CurrentStock,
                        p.DisplayStock,
                        p.PurchasePrice,
                        p.SellingPrice,
                        Margin = p.SellingPrice - p.PurchasePrice,
                        p.IsAvailable,
                        p.DistributorProductId
                    })
                    .ToListAsync();

                return Ok(new
                {
                    message = "Products retrieved successfully",
                    productCount = products.Count,
                    products = products
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking products");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get customer orders summary
        /// </summary>
        [HttpGet("check-orders")]
        [AllowAnonymous]
        public async Task<IActionResult> CheckOrders()
        {
            try
            {
                var orders = await _context.Orders
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                    .Select(o => new
                    {
                        o.Id,
                        o.OrderNumber,
                        o.CustomerName,
                        o.CustomerEmail,
                        o.Status,
                        o.OrderDate,
                        o.TotalAmount,
                        o.IsPaid,
                        ItemCount = o.OrderItems.Count,
                        Items = o.OrderItems.Select(oi => new
                        {
                            oi.ProductId,
                            ProductName = oi.Product.ProductName,
                            oi.Quantity,
                            oi.UnitPrice
                        })
                    })
                    .OrderByDescending(o => o.OrderDate)
                    .Take(10)
                    .ToListAsync();

                return Ok(new
                {
                    message = "Orders retrieved successfully",
                    orderCount = orders.Count,
                    orders = orders
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking orders");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Check distributor orders
        /// </summary>
        [HttpGet("check-distributor-orders")]
        [AllowAnonymous]
        public async Task<IActionResult> CheckDistributorOrders()
        {
            try
            {
                var distributorOrders = await _context.DistributorOrders
                    .Include(o => o.OrderItems)
                    .Select(o => new
                    {
                        o.Id,
                        o.OrderNumber,
                        o.DistributorOrderNumber,
                        o.Status,
                        o.OrderDate,
                        o.ExpectedDeliveryDate,
                        o.ActualDeliveryDate,
                        o.TotalAmount,
                        ItemCount = o.OrderItems.Count
                    })
                    .ToListAsync();

                return Ok(new
                {
                    message = "Distributor orders retrieved successfully",
                    orderCount = distributorOrders.Count,
                    distributorOrders = distributorOrders
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking distributor orders");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Reset password for a seller user (DEVELOPMENT ONLY - REMOVE IN PRODUCTION)
        /// </summary>
        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.NewPassword))
            {
                return BadRequest("Email and password are required");
            }

            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
                if (user == null)
                {
                    return NotFound($"User with email {request.Email} not found");
                }

                // Hash the new password
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
                user.UpdatedAt = DateTime.UtcNow;
                user.UpdatedBy = "PasswordReset";

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Password reset for seller user: {request.Email}");

                return Ok(new
                {
                    message = "Password reset successful",
                    email = user.Email,
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    role = user.Role.ToString(),
                    isActive = user.IsActive
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password for seller");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Test password verification for seller
        /// </summary>
        [HttpPost("test-login")]
        [AllowAnonymous]
        public async Task<IActionResult> TestLogin([FromBody] TestLoginRequest request)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
                if (user == null)
                {
                    return Ok(new { found = false, message = "User not found" });
                }

                var passwordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

                return Ok(new
                {
                    found = true,
                    email = user.Email,
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    isActive = user.IsActive,
                    passwordValid = passwordValid,
                    role = user.Role.ToString(),
                    companyName = user.CompanyName,
                    message = passwordValid ? "Password is correct" : "Password is incorrect"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing seller login");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Create a new seller user for testing
        /// </summary>
        [HttpPost("create-seller")]
        [AllowAnonymous]
        public async Task<IActionResult> CreateSeller([FromBody] CreateSellerRequest request)
        {
            try
            {
                // Check if user already exists
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
                if (existingUser != null)
                {
                    return BadRequest($"User with email {request.Email} already exists");
                }

                var user = new CozyComfort.Shared.Models.User
                {
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    CompanyName = request.CompanyName ?? "CozyComfort Retail Store",
                    Role = UserRole.Seller,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "TestController",
                    IsActive = true
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Created seller user: {request.Email}");

                return Ok(new
                {
                    message = "Seller user created successfully",
                    id = user.Id,
                    email = user.Email,
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    role = user.Role.ToString(),
                    companyName = user.CompanyName
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating seller user");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Add sample inventory transaction
        /// </summary>
        [HttpPost("add-inventory-transaction")]
        [AllowAnonymous]
        public async Task<IActionResult> AddInventoryTransaction([FromBody] AddInventoryTransactionRequest request)
        {
            try
            {
                var product = await _context.SellerProducts.FindAsync(request.ProductId);
                if (product == null)
                {
                    return NotFound($"Product with ID {request.ProductId} not found");
                }

                var transaction = new CozyComfort.Seller.API.Models.Entities.SellerInventoryTransaction
                {
                    ProductId = request.ProductId,
                    TransactionType = request.TransactionType,
                    Quantity = request.Quantity,
                    Reference = request.Reference ?? $"TEST-{DateTime.UtcNow:yyyyMMddHHmmss}",
                    Notes = request.Notes ?? "Test transaction",
                    TransactionDate = DateTime.UtcNow,
                    UnitCost = request.UnitCost,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "TestController",
                    IsActive = true
                };

                _context.InventoryTransactions.Add(transaction);

                // Update product stock based on transaction type
                if (request.TransactionType.ToUpper() == "IN" || request.TransactionType.ToUpper() == "PURCHASE")
                {
                    product.CurrentStock += request.Quantity;
                    product.DisplayStock += request.Quantity;
                }
                else if (request.TransactionType.ToUpper() == "OUT" || request.TransactionType.ToUpper() == "SALE")
                {
                    product.CurrentStock -= request.Quantity;
                    product.DisplayStock -= request.Quantity;
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Inventory transaction added successfully",
                    transactionId = transaction.Id,
                    productName = product.ProductName,
                    newStock = product.CurrentStock,
                    transactionType = transaction.TransactionType,
                    quantity = transaction.Quantity
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding inventory transaction");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Activate or deactivate a user
        /// </summary>
        [HttpPost("toggle-user-status")]
        [AllowAnonymous]
        public async Task<IActionResult> ToggleUserStatus([FromBody] ToggleUserStatusRequest request)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
                if (user == null)
                {
                    return NotFound($"User with email {request.Email} not found");
                }

                user.IsActive = request.IsActive;
                user.UpdatedAt = DateTime.UtcNow;
                user.UpdatedBy = "TestController";

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = $"User {(request.IsActive ? "activated" : "deactivated")} successfully",
                    email = user.Email,
                    isActive = user.IsActive
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling user status");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get database information
        /// </summary>
        [HttpGet("db-info")]
        [AllowAnonymous]
        public async Task<IActionResult> GetDatabaseInfo()
        {
            try
            {
                var totalUsers = await _context.Users.CountAsync();
                var activeUsers = await _context.Users.CountAsync(u => u.IsActive);
                var sellerUsers = await _context.Users.CountAsync(u => u.Role == UserRole.Seller);
                var adminUsers = await _context.Users.CountAsync(u => u.Role == UserRole.Administrator);
                var products = await _context.SellerProducts.CountAsync();
                var availableProducts = await _context.SellerProducts.CountAsync(p => p.IsAvailable);
                var customerOrders = await _context.Orders.CountAsync();
                var distributorOrders = await _context.DistributorOrders.CountAsync();
                var inventoryTransactions = await _context.InventoryTransactions.CountAsync();

                return Ok(new
                {
                    message = "Database information",
                    users = new
                    {
                        total = totalUsers,
                        active = activeUsers,
                        sellers = sellerUsers,
                        admins = adminUsers
                    },
                    products = new
                    {
                        total = products,
                        available = availableProducts
                    },
                    orders = new
                    {
                        customerOrders = customerOrders,
                        distributorOrders = distributorOrders
                    },
                    inventoryTransactions = inventoryTransactions,
                    databaseConnected = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting database info");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Create sample customer order
        /// </summary>
        [HttpPost("create-sample-order")]
        [AllowAnonymous]
        public async Task<IActionResult> CreateSampleOrder()
        {
            try
            {
                var products = await _context.SellerProducts.Where(p => p.IsAvailable).Take(2).ToListAsync();
                if (!products.Any())
                {
                    return BadRequest("No available products to create order");
                }

                var order = new CozyComfort.Seller.API.Models.Entities.CustomerOrder
                {
                    OrderNumber = $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}",
                    CustomerName = "Test Customer",
                    CustomerEmail = "test@customer.com",
                    CustomerPhone = "123-456-7890",
                    Status = CozyComfort.Shared.Enums.OrderStatus.Pending,
                    OrderDate = DateTime.UtcNow,
                    SubTotal = 0,
                    Tax = 0,
                    ShippingCost = 10.00M,
                    TotalAmount = 0,
                    ShippingAddress = "123 Test St, Test City, TC 12345",
                    BillingAddress = "123 Test St, Test City, TC 12345",
                    PaymentMethod = "Credit Card",
                    IsPaid = false,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "TestController",
                    IsActive = true,
                    OrderItems = new List<CozyComfort.Seller.API.Models.Entities.CustomerOrderItem>()
                };

                decimal subTotal = 0;
                foreach (var product in products)
                {
                    var orderItem = new CozyComfort.Seller.API.Models.Entities.CustomerOrderItem
                    {
                        ProductId = product.Id,
                        Quantity = 1,
                        UnitPrice = product.SellingPrice,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = "TestController",
                        IsActive = true
                    };
                    order.OrderItems.Add(orderItem);
                    subTotal += product.SellingPrice;
                }

                order.SubTotal = subTotal;
                order.Tax = subTotal * 0.08M; // 8% tax
                order.TotalAmount = order.SubTotal + order.Tax + order.ShippingCost;

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Sample order created successfully",
                    orderId = order.Id,
                    orderNumber = order.OrderNumber,
                    totalAmount = order.TotalAmount,
                    itemCount = order.OrderItems.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating sample order");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Request DTOs
    public class ResetPasswordRequest
    {
        public string Email { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    public class TestLoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class CreateSellerRequest
    {
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? CompanyName { get; set; }
    }

    public class ToggleUserStatusRequest
    {
        public string Email { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class AddInventoryTransactionRequest
    {
        public int ProductId { get; set; }
        public string TransactionType { get; set; } = string.Empty; // "IN", "OUT", "PURCHASE", "SALE", etc.
        public int Quantity { get; set; }
        public string? Reference { get; set; }
        public string? Notes { get; set; }
        public decimal? UnitCost { get; set; }
    }
}