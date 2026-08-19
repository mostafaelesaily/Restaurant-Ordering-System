using Business_Layer.Interfaces.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Resturant_Ordering_System.Application.DTOs.OrderDTOs;
using Resturant_Ordering_System.Application.Interfaces.IService;
using System.Security.Claims;

namespace Resturant_Ordering_System.Api_Layer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService orderService;

        public OrderController(IOrderService orderService)
        {
            this.orderService = orderService;
        }

        [HttpPost("[action]")]
        [Authorize]
        public async Task<IActionResult> CreateOrder(CreateOrderDto orderCreateDto)
        {
            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await orderService.CreateOrder(orderCreateDto, customerId);
            return Ok(result);
        }

        [HttpPatch("[action]")]
        [Authorize]
        public async Task<IActionResult> CancelOrder(int orderId)
        {
            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await orderService.CancelOrder(orderId, customerId);
            return NoContent();
        }

        [HttpGet("[action]")]
        [Authorize]
        public async Task<IActionResult> GetMyOrderDetails(int orderId)
        {
            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await orderService.GetMyOrderDetails(orderId, customerId);
            return Ok(result);
        }

        [HttpGet("[action]")]
        [Authorize]
        public async Task<IActionResult> GetMyOrders(int pageNum, int pageSize)
        {
            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await orderService.GetMyOrders(customerId, pageNum, pageSize);
            return Ok(result);
        }
    }
}