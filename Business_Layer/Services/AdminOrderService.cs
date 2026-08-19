using Application.Interfaces.IService;
using AutoMapper;
using Business_Layer.DTOs.NotificationDTOs;
using Business_Layer.DTOs.PaginatedDtos;
using Business_Layer.Exceptions;
using Business_Layer.Interfaces;
using Domain_Layer.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Resturant_Ordering_System.Application.DTOs.OrderDTOs;
using Resturant_Ordering_System.Application.Interfaces.IService;

public class AdminOrderService : IAdminOrderService
{
    private readonly INotificationService notificationService;
    private readonly ISendNotificationService sendNotificationService;
    private readonly ILogger<AdminOrderService> logger;
    private readonly IUow uow;
    private readonly ICacheService cacheService;
    private readonly IMapper mapper;
    private readonly ICouponService couponService;
    private readonly UserManager<AppUser> userManager;
    public AdminOrderService(
        INotificationService notificationService,
        ILogger<AdminOrderService> logger,
        IUow uow,
        ICacheService cacheService,
        IMapper mapper,
        ISendNotificationService sendNotificationService,
        ICouponService couponService,
        UserManager<AppUser> userManager
        )
    {
        this.notificationService = notificationService;
        this.logger = logger;
        this.uow = uow;
        this.cacheService = cacheService;
        this.mapper = mapper;
        this.sendNotificationService = sendNotificationService;
        this.couponService = couponService;
        this.userManager = userManager;
    }

    public async Task AssignChef(int orderId, string chefId)
    {
        var order = await uow.Orders.GetByIdAsync(orderId);

        if (order == null)
        {
            logger.LogInformation(
                "Order with Id {orderId} not found",
                orderId);

            throw new NotFoundException("Order not found");
        }

        var chef = await uow.AppUserRepo.GetByIdAsync(chefId);

        if (chef == null)
        {
            logger.LogInformation(
                "Chef with Id {chefId} not found",
                chefId);

            throw new NotFoundException("Chef not found");
        }
        var checkRole = await userManager.IsInRoleAsync(chef, "Cheif");
        if (!checkRole)
        {
            logger.LogInformation("User with id : {userId} is not a cheif", chefId);
            throw new BadRequestException($"Selected User is not a cheif !");
        }


        order.CheifId = chefId;
        order.UpdatedAt = DateTime.UtcNow;
        var notification = new CreateNotificationDto
        {
            Title = "New Order Assigned",
            Message = $"Order #{order.id} has been assigned to you.",
            UserId = chefId
        };

        await notificationService.CreateAsync(notification);

        await uow.SaveChangesAsync();

        await sendNotificationService.SendToUserAsync(
            chefId,
            $"Order #{order.id} has been assigned to you."
        );

        await cacheService.RemoveAsync("Get_Orders");
    }

    public async Task AssignDelivery(int orderId, string deliveryId)
    {
        var order = await uow.Orders.GetByIdAsync(orderId);

        if (order == null)
        {
            logger.LogInformation(
                "Order with Id {orderId} not found",
                orderId);

            throw new NotFoundException("Order not found");
        }

        var delivery = await uow.AppUserRepo.GetByIdAsync(deliveryId);

        if (delivery == null)
        {
            logger.LogInformation(
                "Delivery with Id {deliveryId} not found",
                deliveryId);

            throw new NotFoundException("Delivery not found");
        }
        var checkRole = await userManager.IsInRoleAsync(delivery, "Delivery");
        if (!checkRole)
        {
            logger.LogInformation("User with id : {userId} is not a Delivery", deliveryId);
            throw new BadRequestException($"Selected User is not a Delivery !");
        }
        order.DeliveryId = deliveryId;
        order.UpdatedAt = DateTime.UtcNow;

        var notification = new CreateNotificationDto
        {
            Title = "New Order Assigned",
            Message = $"Order #{order.id} has been assigned to you.",
            UserId = deliveryId
        };

        await notificationService.CreateAsync(notification);

        await uow.SaveChangesAsync();

        await sendNotificationService.SendToUserAsync(
            deliveryId,
            $"Order #{order.id} has been assigned to you."
        );

        await cacheService.RemoveAsync("Get_Orders");
    }

    public async Task<OrderSummaryDto> CreateOrderByAdmin(
    CreateOrderByAdminDto orderCreateDto)
    {
        logger.LogInformation(
            "Attempting to create order for Customer {customerId}",
            orderCreateDto.CustomerId);

        var customer = await uow.AppUserRepo
            .GetByIdAsync(orderCreateDto.CustomerId);

        if (customer == null)
        {
            logger.LogWarning(
                "Customer with Id {customerId} not found",
                orderCreateDto.CustomerId);

            throw new NotFoundException("Customer not found");
        }

        if (orderCreateDto.itemDtos == null || !orderCreateDto.itemDtos.Any())
        {
            throw new BadRequestException("Order must contain at least one item.");
        }

        var adminOrder = mapper.Map<Orders>(orderCreateDto);

        adminOrder.customerId = orderCreateDto.CustomerId;
        adminOrder.orderItems = new List<OrderItems>();
        adminOrder.TotalPrice = 0;

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

            adminOrder.orderItems.Add(orderItem);

            adminOrder.TotalPrice += menuItem.price * itemDto.Quantity;
        }

        if (!string.IsNullOrWhiteSpace(orderCreateDto.CouponCode))
        {
            var coupon = await couponService.ValidateCoupon(
                orderCreateDto.CouponCode);

            if (coupon == null)
            {
                throw new NotFoundException("Coupon not found");
            }

            adminOrder.couponId = coupon.Id;

            var discountAmount =
                adminOrder.TotalPrice *
                (coupon.Discount / 100m);

            adminOrder.TotalPrice -= discountAmount;

            if (adminOrder.TotalPrice < 0)
            {
                adminOrder.TotalPrice = 0;
            }
        }

        await uow.Orders.CreateAsync(adminOrder);

        var notification = new CreateNotificationDto
        {
            Title = "New Order Created",
            Message =
                $"A new order #{adminOrder.id} has been created for you by an admin.",
            UserId = orderCreateDto.CustomerId
        };

        await notificationService.CreateAsync(notification);

        await uow.SaveChangesAsync();

        await sendNotificationService.SendToUserAsync(
            orderCreateDto.CustomerId,
            $"A new order #{adminOrder.id} has been created for you by an admin."
        );

        await cacheService.RemoveAsync("Get_Orders");

        return mapper.Map<OrderSummaryDto>(adminOrder);
    }

    public async Task DeleteOrder(int orderId)
    {
        var order = await uow.Orders.GetByIdAsync(orderId);

        if (order == null)
        {
            logger.LogInformation(
                "Order with Id {orderId} not found",
                orderId);

            throw new NotFoundException("Order not found");
        }

        await uow.Orders.DeleteAsync(order);

        var notification = new CreateNotificationDto
        {
            Title = "Order Cancelled",
            Message = $"Your order #{order.id} has been cancelled.",
            UserId = order.customerId
        };

        await notificationService.CreateAsync(notification);

        await uow.SaveChangesAsync();

        await sendNotificationService.SendToUserAsync(
            order.customerId,
            $"Your order #{order.id} has been cancelled."
        );

        await cacheService.RemoveAsync("Get_Orders");
    }

    public async Task<PaginatedResultDto<OrderDetailsDto>> GetAllOrders(
        int pageNum,
        int pageSize)
    {
        logger.LogInformation(
            "Attempting to get Orders page {pageNum} and count {pageSize}",
            pageNum,
            pageSize);

        var cacheKey =
            $"Get_Orders_pageNum:{pageNum}_pageSize:{pageSize}";

        var result = await cacheService.GetOrSetAsync(
            cacheKey,
            async () =>
            {
                var query =  uow.Orders.GetOrdersWithDetails() ;
                var orders = await uow.Orders.GetAllPaged(pageNum, pageSize, query);

                return new PaginatedResultDto<OrderDetailsDto>
                {
                    Data = mapper.Map<List<OrderDetailsDto>>(orders.Data),
                    PageNumber = pageNum,
                    PageSize = pageSize,
                    TotalCount = orders.TotalCount
                };
            });

        return result;
    }

    public async Task<OrderDetailsDto> GetOrderDetailsById(int orderId)
    {
        logger.LogInformation(
            "Attempting to get Order with Id {orderId}",
            orderId);

        var order = await uow.Orders.GetOrderWithDetails(orderId);

        if (order == null)
        {
            logger.LogInformation(
                "Order with Id {orderId} not found",
                orderId);

            throw new NotFoundException("Order not found");
        }

        return mapper.Map<OrderDetailsDto>(order);
    }

    public async Task<PaginatedResultDto<OrderDetailsDto>> SearchOrders(
        string searchKey,
        int pageNum,
        int pageSize)
    {
        logger.LogInformation(
            "Attempting to search Orders page {pageNum} and count {pageSize}",
            pageNum,
            pageSize);

        var cacheKey =
            $"Search_Orders_{searchKey}_pageNum:{pageNum}_pageSize:{pageSize}";

        var result = await cacheService.GetOrSetAsync(
            cacheKey,
            async () =>
            {
                var query = uow.Orders.SearchOrder(searchKey);
                var orders =
                    await uow.Orders.GetAllPaged(
                        pageNum,
                        pageSize,
                        query);

                return new PaginatedResultDto<OrderDetailsDto>
                {
                    Data = mapper.Map<List<OrderDetailsDto>>(orders.Data),
                    PageNumber = pageNum,
                    PageSize = pageSize,
                    TotalCount = orders.TotalCount
                };
            });

        return result;
    }

    public async Task UpdateOrderStatus(
        int orderId,
        UpdateOrderStatusDto dto)
    {
        var order = await uow.Orders.GetByIdAsync(orderId);

        if (order == null)
        {
            logger.LogInformation(
                "Order with Id {orderId} not found",
                orderId);

            throw new NotFoundException("Order not found");
        }

        order.Status = dto.Status;
        order.UpdatedAt = DateTime.UtcNow;
        var notification = new CreateNotificationDto
        {
            Title = "Order Status Updated",
            Message =
                $"Your order #{order.id} status is now {order.Status}.",
            UserId = order.customerId
        };

        await notificationService.CreateAsync(notification);

        await uow.SaveChangesAsync();

        await sendNotificationService.SendToUserAsync(
            order.customerId,
            $"Your order #{order.id} status is now {order.Status}."
        );

        await cacheService.RemoveAsync("Get_Orders");
    }
}