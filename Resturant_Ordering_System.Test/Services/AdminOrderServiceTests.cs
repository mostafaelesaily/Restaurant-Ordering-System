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
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using Resturant_Ordering_System.Application.DTOs.OrderDTOs;
using Resturant_Ordering_System.Application.Interfaces.IService;
using Resturant_Ordering_System.Domain.Abstract;
using Resturant_Ordering_System.Test.Helpers;

namespace Resturant_Ordering_System.Test.Services
{
    public class AdminOrderServiceTests
    {
        private readonly Mock<IUow> _uow = new();
        private readonly Mock<IOrderRepo> _orders = new();
        private readonly Mock<IGenaricRepo<AppUser, string>> _users = new();
        private readonly Mock<IMenuItemRepo> _menuItems = new();
        private readonly Mock<ICacheService> _cache = new();
        private readonly Mock<IMapper> _mapper = new();
        private readonly Mock<ILogger<AdminOrderService>> _logger = new();
        private readonly Mock<INotificationService> _notifications = new();
        private readonly Mock<ISendNotificationService> _sendNotifications = new();
        private readonly Mock<ICouponService> _coupons = new();
        private readonly Mock<UserManager<AppUser>> _userManager;
        private readonly AdminOrderService _sut;

        public AdminOrderServiceTests()
        {
            _userManager = TestMockHelpers.CreateUserManager();
            _uow.Setup(u => u.Orders).Returns(_orders.Object);
            _uow.Setup(u => u.AppUserRepo).Returns(_users.Object);
            _uow.Setup(u => u.MenuItems).Returns(_menuItems.Object);
            _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
            TestMockHelpers.SetupCacheToExecuteFactory<PaginatedResultDto<OrderDetailsDto>>(_cache);
            _sut = new AdminOrderService(
                _notifications.Object,
                _logger.Object,
                _uow.Object,
                _cache.Object,
                _mapper.Object,
                _sendNotifications.Object,
                _coupons.Object,
                _userManager.Object);
        }

        [Fact]
        public async Task GetAllOrders_WhenCalled_UsesDetailsQuery()
        {
            var query = new List<Orders>().AsQueryable();
            _orders.Setup(r => r.GetOrdersWithDetails()).Returns(query);
            _orders.Setup(r => r.GetAllPaged(1, 10, query))
                .ReturnsAsync((Enumerable.Empty<Orders>(), 0));
            _mapper.Setup(m => m.Map<List<OrderDetailsDto>>(It.IsAny<IEnumerable<Orders>>()))
                .Returns(new List<OrderDetailsDto>());

            var result = await _sut.GetAllOrders(1, 10);

            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task SearchOrders_WhenCalled_UsesSearchQuery()
        {
            var query = new List<Orders>().AsQueryable();
            _orders.Setup(r => r.SearchOrder("john")).Returns(query);
            _orders.Setup(r => r.GetAllPaged(1, 10, query))
                .ReturnsAsync((Enumerable.Empty<Orders>(), 0));
            _mapper.Setup(m => m.Map<List<OrderDetailsDto>>(It.IsAny<IEnumerable<Orders>>()))
                .Returns(new List<OrderDetailsDto>());

            await _sut.SearchOrders("john", 1, 10);

            _orders.Verify(r => r.SearchOrder("john"), Times.Once);
        }

        [Fact]
        public async Task GetOrderDetailsById_WhenOrderExists_ReturnsMappedDto()
        {
            var order = new Orders { id = 5 };
            var dto = new OrderDetailsDto { OrderId = 5 };
            _orders.Setup(r => r.GetOrderWithDetails(5)).ReturnsAsync(order);
            _mapper.Setup(m => m.Map<OrderDetailsDto>(order)).Returns(dto);

            var result = await _sut.GetOrderDetailsById(5);

            Assert.Same(dto, result);
        }

        [Fact]
        public async Task GetOrderDetailsById_WhenOrderIsMissing_ThrowsNotFoundException()
        {
            _orders.Setup(r => r.GetOrderWithDetails(5)).ReturnsAsync((Orders)null!);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetOrderDetailsById(5));

            Assert.Equal("Order not found", ex.Message);
        }

        [Fact]
        public async Task AssignChef_WhenChefHasRole_AssignsAndNotifies()
        {
            var order = new Orders { id = 5 };
            var chef = new AppUser { Id = "chef-1" };
            _orders.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(order);
            _users.Setup(r => r.GetByIdAsync("chef-1")).ReturnsAsync(chef);
            _userManager.Setup(m => m.IsInRoleAsync(chef, "Cheif")).ReturnsAsync(true);

            await _sut.AssignChef(5, "chef-1");

            Assert.Equal("chef-1", order.CheifId);
            Assert.NotNull(order.UpdatedAt);
            _notifications.Verify(n => n.CreateAsync(It.IsAny<CreateNotificationDto>()), Times.Once);
            _sendNotifications.Verify(s => s.SendToUserAsync("chef-1", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task AssignChef_WhenOrderIsMissing_ThrowsNotFoundException()
        {
            _orders.Setup(r => r.GetByIdAsync(5)).ReturnsAsync((Orders?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _sut.AssignChef(5, "chef-1"));

            Assert.Equal("Order not found", ex.Message);
        }

        [Fact]
        public async Task AssignChef_WhenChefIsMissing_ThrowsNotFoundException()
        {
            _orders.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(new Orders());
            _users.Setup(r => r.GetByIdAsync("chef-1")).ReturnsAsync((AppUser?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _sut.AssignChef(5, "chef-1"));

            Assert.Equal("Chef not found", ex.Message);
        }

        [Fact]
        public async Task AssignChef_WhenUserIsNotChef_ThrowsBadRequestException()
        {
            var chef = new AppUser { Id = "chef-1" };
            _orders.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(new Orders());
            _users.Setup(r => r.GetByIdAsync("chef-1")).ReturnsAsync(chef);
            _userManager.Setup(m => m.IsInRoleAsync(chef, "Cheif")).ReturnsAsync(false);

            var ex = await Assert.ThrowsAsync<BadRequestException>(() => _sut.AssignChef(5, "chef-1"));

            Assert.Equal("Selected User is not a cheif !", ex.Message);
        }

        [Fact]
        public async Task AssignDelivery_WhenDeliveryHasRole_AssignsAndNotifies()
        {
            var order = new Orders { id = 5 };
            var delivery = new AppUser { Id = "d1" };
            _orders.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(order);
            _users.Setup(r => r.GetByIdAsync("d1")).ReturnsAsync(delivery);
            _userManager.Setup(m => m.IsInRoleAsync(delivery, "Delivery")).ReturnsAsync(true);

            await _sut.AssignDelivery(5, "d1");

            Assert.Equal("d1", order.DeliveryId);
            _sendNotifications.Verify(s => s.SendToUserAsync("d1", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task AssignDelivery_WhenUserIsNotDelivery_ThrowsBadRequestException()
        {
            var delivery = new AppUser { Id = "d1" };
            _orders.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(new Orders());
            _users.Setup(r => r.GetByIdAsync("d1")).ReturnsAsync(delivery);
            _userManager.Setup(m => m.IsInRoleAsync(delivery, "Delivery")).ReturnsAsync(false);

            var ex = await Assert.ThrowsAsync<BadRequestException>(() => _sut.AssignDelivery(5, "d1"));

            Assert.Equal("Selected User is not a Delivery !", ex.Message);
        }

        [Fact]
        public async Task AssignDelivery_WhenOrderIsMissing_ThrowsNotFoundException()
        {
            _orders.Setup(r => r.GetByIdAsync(5)).ReturnsAsync((Orders?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _sut.AssignDelivery(5, "d1"));

            Assert.Equal("Order not found", ex.Message);
        }

        [Fact]
        public async Task CreateOrderByAdmin_WhenValid_CreatesOrderForCustomer()
        {
            var dto = new CreateOrderByAdminDto
            {
                CustomerId = "u1",
                itemDtos = new List<CreateOrderItemDto> { new() { MenuItemId = 8, Quantity = 1 } }
            };
            var order = new Orders();
            _users.Setup(r => r.GetByIdAsync("u1")).ReturnsAsync(new AppUser { Id = "u1" });
            _mapper.Setup(m => m.Map<Orders>(dto)).Returns(order);
            _menuItems.Setup(r => r.GetByIdAsync(8))
                .ReturnsAsync(new MenuItems { id = 8, name = "Burger", price = 40, isAvailable = true });
            _mapper.Setup(m => m.Map<OrderSummaryDto>(order)).Returns(new OrderSummaryDto { totalPrice = 40 });

            var result = await _sut.CreateOrderByAdmin(dto);

            Assert.Equal(40, order.TotalPrice);
            Assert.Equal("u1", order.customerId);
            _orders.Verify(r => r.CreateAsync(order), Times.Once);
            Assert.Equal(40, result.totalPrice);
        }

        [Fact]
        public async Task CreateOrderByAdmin_WhenCustomerIsMissing_ThrowsNotFoundException()
        {
            _users.Setup(r => r.GetByIdAsync("u1")).ReturnsAsync((AppUser?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(
                () => _sut.CreateOrderByAdmin(new CreateOrderByAdminDto { CustomerId = "u1" }));

            Assert.Equal("Customer not found", ex.Message);
        }

        [Fact]
        public async Task CreateOrderByAdmin_WhenItemsAreMissing_ThrowsBadRequestException()
        {
            _users.Setup(r => r.GetByIdAsync("u1")).ReturnsAsync(new AppUser { Id = "u1" });

            var ex = await Assert.ThrowsAsync<BadRequestException>(
                () => _sut.CreateOrderByAdmin(new CreateOrderByAdminDto { CustomerId = "u1" }));

            Assert.Equal("Order must contain at least one item.", ex.Message);
        }

        [Fact]
        public async Task CreateOrderByAdmin_WhenCouponIsApplied_ReducesTotal()
        {
            var dto = new CreateOrderByAdminDto
            {
                CustomerId = "u1",
                CouponCode = "SAVE50",
                itemDtos = new List<CreateOrderItemDto> { new() { MenuItemId = 8, Quantity = 1 } }
            };
            var order = new Orders();
            _users.Setup(r => r.GetByIdAsync("u1")).ReturnsAsync(new AppUser { Id = "u1" });
            _mapper.Setup(m => m.Map<Orders>(dto)).Returns(order);
            _menuItems.Setup(r => r.GetByIdAsync(8))
                .ReturnsAsync(new MenuItems { id = 8, name = "Burger", price = 100, isAvailable = true });
            _coupons.Setup(c => c.ValidateCoupon("SAVE50"))
                .ReturnsAsync(new GetCouponDto { Id = 2, Discount = 50 });
            _mapper.Setup(m => m.Map<OrderSummaryDto>(order)).Returns(new OrderSummaryDto());

            await _sut.CreateOrderByAdmin(dto);

            Assert.Equal(50, order.TotalPrice);
            Assert.Equal(2, order.couponId);
        }

        [Fact]
        public async Task DeleteOrder_WhenOrderExists_DeletesAndNotifiesCustomer()
        {
            var order = new Orders { id = 5, customerId = "u1" };
            _orders.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(order);

            await _sut.DeleteOrder(5);

            _orders.Verify(r => r.DeleteAsync(order), Times.Once);
            _notifications.Verify(n => n.CreateAsync(It.IsAny<CreateNotificationDto>()), Times.Once);
            _sendNotifications.Verify(s => s.SendToUserAsync("u1", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task DeleteOrder_WhenOrderIsMissing_ThrowsNotFoundException()
        {
            _orders.Setup(r => r.GetByIdAsync(5)).ReturnsAsync((Orders?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _sut.DeleteOrder(5));

            Assert.Equal("Order not found", ex.Message);
        }

        [Fact]
        public async Task UpdateOrderStatus_WhenOrderExists_UpdatesStatusAndNotifies()
        {
            var order = new Orders { id = 5, customerId = "u1", Status = OrderStatus.Pending };
            _orders.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(order);

            await _sut.UpdateOrderStatus(5, new UpdateOrderStatusDto { Status = OrderStatus.InProgress });

            Assert.Equal(OrderStatus.InProgress, order.Status);
            Assert.NotNull(order.UpdatedAt);
            _cache.Verify(c => c.RemoveAsync("Get_Orders"), Times.Once);
        }

        [Fact]
        public async Task UpdateOrderStatus_WhenOrderIsMissing_ThrowsNotFoundException()
        {
            _orders.Setup(r => r.GetByIdAsync(5)).ReturnsAsync((Orders?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(
                () => _sut.UpdateOrderStatus(5, new UpdateOrderStatusDto { Status = OrderStatus.Completed }));

            Assert.Equal("Order not found", ex.Message);
        }
    }
}
