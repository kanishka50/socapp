using CozyComfort.Shared.Models;
using CozyComfort.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace CozyComfort.Distributor.API.Data
{
    public static class DistributorDbInitializer
    {
        public static async Task InitializeAsync(DistributorDbContext context)
        {
            // Ensure database is created
            await context.Database.EnsureCreatedAsync();

            // Check if we need to add the seller-api user
            var sellerApiUser = await context.Users
                .FirstOrDefaultAsync(u => u.Email == "seller-api@cozycomfort.com");

            if (sellerApiUser == null)
            {
                sellerApiUser = new User
                {
                    Email = "seller-api@cozycomfort.com",
                    FirstName = "Seller",
                    LastName = "API",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("SellerAPI123!"),
                    Role = UserRole.System, // You'll need to add this to the enum
                    CompanyName = "Seller API Service",
                    Phone = "0000000000",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                context.Users.Add(sellerApiUser);
                await context.SaveChangesAsync();

                Console.WriteLine("Seller API user created successfully");
            }
        }
    }
}