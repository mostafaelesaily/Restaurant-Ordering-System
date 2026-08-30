using AutoMapper;
using Business_Layer.DTOs.NotificationDTOs;
using Business_Layer.DTOs.PaginatedDtos;
using Business_Layer.Exceptions;
using Business_Layer.Interfaces;
using Domain_Layer.Entities;
using Domain_Layer.Enums;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using MockQueryable;
using MockQueryable.Moq;
using Moq;
using Resturant_Ordering_System.Application.DTOs.ReservationDtos;
using Resturant_Ordering_System.Application.Interfaces.IService;
using Resturant_Ordering_System.Application.Services;
using Resturant_Ordering_System.Domain.Abstract;
using Resturant_Ordering_System.Test.Helpers;

namespace Resturant_Ordering_System.Test.Services
{
    public class ReservationServiceTests
    {
        private readonly Mock<IUow> _uow = new();
        private readonly Mock<IReservationRepo> _reservations = new();
        private readonly Mock<ITableRepo> _tables = new();
        private readonly Mock<IGenaricRepo<AppUser, string>> _users = new();
        private readonly Mock<ICacheService> _cache = new();
        private readonly Mock<IMapper> _mapper = new();
        private readonly Mock<ILogger<ReservationService>> _logger = new();
        private readonly Mock<INotificationService> _notifications = new();
        private readonly Mock<ISendNotificationService> _sendNotifications = new();
        private readonly Mock<IDbContextTransaction> _transaction;
        private readonly ReservationService _sut;

        public ReservationServiceTests()
        {
            _transaction = TestMockHelpers.CreateTransaction();
            _uow.Setup(u => u.Reservations).Returns(_reservations.Object);
            _uow.Setup(u => u.Tables).Returns(_tables.Object);
            _uow.Setup(u => u.AppUserRepo).Returns(_users.Object);
            _uow.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(_transaction.Object);
            _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
            TestMockHelpers.SetupCacheToExecuteFactory<PaginatedResultDto<ReservationDetailsDto>>(_cache);
            _sut = new ReservationService(
                _uow.Object,
                _cache.Object,
                _mapper.Object,
                _logger.Object,
                _notifications.Object,
                _sendNotifications.Object);
        }

        [Fact]
        public async Task GetAllReservation_WhenCalled_ReturnsMappedPage()
        {
            _reservations.Setup(r => r.GetAllPaged(1, 10))
                .ReturnsAsync((Enumerable.Empty<Reservations>(), 0));
            _mapper.Setup(m => m.Map<List<ReservationDetailsDto>>(It.IsAny<IEnumerable<Reservations>>()))
                .Returns(new List<ReservationDetailsDto>());

            var result = await _sut.GetAllReservation(1, 10);

            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetReservationById_WhenReservationExists_ReturnsMappedDto()
        {
            var reservation = new Reservations { Id = 7 };
            var dto = new ReservationDetailsDto { Id = 7 };
            _reservations.Setup(r => r.GetByIdAsync(7)).ReturnsAsync(reservation);
            _mapper.Setup(m => m.Map<ReservationDetailsDto>(reservation)).Returns(dto);

            var result = await _sut.GetReservationById(7);

            Assert.Same(dto, result);
        }

        [Fact]
        public async Task GetReservationById_WhenReservationIsMissing_ThrowsNotFoundException()
        {
            _reservations.Setup(r => r.GetByIdAsync(7)).ReturnsAsync((Reservations?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetReservationById(7));

            Assert.Equal("Reservation not found", ex.Message);
        }

        [Fact]
        public async Task GetUserReservations_WhenCalled_UsesUserQuery()
        {
            var query = new List<Reservations>().AsQueryable();
            _reservations.Setup(r => r.GetUserReservations("u1")).Returns(query);
            _reservations.Setup(r => r.GetAllPaged(1, 10, query))
                .ReturnsAsync((Enumerable.Empty<Reservations>(), 0));
            _mapper.Setup(m => m.Map<List<ReservationDetailsDto>>(It.IsAny<IEnumerable<Reservations>>()))
                .Returns(new List<ReservationDetailsDto>());

            await _sut.GetUserReservations("u1", 1, 10);

            _reservations.Verify(r => r.GetUserReservations("u1"), Times.Once);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("john")]
        public async Task SearchReservations_WhenCalled_UsesSearchQuery(string? search)
        {
            var query = new List<Reservations>().AsQueryable();
            _reservations.Setup(r => r.SearchReservations(search)).Returns(query);
            _reservations.Setup(r => r.GetAllPaged(1, 10, query))
                .ReturnsAsync((Enumerable.Empty<Reservations>(), 0));
            _mapper.Setup(m => m.Map<List<ReservationDetailsDto>>(It.IsAny<IEnumerable<Reservations>>()))
                .Returns(new List<ReservationDetailsDto>());

            await _sut.SearchReservations(search, 1, 10);

            _reservations.Verify(r => r.SearchReservations(search), Times.Once);
        }

        [Fact]
        public async Task IsReserved_WhenTableIsMissing_ThrowsNotFoundException()
        {
            _tables.Setup(r => r.GetByIdAsync(3)).ReturnsAsync((Tables?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(
                () => _sut.IsReserved(3, DateTime.UtcNow, TimeSpan.FromHours(1)));

            Assert.Equal("Table Not Found!", ex.Message);
        }

        [Fact]
        public async Task IsReserved_WhenSlotOverlapsExistingReservation_ReturnsTrue()
        {
            var start = new DateTime(2026, 8, 30, 18, 0, 0, DateTimeKind.Utc);
            var existing = new Reservations
            {
                ReservationDate = start,
                EndTime = start.AddHours(2)
            };
            _tables.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(new Tables { Id = 3 });
            _reservations.Setup(r => r.GetReservationsByTableId(3))
                .Returns(new List<Reservations> { existing }.BuildMock());

            var result = await _sut.IsReserved(3, start.AddHours(1), TimeSpan.FromHours(1));

            Assert.True(result);
        }

        [Fact]
        public async Task IsReserved_WhenSlotDoesNotOverlap_ReturnsFalse()
        {
            var start = new DateTime(2026, 8, 30, 18, 0, 0, DateTimeKind.Utc);
            var existing = new Reservations
            {
                ReservationDate = start,
                EndTime = start.AddHours(1)
            };
            _tables.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(new Tables { Id = 3 });
            _reservations.Setup(r => r.GetReservationsByTableId(3))
                .Returns(new List<Reservations> { existing }.BuildMock());

            var result = await _sut.IsReserved(3, start.AddHours(1), TimeSpan.FromHours(1));

            Assert.False(result);
        }

        [Fact]
        public async Task CreateReservation_WhenValid_CreatesPendingReservationAndNotifies()
        {
            var start = DateTime.UtcNow.AddDays(1);
            var dto = new CreateReservationDto
            {
                TableId = 3,
                ReservationDate = start,
                Duration = TimeSpan.FromHours(1)
            };
            var reservation = new Reservations
            {
                ReservationDate = start,
                Duration = TimeSpan.FromHours(1)
            };
            _users.Setup(r => r.GetByIdAsync("u1")).ReturnsAsync(new AppUser { Id = "u1", UserName = "john" });
            _tables.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(new Tables { Id = 3 });
            _reservations.Setup(r => r.GetReservationsByTableId(3))
                .Returns(new List<Reservations>().BuildMock());
            _mapper.Setup(m => m.Map<Reservations>(dto)).Returns(reservation);
            _mapper.Setup(m => m.Map<ReservationDetailsDto>(reservation))
                .Returns(new ReservationDetailsDto { TableId = 3 });

            var result = await _sut.CreateReservation(dto, "u1");

            Assert.Equal("u1", reservation.custoemerId);
            Assert.Equal(ReservationStatus.Pending, reservation.Status);
            Assert.Equal(start.AddHours(1), reservation.EndTime);
            _reservations.Verify(r => r.CreateAsync(reservation), Times.Once);
            _notifications.Verify(n => n.CreateAsync(It.IsAny<CreateNotificationDto>()), Times.Once);
            _sendNotifications.Verify(s => s.SendToUserAsync("u1", It.IsAny<string>()), Times.Once);
            Assert.Equal(3, result.TableId);
        }

        [Fact]
        public async Task CreateReservation_WhenCustomerIsMissing_ThrowsNotFoundException()
        {
            _users.Setup(r => r.GetByIdAsync("u1")).ReturnsAsync((AppUser?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(
                () => _sut.CreateReservation(new CreateReservationDto { TableId = 3 }, "u1"));

            Assert.Equal("Customer not found", ex.Message);
        }

        [Fact]
        public async Task CreateReservation_WhenTableIsAlreadyReserved_ThrowsBadRequestException()
        {
            var start = DateTime.UtcNow.AddDays(1);
            var dto = new CreateReservationDto
            {
                TableId = 3,
                ReservationDate = start,
                Duration = TimeSpan.FromHours(1)
            };
            _users.Setup(r => r.GetByIdAsync("u1")).ReturnsAsync(new AppUser { Id = "u1" });
            _tables.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(new Tables { Id = 3 });
            _reservations.Setup(r => r.GetReservationsByTableId(3)).Returns(new List<Reservations>
            {
                new() { ReservationDate = start, EndTime = start.AddHours(2) }
            }.BuildMock());

            var ex = await Assert.ThrowsAsync<BadRequestException>(() => _sut.CreateReservation(dto, "u1"));

            Assert.Equal("Table Is Reserved For This Time", ex.Message);
            _reservations.Verify(r => r.CreateAsync(It.IsAny<Reservations>()), Times.Never);
        }

        [Fact]
        public async Task UpdateReservation_WhenOwnedReservationExists_UpdatesAndNotifies()
        {
            var reservation = new Reservations { Id = 7, custoemerId = "u1", tableId = 3 };
            var dto = new UpdateReservationDto { TableId = 4, NumberOfGuests = 2, ReservationDate = DateTime.UtcNow };
            _reservations.Setup(r => r.GetByIdAsync(7)).ReturnsAsync(reservation);
            _mapper.Setup(m => m.Map(dto, reservation)).Returns(reservation);

            await _sut.UpdateReservation(7, dto, "u1");

            _reservations.Verify(r => r.UpdateAsync(reservation), Times.Once);
            _notifications.Verify(n => n.CreateAsync(It.IsAny<CreateNotificationDto>()), Times.Once);
        }

        [Fact]
        public async Task UpdateReservation_WhenReservationIsMissing_ThrowsNotFoundException()
        {
            _reservations.Setup(r => r.GetByIdAsync(7)).ReturnsAsync((Reservations?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(
                () => _sut.UpdateReservation(7, new UpdateReservationDto(), "u1"));

            Assert.Equal("Reservation not found", ex.Message);
        }

        [Fact]
        public async Task DeleteReservation_WhenOwnedReservationExists_DeletesAndNotifies()
        {
            var reservation = new Reservations { Id = 7, custoemerId = "u1", tableId = 3 };
            _reservations.Setup(r => r.GetByIdAsync(7)).ReturnsAsync(reservation);

            await _sut.DeleteReservation(7, "u1");

            _reservations.Verify(r => r.DeleteAsync(reservation), Times.Once);
            _transaction.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
            _notifications.Verify(n => n.CreateAsync(It.IsAny<CreateNotificationDto>()), Times.Once);
        }

        [Fact]
        public async Task DeleteReservation_WhenReservationBelongsToAnotherCustomer_ThrowsNotFoundException()
        {
            _reservations.Setup(r => r.GetByIdAsync(7))
                .ReturnsAsync(new Reservations { Id = 7, custoemerId = "other" });

            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _sut.DeleteReservation(7, "u1"));

            Assert.Equal("Reservation not found", ex.Message);
            _transaction.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
