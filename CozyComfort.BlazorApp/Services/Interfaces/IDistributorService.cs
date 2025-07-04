﻿// File: CozyComfort.BlazorApp/Services/Interfaces/IDistributorService.cs
using CozyComfort.Shared.DTOs;
using CozyComfort.Shared.DTOs.Distributor;
using CozyComfort.Shared.DTOs.Manufacturer;

namespace CozyComfort.BlazorApp.Services.Interfaces
{
    public interface IDistributorService
    {
        Task<ApiResponse<PagedResult<DistributorProductDto>>> GetProductsAsync(PagedRequest request);
        Task<ApiResponse<DistributorProductDto>> GetProductByIdAsync(int id);
        Task<ApiResponse<OrderDto>> CreateManufacturerOrderAsync(CreateManufacturerOrderDto dto);
        Task<ApiResponse<OrderDto>> ProcessSellerOrderAsync(ProcessSellerOrderDto dto);
        Task<ApiResponse<DistributorStockCheckResponse>> CheckStockAsync(DistributorStockCheckRequest request);
        Task<ApiResponse<PagedResult<OrderDto>>> GetOrdersAsync(int pageNumber, int pageSize);
        Task<ApiResponse<bool>> UpdateOrderStatusAsync(int orderId, string status);

        // ADD THIS METHOD
        Task<ApiResponse<DistributorProductDto>> AddProductFromManufacturerAsync(CreateDistributorProductDto dto);


        // Manufacturer Products for Order Creation
        Task<ApiResponse<PagedResult<ProductDto>>> GetManufacturerProductsAsync(PagedRequest request);
        Task<ApiResponse<ProductDto>> GetManufacturerProductByIdAsync(int id);
        Task<ApiResponse<StockCheckResponse>> CheckManufacturerStockAsync(StockCheckRequest request);
    }
}