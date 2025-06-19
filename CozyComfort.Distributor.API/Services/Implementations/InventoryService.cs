using CozyComfort.Distributor.API.Data;
using CozyComfort.Distributor.API.Services.Interfaces;

namespace CozyComfort.Distributor.API.Services.Implementations
{
    public class InventoryService : IInventoryService
    {
        private readonly DistributorDbContext _context;

        public InventoryService(DistributorDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ReserveStockAsync(int productId, int quantity, string orderReference)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> ReleaseStockAsync(int productId, int quantity, string orderReference)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> UpdateStockLevelAsync(int productId, int newQuantity)
        {
            throw new NotImplementedException();
        }
    }
}