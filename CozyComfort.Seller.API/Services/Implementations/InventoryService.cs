using CozyComfort.Seller.API.Data;
using CozyComfort.Seller.API.Services.Interfaces;

namespace CozyComfort.Seller.API.Services.Implementations
{
    public class InventoryService : IInventoryService
    {
        private readonly SellerDbContext _context;

        public InventoryService(SellerDbContext context)
        {
            _context = context;
        }

        public async Task<bool> UpdateStockAfterOrderAsync(int productId, int quantity)
        {
            // Implementation
            throw new NotImplementedException();
        }

        public async Task<bool> CheckStockAvailabilityAsync(int productId, int quantity)
        {
            var product = await _context.SellerProducts.FindAsync(productId);
            return product != null && product.CurrentStock >= quantity;
        }
    }
}