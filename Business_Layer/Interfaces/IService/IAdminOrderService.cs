using Business_Layer.DTOs.PaginatedDtos;
using Resturant_Ordering_System.Application.DTOs.OrderDTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Resturant_Ordering_System.Application.Interfaces.IService
{
    public interface IAdminOrderService
    {
        Task<PaginatedResultDto<OrderDetailsDto>> GetAllOrders(int pageNum, int pageSize);
        Task<PaginatedResultDto<OrderDetailsDto>> SearchOrders(string searchKey, int pageNum, int pageSize);
        Task<OrderDetailsDto> GetOrderDetailsById(int orderId);
        Task<OrderSummaryDto> CreateOrderByAdmin(CreateOrderByAdminDto orderCreateDto);
        Task UpdateOrderStatus(int orderId, UpdateOrderStatusDto dto);
        Task AssignChef(int orderId, string chefId);
        Task AssignDelivery(int orderId, string deliveryId);
        Task DeleteOrder(int orderId);

    }
}
