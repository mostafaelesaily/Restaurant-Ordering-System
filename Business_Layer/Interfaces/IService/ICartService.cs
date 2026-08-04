using Business_Layer.DTOs.PaginatedDtos;
using Resturant_Ordering_System.Application.DTOs.CartDTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Resturant_Ordering_System.Application.Interfaces.IService
{
    public interface ICartService
    {
        Task<PaginatedResultDto<GetCartDto>> GetAllCartsAsync(int pageNumber, int pageSize);
        Task<GetCartDto> GetCartAsync(string customerId);
        Task AddToCartAsync(string customerId, AddToCartDto dto);
        Task UpdateCartItemAsync(string customerId, int cartItemId, UpdateCartDto dto);
        Task RemoveCartItemAsync(string customerId, int cartItemId);
        Task ClearCartAsync(string customerId);
    }
}
