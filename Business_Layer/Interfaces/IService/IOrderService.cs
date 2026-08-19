using Business_Layer.DTOs.PaginatedDtos;
using Resturant_Ordering_System.Application.DTOs.OrderDTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Resturant_Ordering_System.Application.Interfaces.IService
{
    public interface IOrderService
    {
        Task<OrderSummaryDto> CreateOrder(CreateOrderDto orderCreateDto, string customerId);
        Task<PaginatedResultDto<OrderSummaryDto>> GetMyOrders(
            string customerId,
            int pageNum,
            int pageSize);
        Task<OrderDetailsDto> GetMyOrderDetails(
            int orderId,
            string customerId);
        Task CancelOrder(
            int orderId,
            string customerId);
    }
}
