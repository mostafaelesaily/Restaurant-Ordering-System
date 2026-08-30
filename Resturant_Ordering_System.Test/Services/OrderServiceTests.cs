using Application.DTOs.CouponDTOs;
using Application.Interfaces.IService;
using AutoMapper;
using Business_Layer.DTOs.NotificationDTOs;
using Business_Layer.DTOs.PaginatedDtos;
using Business_Layer.Exceptions;
using Business_Layer.Interfaces;
using Domain_Layer.Abstract;
using Domain_Layer.Entities;
using Domain_Layer.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using Resturant_Ordering_System.Application.DTOs.OrderDTOs;
using Resturant_Ordering_System.Application.Interfaces.IService;
using Resturant_Ordering_System.Application.Services;
using Resturant_Ordering_System.Domain.Abstract;
using Resturant_Ordering_System.Test.Helpers;

namespace Resturant_Ordering_System.Test.Services
{
    public class OrderServiceTests
    {
        private readonly Mock<IUow> _uow = new();
        private readonly Mock<IOrderRepo> _orders = new();
        private readonly Mock<IGenaricRepo<AppUser, string>> _users = new();
        private readonly Mock<IMenuItemRepo> _menuItems = new();
        private readonly Mock<ICacheService> _cache = new();
        private readonly Mock<IMapper> _mapper = new();
        private readonly Mock<ILogger<OrderService>> _logger = new();
        private readonly Mock<INotificationService> _notifications = new();
        private readonly Mock<ISendNotificationService> _sendNotifications = new();
        private readonly Mock<ICouponService> _coupons = new();
        private readonly OrderService _sut;

        public OrderServiceTests()
        {
            _uow.Setup(u => u.Orders).Returns(_orders.Object);
            _uow.Setup(u => u.AppUserRepo).Returns(_users.Object);
            _uow.Setup(u => u.MenuItems).Returns(_menuItems.Object);
            _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
            TestMockHelpers.SetupCacheToExecuteFactory<PaginatedResultDto<OrderSummaryDto>>(_cache);
            _sut = new OrderService(
                _notifications.Object,
                _sendNotifications.Object,
                _mapper.Object,
                _uow.Object,
                _cache.Object,
                _coupons.Object,
                _logger.Object);
        }

        private static CreateOrderDto CreateDto(params CreateOrderItemDto[] items) =>
            new() { Address = "Street 1", itemDtos = items.ToList() };

        [Fact]
        public async Task CreateOrder_WhenValidWithoutCoupon_CreatesOrderAndNotifies()
        {
            var dto = CreateDto(new CreateOrderItemDto { MenuItemId = 8, Quantity = 2 });
            var order = new Orders();
            _users.Setup(r => r.GetByIdAsync("u1")).ReturnsAsync(new AppUser { Id = "u1" });
            _mapper.Setup(m => m.Map<Orders>(dto)).Returns(order);
            _menuItems.Setup(r => r.GetByIdAsync(8))
                .ReturnsAsync(new MenuItems { id = 8, name = "Burger", price = 50, isAvailable = true });
            _mapper.Setup(m => m.Map<OrderSummaryDto>(order)).Returns(new OrderSummaryDto { totalPrice = 100 });

            var result = await _sut.CreateOrder(dto, "u1");

            Assert.Equal(100, order.TotalPrice);
            Assert.Equal("u1", order.customerId);
            Assert.Single(order.orderItems);
            _orders.Verify(r => r.CreateAsync(order), Times.Once);
            _notifications.Verify(n => n.CreateAsync(It.IsAny<CreateNotificationDto>()), Times.Once);
            _sendNotifications.Verify(s => s.SendToUserAsync("u1", It.IsAny<string>()), Times.Once);
            Assert.Equal(100, result.totalPrice);
        }

        [Fact]
        public async Task CreateOrder_WhenCouponIsApplied_ReducesTotalPrice()
        {
            var dto = CreateDto(new CreateOrderItemDto { MenuItemId = 8, Quantity = 2 });
            dto.CouponCode = "SAVE10";
            var order = new Orders();
            _users.Setup(r => r.GetByIdAsync("u1")).ReturnsAsync(new AppUser { Id = "u1" });
            _mapper.Setup(m => m.Map<Orders>(dto)).Returns(order);
            _menuItems.Setup(r => r.GetByIdAsync(8))
                .ReturnsAsync(new MenuItems { id = 8, name = "Burger", price = 50, isAvailable = true });
            _coupons.Setup(c => c.ValidateCoupon("SAVE10"))
                .ReturnsAsync(new GetCouponDto { Id = 12, Discount = 10, Code = "SAVE10" });
            _mapper.Setup(m => m.Map<OrderSummaryDto>(order)).Returns(new OrderSummaryDto());

            await _sut.CreateOrder(dto, "u1");

            Assert.Equal(12, order.couponId);
            Assert.Equal(90, order.TotalPrice);
        }

        [Fact]
        public async Task CreateOrder_WhenCustomerIsMissing_ThrowsNotFoundException()
        {
            _users.Setup(r => r.GetByIdAsync("u1")).ReturnsAsync((AppUser?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(
                () => _sut.CreateOrder(CreateDto(new CreateOrderItemDto { MenuItemId = 1, Quantity = 1 }), "u1"));

            Assert.Equal("Customer not found", ex.Message);
        }

        [Fact]
        public async Task CreateOrder_WhenItemsAreMissing_ThrowsBadRequestException()
        {
            _users.Setup(r => r.GetByIdAsync("u1")).ReturnsAsync(new AppUser { Id = "u1" });

            var ex = await Assert.ThrowsAsync<BadRequestException>(
                () => _sut.CreateOrder(new CreateOrderDto { itemDtos = new List<CreateOrderItemDto>() }, "u1"));

            Assert.Equal("Order must contain at least one item.", ex.Message);
        }

        [Fact]
        public async Task CreateOrder_WhenItemsCollectionIsNull_ThrowsBadRequestException()
        {
            _users.Setup(r => r.GetByIdAsync("u1")).ReturnsAsync(new AppUser { Id = "u1" });

            var ex = await Assert.ThrowsAsync<BadRequestException>(
                () => _sut.CreateOrder(new CreateOrderDto { itemDtos = null! }, "u1"));

            Assert.Equal("Order must contain at least one item.", ex.Message);
        }

        [Fact]
        public async Task CreateOrder_WhenMenuItemIsMissing_ThrowsNotFoundException()
        {
            var dto = CreateDto(new CreateOrderItemDto { MenuItemId = 8, Quantity = 1 });
            _users.Setup(r => r.GetByIdAsync("u1")).ReturnsAsync(new AppUser { Id = "u1" });
            _mapper.Setup(m => m.Map<Orders>(dto)).Returns(new Orders());
            _menuItems.Setup(r => r.GetByIdAsync(8)).ReturnsAsync((MenuItems?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _sut.CreateOrder(dto, "u1"));

            Assert.Equal("Menu item with id 8 not found.", ex.Message);
        }

        [Fact]
        public async Task CreateOrder_WhenMenuItemIsUnavailable_ThrowsBadRequestException()
        {
            var dto = CreateDto(new CreateOrderItemDto { MenuItemId = 8, Quantity = 1 });
            _users.Setup(r => r.GetByIdAsync("u1")).ReturnsAsync(new AppUser { Id = "u1" });
            _mapper.Setup(m => m.Map<Orders>(dto)).Returns(new Orders());
            _menuItems.Setup(r => r.GetByIdAsync(8))
                .ReturnsAsync(new MenuItems { id = 8, name = "Burger", isAvailable = false });

            var ex = await Assert.ThrowsAsync<BadRequestException>(() => _sut.CreateOrder(dto, "u1"));

            Assert.Equal("Menu item Burger is not available.", ex.Message);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-2)]
        public async Task CreateOrder_WhenQuantityIsNotPositive_ThrowsBadRequestException(int quantity)
        {
            var dto = CreateDto(new CreateOrderItemDto { MenuItemId = 8, Quantity = quantity });
            _users.Setup(r => r.GetByIdAsync("u1")).ReturnsAsync(new AppUser { Id = "u1" });
            _mapper.Setup(m => m.Map<Orders>(dto)).Returns(new Orders());
            _menuItems.Setup(r => r.GetByIdAsync(8))
                .ReturnsAsync(new MenuItems { id = 8, name = "Burger", price = 10, isAvailable = true });

            var ex = await Assert.ThrowsAsync<BadRequestException>(() => _sut.CreateOrder(dto, "u1"));

            Assert.Equal("Quantity must be greater than zero.", ex.Message);
        }

        [Fact]
        public async Task GetMyOrderDetails_WhenOwnerRequestsOrder_ReturnsMappedDto()
        {
            var order = new Orders { id = 5, customerId = "u1" };
            var dto = new OrderDetailsDto { OrderId = 5 };
            _orders.Setup(r => r.GetOrderWithDetails(5)).ReturnsAsync(order);
            _users.Setup(r => r.GetByIdAsync("u1")).ReturnsAsync(new AppUser { Id = "u1" });
            _mapper.Setup(m => m.Map<OrderDetailsDto>(order)).Returns(dto);

            var result = await _sut.GetMyOrderDetails(5, "u1");

            Assert.Same(dto, result);
        }

        [Fact]
        public async Task GetMyOrderDetails_WhenOrderIsMissing_ThrowsNotFoundException()
        {
            _orders.Setup(r => r.GetOrderWithDetails(5)).ReturnsAsync((Orders)null!);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetMyOrderDetails(5, "u1"));

            Assert.Equal("Order Not Found", ex.Message);
        }

        [Fact]
        public async Task GetMyOrderDetails_WhenCustomerIsMissing_ThrowsNotFoundException()
        {
            _orders.Setup(r => r.GetOrderWithDetails(5)).ReturnsAsync(new Orders { customerId = "u1" });
            _users.Setup(r => r.GetByIdAsync("u1")).ReturnsAsync((AppUser?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetMyOrderDetails(5, "u1"));

            Assert.Equal("Customer Not Found", ex.Message);
        }

        [Fact]
        public async Task GetMyOrderDetails_WhenCustomerDoesNotOwnOrder_ThrowsUnauthorizedException()
        {
            _orders.Setup(r => r.GetOrderWithDetails(5)).ReturnsAsync(new Orders { customerId = "other" });
            _users.Setup(r => r.GetByIdAsync("u1")).ReturnsAsync(new AppUser { Id = "u1" });

            var ex = await Assert.ThrowsAsync<UnauthorizedException>(() => _sut.GetMyOrderDetails(5, "u1"));

            Assert.Equal("You are not authorized to get this order.", ex.Message);
        }

        [Fact]
        public async Task GetMyOrders_WhenCustomerExists_UsesCustomerQuery()
        {
            var query = new List<Orders>().AsQueryable();
            _users.Setup(r => r.GetByIdAsync("u1")).ReturnsAsync(new AppUser { Id = "u1" });
            _orders.Setup(r => r.GetOrdersByCustomerId("u1")).Returns(query);
            _orders.Setup(r => r.GetAllPaged(1, 10, query))
                .ReturnsAsync((Enumerable.Empty<Orders>(), 0));
            _mapper.Setup(m => m.Map<List<OrderSummaryDto>>(It.IsAny<IEnumerable<Orders>>()))
                .Returns(new List<OrderSummaryDto>());

            var result = await _sut.GetMyOrders("u1", 1, 10);

            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetMyOrders_WhenCustomerIsMissing_ThrowsNotFoundException()
        {
            _users.Setup(r => r.GetByIdAsync("u1")).ReturnsAsync((AppUser?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetMyOrders("u1", 1, 10));

            Assert.Equal("Customer Not Found", ex.Message);
        }

        [Fact]
        public async Task CancelOrder_WhenPendingOwnedOrder_SetsCancelled()
        {
            var order = new Orders { id = 5, customerId = "u1", Status = OrderStatus.Pending };
            _orders.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(order);
            _users.Setup(r => r.GetByIdAsync("u1")).ReturnsAsync(new AppUser { Id = "u1" });

            await _sut.CancelOrder(5, "u1");

            Assert.Equal(OrderStatus.Cancelled, order.Status);
            _cache.Verify(c => c.RemoveAsync("Get_Customer_Orders_u1"), Times.Once);
        }

        [Fact]
        public async Task CancelOrder_WhenOrderIsMissing_ThrowsNotFoundException()
        {
            _orders.Setup(r => r.GetByIdAsync(5)).ReturnsAsync((Orders?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _sut.CancelOrder(5, "u1"));

            Assert.Equal("Order Not Found", ex.Message);
        }

        [Fact]
        public async Task CancelOrder_WhenCustomerIsMissing_ThrowsNotFoundException()
        {
            _orders.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(new Orders { customerId = "u1" });
            _users.Setup(r => r.GetByIdAsync("u1")).ReturnsAsync((AppUser?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _sut.CancelOrder(5, "u1"));

            Assert.Equal("Customer Not Found", ex.Message);
        }

        [Fact]
        public async Task CancelOrder_WhenCustomerDoesNotOwnOrder_ThrowsUnauthorizedException()
        {
            _orders.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(new Orders { customerId = "other", Status = OrderStatus.Pending });
            _users.Setup(r => r.GetByIdAsync("u1")).ReturnsAsync(new AppUser { Id = "u1" });

            var ex = await Assert.ThrowsAsync<UnauthorizedException>(() => _sut.CancelOrder(5, "u1"));

            Assert.Equal("You are not authorized to cancel this order.", ex.Message);
        }

        [Fact]
        public async Task CancelOrder_WhenAlreadyCancelled_ThrowsBadRequestException()
        {
            _orders.Setup(r => r.GetByIdAsync(5))
                .ReturnsAsync(new Orders { customerId = "u1", Status = OrderStatus.Cancelled });
            _users.Setup(r => r.GetByIdAsync("u1")).ReturnsAsync(new AppUser { Id = "u1" });

            var ex = await Assert.ThrowsAsync<BadRequestException>(() => _sut.CancelOrder(5, "u1"));

            Assert.Equal("Order is already cancelled.", ex.Message);
        }

        [Theory]
        [InlineData(OrderStatus.InProgress)]
        [InlineData(OrderStatus.Completed)]
        public async Task CancelOrder_WhenStatusIsNotPending_ThrowsBadRequestException(OrderStatus status)
        {
            _orders.Setup(r => r.GetByIdAsync(5))
                .ReturnsAsync(new Orders { customerId = "u1", Status = status });
            _users.Setup(r => r.GetByIdAsync("u1")).ReturnsAsync(new AppUser { Id = "u1" });

            var ex = await Assert.ThrowsAsync<BadRequestException>(() => _sut.CancelOrder(5, "u1"));

            Assert.Equal("Order cannot be cancelled at this stage.", ex.Message);
        }
    }
}
