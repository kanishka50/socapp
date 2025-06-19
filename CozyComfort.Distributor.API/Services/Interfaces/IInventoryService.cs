namespace CozyComfort.Distributor.API.Services.Interfaces
{
    public interface IInventoryService
    {
        Task<bool> ReserveStockAsync(int productId, int quantity, string orderReference);
        Task<bool> ReleaseStockAsync(int productId, int quantity, string orderReference);
        Task<bool> UpdateStockLevelAsync(int productId, int newQuantity);
    }
}