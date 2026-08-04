using AutoMapper;
using Business_Layer.DTOs.PaginatedDtos;
using Business_Layer.Exceptions;
using Business_Layer.Interfaces;
using Domain_Layer.Entities;
using Microsoft.Extensions.Logging;
using Resturant_Ordering_System.Application.DTOs.CartDTOs;
using Resturant_Ordering_System.Application.Interfaces.IService;
using System;
using System.Collections.Generic;
using System.Text;

namespace Resturant_Ordering_System.Application.Services
{
    public class CartService : ICartService
    {
        private readonly ILogger<CartService> logger;
        private readonly IUow uow;
        private readonly IMapper mapper;
        private readonly ICacheService cacheService;
        public CartService
            (
            ILogger<CartService> logger,
            IUow uow,
            IMapper mapper,
            ICacheService cacheService
            ) 
        {
            this.logger = logger;
            this.uow = uow;
            this.mapper = mapper;
            this.cacheService = cacheService;
        }
        public async Task AddToCartAsync(string customerId, AddToCartDto dto)
        {
            logger.LogInformation(
                "Attempting to add menu item {menuItemId} to cart for customer {customerId}",
                dto.MenuItemId,
                customerId);

            await using var transaction = await uow.BeginTransactionAsync();

            try
            {
                var menuItem = await uow.MenuItems.GetByIdAsync(dto.MenuItemId);

                if (menuItem == null)
                {
                    logger.LogWarning("Menu item with id {id} not found", dto.MenuItemId);
                    throw new NotFoundException("Menu item not found");
                }

                var cart = await uow.Cart.GetCartWithItemsAsync(customerId);

                if (cart == null)
                {
                    cart = new Cart
                    {
                        CustomerId = customerId
                    };

                    await uow.Cart.CreateAsync(cart);
                }

                var cartItem = cart.Items
                    .FirstOrDefault(i => i.MenuItemId == dto.MenuItemId);

                if (cartItem != null)
                {
                    cartItem.Quantity += dto.Quantity;
                }
                else
                {
                    cart.Items.Add(mapper.Map<CartItem>(dto));
                }

                await uow.SaveChangesAsync();
                await transaction.CommitAsync();
                await cacheService.RemoveAsync("Get_Carts");
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task ClearCartAsync(string customerId)
        {
            logger.LogInformation(
                "Attempting to clear cart for customer {customerId}",
                customerId);

            await using var transaction = await uow.BeginTransactionAsync();

            try
            {
                var cart = await uow.Cart.GetCartWithItemsAsync(customerId);

                if (cart == null)
                {
                    logger.LogWarning(
                        "Cart for customer {customerId} not found",
                        customerId);

                    throw new NotFoundException("Cart not found");
                }

                cart.Items.Clear();

                await uow.SaveChangesAsync();
                await transaction.CommitAsync();
                await cacheService.RemoveAsync("Get_Carts");
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<PaginatedResultDto<GetCartDto>> GetAllCartsAsync(int pageNumber, int pageSize)
        {
            logger.LogInformation(
                "Attempting to get carts page {pageNumber} size {pageSize}",
                pageNumber,
                pageSize);

            var cacheKey = $"Get_Carts_page:{pageNumber}_size:{pageSize}";

            var result = await cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                var query = uow.Cart.GetAllWithItems();

                var carts = await uow.Cart.GetAllPaged(pageNumber, pageSize, query);

                return new PaginatedResultDto<GetCartDto>
                {
                    Data = mapper.Map<List<GetCartDto>>(carts.Data),
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = carts.TotalCount
                };
            });

            return result!;
        }

        public async Task<GetCartDto> GetCartAsync(string customerId)
        {
            logger.LogInformation($"Fetching cart for customer: {customerId}");
            var cart = await uow.Cart.GetCartWithItemsAsync(customerId);
            if (cart == null) 
            {
                logger.LogWarning($"Cart not found for customer: {customerId}");
                throw new NotFoundException("Cart not found");
            }
            return mapper.Map<GetCartDto>(cart);
        }

        public async Task RemoveCartItemAsync(string customerId, int cartItemId)
        {
            logger.LogInformation(
                "Attempting to remove cart item {cartItemId} for customer {customerId}",
                cartItemId,
                customerId);

            await using var transaction = await uow.BeginTransactionAsync();

            try
            {
                var cart = await uow.Cart.GetCartWithItemsAsync(customerId);

                if (cart == null)
                {
                    logger.LogWarning("Cart for customer {customerId} not found", customerId);
                    throw new NotFoundException("Cart not found");
                }

                var cartItem = cart.Items.FirstOrDefault(i => i.Id == cartItemId);

                if (cartItem == null)
                {
                    logger.LogWarning("Cart item {cartItemId} not found", cartItemId);
                    throw new NotFoundException("Cart item not found");
                }

                cart.Items.Remove(cartItem);

                await uow.SaveChangesAsync();
                await transaction.CommitAsync();

                await cacheService.RemoveAsync("Get_Carts");
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task UpdateCartItemAsync(string customerId, int cartItemId, UpdateCartDto dto)
        {
            logger.LogInformation(
                "Attempting to update cart item {cartItemId} for customer {customerId}",
                cartItemId,
                customerId);

            await using var transaction = await uow.BeginTransactionAsync();

            try
            {
                var cart = await uow.Cart.GetCartWithItemsAsync(customerId);

                if (cart == null)
                {
                    logger.LogWarning("Cart for customer {customerId} not found", customerId);
                    throw new NotFoundException("Cart not found");
                }

                var cartItem = cart.Items.FirstOrDefault(i => i.Id == cartItemId);

                if (cartItem == null)
                {
                    logger.LogWarning("Cart item {cartItemId} not found", cartItemId);
                    throw new NotFoundException("Cart item not found");
                }

                cartItem.Quantity = dto.Quantity;

                await uow.SaveChangesAsync();
                await transaction.CommitAsync();

                await cacheService.RemoveAsync("Get_Carts");
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
