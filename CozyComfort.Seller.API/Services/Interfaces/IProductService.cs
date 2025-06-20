﻿using CozyComfort.Shared.DTOs;
//using CozyComfort.Seller.API.Models.DTOs;
using CozyComfort.Shared.DTOs.Seller;

namespace CozyComfort.Seller.API.Services.Interfaces
{
    public interface IProductService
    {
        Task<ApiResponse<PagedResult<SellerProductDto>>> GetProductsAsync(PagedRequest request);
        Task<ApiResponse<SellerProductDto>> GetProductByIdAsync(int id);
    }

    // DTO for Seller products
    
}