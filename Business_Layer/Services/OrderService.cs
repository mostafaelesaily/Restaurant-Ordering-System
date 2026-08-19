using Application.Interfaces.IService;
using AutoMapper;
using Business_Layer.DTOs.NotificationDTOs;
using Business_Layer.DTOs.PaginatedDtos;
using Business_Layer.Exceptions;
using Business_Layer.Interfaces;
using Domain_Layer.Entities;
using Domain_Layer.Enums;
using Microsoft.Extensions.Logging;
using Resturant_Ordering_System.Application.DTOs.OrderDTOs;
using Resturant_Ordering_System.Application.Interfaces.IService;

namespace Resturant_Ordering_System.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly INotificationService notificationService;
        private readonly ISendNotificationService sendNotificationService;
        private readonly IMapper mapper;
        private readonly ILogger<OrderService> logger;
        private readonly IUow uow;
        private readonly ICacheService cacheService;
        private readonly ICouponService couponService;

        public OrderService(
            INotificationService notificationService,
            ISendNotificationService sendNotificationService,
            IMapper mapper,
            IUow uow,
            ICacheService cacheService,
            ICouponService couponService,
            ILogger<OrderService> logger)
        {
            this.notificationService = notificationService;
            this.sendNotificationService = sendNotificationService;
            this.mapper = mapper;
            this.uow = uow;
            this.cacheService = cacheService;
            this.couponService = couponService;
            this.logger = logger;
        }

        public async Task CancelOrder(int orderId, string customerId)
        {
            logger.LogInformation(
                "Attempting to cancel order with id {OrderId}",
                orderId);

            var order = await uow.Orders.GetByIdAsync(orderId);

            if (order == null)
            {
                logger.LogWarning(
                    "Order with Id {OrderId} not found",
                    orderId);

                throw new NotFoundException("Order Not Found");
            }

            var customer = await uow.AppUserRepo.GetByIdAsync(customerId);

            if (customer == null)
            {
                logger.LogWarning(
                    "Customer with Id {CustomerId} not found",
                    customerId);

                throw new NotFoundException("Customer Not Found");
            }

            if (order.customerId != customerId)
            {
                throw new UnauthorizedException(
                    "You are not authorized to cancel this order.");
            }

            if (order.Status == OrderStatus.Cancelled)
            {
                throw new BadRequestException(
                    "Order is already cancelled.");
            }

            if (order.Status != OrderStatus.Pending)
            {
                throw new BadRequestException(
                    "Order cannot be cancelled at this stage.");
            }

            order.Status = OrderStatus.Cancelled;

            await uow.SaveChangesAsync();

            await cacheService.RemoveAsync(
                $"Get_Customer_Orders_{customerId}");

            logger.LogInformation(
                "Order with id {OrderId} cancelled successfully by Customer {CustomerId}",
                orderId,
                customerId);
        }

        public async Task<OrderSummaryDto> CreateOrder(
    CreateOrderDto orderCreateDto,
    string customerId)
        {
            logger.LogInformation(
                "Attempting to create order for Customer {CustomerId}",
                customerId);

            var customer = await uow.AppUserRepo
                .GetByIdAsync(customerId);

            if (customer == null)
            {
                logger.LogWarning(
                    "Customer with Id {CustomerId} not found",
                    customerId);

                throw new NotFoundException("Customer not found");
            }

            if (orderCreateDto.itemDtos == null || !orderCreateDto.itemDtos.Any())
            {
                throw new BadRequestException("Order must contain at least one item.");
            }

            var order = mapper.Map<Orders>(orderCreateDto);

            order.customerId = customerId;
            order.orderItems = new List<OrderItems>();
            order.TotalPrice = 0;

            foreach (var itemDto in orderCreateDto.itemDtos)
            {
                var menuItem = await uow.MenuItems.GetByIdAsync(itemDto.MenuItemId);

                if (menuItem == null)
                {
                    throw new NotFoundException(
                        $"Menu item with id {itemDto.MenuItemId} not found.");
                }

                if (!menuItem.isAvailable)
                {
                    throw new BadRequestException(
                        $"Menu item {menuItem.name} is not available.");
                }

                if (itemDto.Quantity <= 0)
                {
                    throw new BadRequestException(
                        "Quantity must be greater than zero.");
                }

                var orderItem = new OrderItems
                {
                    menuItemId = menuItem.id,
                    Quantity = itemDto.Quantity,
                    unitPrice = menuItem.price
                };

                order.orderItems.Add(orderItem);

                order.TotalPrice += menuItem.price * itemDto.Quantity;
            }

            if (!string.IsNullOrWhiteSpace(orderCreateDto.CouponCode))
            {
                var coupon = await couponService.ValidateCoupon(
                    orderCreateDto.CouponCode);

                if (coupon == null)
                {
                    logger.LogWarning(
                        "Coupon with Code {CouponCode} not found",
                        orderCreateDto.CouponCode);

                    throw new NotFoundException("Coupon not found");
                }

                order.couponId = coupon.Id;

                var discountAmount =
                    order.TotalPrice *
                    (coupon.Discount / 100m);

                order.TotalPrice -= discountAmount;

                if (order.TotalPrice < 0)
                {
                    order.TotalPrice = 0;
                }
            }

            await uow.Orders.CreateAsync(order);

            var notification = new CreateNotificationDto
            {
                Title = "Order Created",
                Message =
                    $"Your order #{order.id} has been created successfully.",
                UserId = customerId
            };

            await notificationService.CreateAsync(notification);

            await uow.SaveChangesAsync();

            await sendNotificationService.SendToUserAsync(
                customerId,
                $"Your order #{order.id} has been created successfully."
            );

            await cacheService.RemoveAsync("Get_Orders");

            await cacheService.RemoveAsync(
                $"Get_Customer_Orders_{customerId}");

            logger.LogInformation(
                "Order {OrderId} created successfully for Customer {CustomerId}",
                order.id,
                customerId);

            return mapper.Map<OrderSummaryDto>(order);
        }
        public async Task<OrderDetailsDto> GetMyOrderDetails(
            int orderId,
            string customerId)
        {
            logger.LogInformation(
                "Attempting to get order details for Order {OrderId} by Customer {CustomerId}",
                orderId,
                customerId);

            var order = await uow.Orders.GetOrderWithDetails(orderId);

            if (order == null)
            {
                logger.LogWarning(
                    "Order with Id {OrderId} not found",
                    orderId);

                throw new NotFoundException("Order Not Found");
            }

            var customer = await uow.AppUserRepo
                .GetByIdAsync(customerId);

            if (customer == null)
            {
                logger.LogWarning(
                    "Customer with Id {CustomerId} not found",
                    customerId);

                throw new NotFoundException("Customer Not Found");
            }

            if (order.customerId != customerId)
            {
                throw new UnauthorizedException(
                    "You are not authorized to get this order.");
            }

            return mapper.Map<OrderDetailsDto>(order);
        }

        public async Task<PaginatedResultDto<OrderSummaryDto>> GetMyOrders(
            string customerId,
            int pageNum,
            int pageSize)
        {
            logger.LogInformation(
                "Attempting to get orders for Customer {CustomerId}, PageNum: {PageNum}, PageSize: {PageSize}",
                customerId,
                pageNum,
                pageSize);

            var customer = await uow.AppUserRepo
                .GetByIdAsync(customerId);

            if (customer == null)
            {
                logger.LogWarning(
                    "Customer with Id {CustomerId} not found",
                    customerId);

                throw new NotFoundException("Customer Not Found");
            }

            var cacheKey =
                $"Get_Customer_Orders_{customerId}_pageNum:{pageNum}_pageSize:{pageSize}";

            var result = await cacheService.GetOrSetAsync(
                cacheKey,
                async () =>
                {
                    var query =
                        uow.Orders.GetOrdersByCustomerId(customerId);

                    var orders =
                        await uow.Orders.GetAllPaged(
                            pageNum,
                            pageSize,
                            query);

                    return new PaginatedResultDto<OrderSummaryDto>
                    {
                        Data = mapper.Map<List<OrderSummaryDto>>(
                            orders.Data),
                        PageNumber = pageNum,
                        PageSize = pageSize,
                        TotalCount = orders.TotalCount
                    };
                });

            return result!;
        }
    }
}