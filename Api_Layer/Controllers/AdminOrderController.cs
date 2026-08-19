using Business_Layer.DTOs;
using Business_Layer.Interfaces.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Resturant_Ordering_System.Application.DTOs.OrderDTOs;
using Resturant_Ordering_System.Application.Interfaces.IService;

namespace Resturant_Ordering_System.Api_Layer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminOrderController : ControllerBase
    {
        private readonly IAdminOrderService adminOrderService;

        public AdminOrderController(IAdminOrderService adminOrderService)
        {
            this.adminOrderService = adminOrderService;
        }

        [HttpGet("[action]")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetAllOrders(int pageNum, int pageSize)
        {
            var result = await adminOrderService.GetAllOrders(pageNum, pageSize);
            return Ok(result);
        }

        [HttpGet("[action]")]
        [Authorize(Roles = "Admin,Manager,Cheif,Delivery")]
        public async Task<IActionResult> GetOrderDetailsById(int orderId)
        {
            var result = await adminOrderService.GetOrderDetailsById(orderId);
            return Ok(result);
        }

        [HttpGet("[action]")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> SearchOrders(string searchKey, int pageNum, int pageSize)
        {
            var result = await adminOrderService.SearchOrders(searchKey, pageNum, pageSize);
            return Ok(result);
        }

        [HttpPost("[action]")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> CreateOrderByAdmin(CreateOrderByAdminDto orderCreateDto)
        {
            var result = await adminOrderService.CreateOrderByAdmin(orderCreateDto);
            return Ok(result);
        }

        [HttpPatch("[action]")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> AssignChef(int orderId, string chefId)
        {
            await adminOrderService.AssignChef(orderId, chefId);
            return NoContent();
        }

        [HttpPatch("[action]")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> AssignDelivery(int orderId, string deliveryId)
        {
            await adminOrderService.AssignDelivery(orderId, deliveryId);
            return NoContent();
        }

        [HttpPatch("[action]")]
        [Authorize(Roles = "Admin,Manager,Cheif,Delivery")]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, UpdateOrderStatusDto dto)
        {
            await adminOrderService.UpdateOrderStatus(orderId, dto);
            return NoContent();
        }

        [HttpDelete("[action]")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteOrder(int orderId)
        {
            await adminOrderService.DeleteOrder(orderId);
            return NoContent();
        }
    }
}