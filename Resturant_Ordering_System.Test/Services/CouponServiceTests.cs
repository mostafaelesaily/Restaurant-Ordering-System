using Application.DTOs.CouponDTOs;
using Application.Services;
using AutoMapper;
using Business_Layer.DTOs.PaginatedDtos;
using Business_Layer.Exceptions;
using Business_Layer.Interfaces;
using Domain_Layer.Abstract;
using Domain_Layer.Entities;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Moq;
using Resturant_Ordering_System.Test.Helpers;
using System.Linq.Expressions;

namespace Resturant_Ordering_System.Test.Services
{
    public class CouponServiceTests
    {
        private readonly Mock<IUow> _uow = new();
        private readonly Mock<ICouponRepo> _coupons = new();
        private readonly Mock<ICacheService> _cache = new();
        private readonly Mock<ILogger<CouponService>> _logger = new();
        private readonly Mock<IMapper> _mapper = new();
        private readonly Mock<IDbContextTransaction> _transaction;
        private readonly CouponService _sut;

        public CouponServiceTests()
        {
            _transaction = TestMockHelpers.CreateTransaction();
            _uow.Setup(u => u.couponRepo).Returns(_coupons.Object);
            _uow.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(_transaction.Object);
            _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
            TestMockHelpers.SetupCacheToExecuteFactory<PaginatedResultDto<GetCouponDto>>(_cache);
            _sut = new CouponService(_uow.Object, _cache.Object, _logger.Object, _mapper.Object);
        }

        private static Coupon CreateCoupon(
            int id = 1,
            string code = "SAVE10",
            bool isActive = true,
            DateTime? expireDate = null) =>
            new()
            {
                Id = id,
                Code = code,
                Discount = 10,
                IsActive = isActive,
                ExpireDate = expireDate ?? DateTime.UtcNow.AddDays(7)
            };

        [Fact]
        public async Task GetAllCopounsPagged_WhenCalled_ReturnsMappedPage()
        {
            var coupon = CreateCoupon();
            var dto = new GetCouponDto { Id = 1, Code = "SAVE10" };
            _coupons.Setup(r => r.GetAllPaged(1, 10)).ReturnsAsync((new List<Coupon> { coupon }.AsEnumerable(), 1));
            _mapper.Setup(m => m.Map<List<GetCouponDto>>(It.IsAny<IEnumerable<Coupon>>()))
                .Returns(new List<GetCouponDto> { dto });

            var result = await _sut.GetAllCopounsPagged(1, 10);

            Assert.Single(result.Data);
            Assert.Equal(1, result.TotalCount);
            Assert.Equal(1, result.PageNumber);
            Assert.Equal(10, result.PageSize);
        }

        [Fact]
        public async Task GetCopounById_WhenCouponExists_ReturnsMappedDto()
        {
            var coupon = CreateCoupon();
            var dto = new GetCouponDto { Id = 1, Code = "SAVE10" };
            _coupons.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(coupon);
            _mapper.Setup(m => m.Map<GetCouponDto>(coupon)).Returns(dto);

            var result = await _sut.GetCopounById(1);

            Assert.Same(dto, result);
        }

        [Fact]
        public async Task GetCopounById_WhenCouponIsMissing_ThrowsNotFoundException()
        {
            _coupons.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Coupon?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetCopounById(99));

            Assert.Equal("Coupon not found", ex.Message);
        }

        [Fact]
        public async Task CreateCopoun_WhenValid_CreatesCommitsAndClearsCache()
        {
            var dto = new CreateCouponDto { Code = "SAVE10", Discount = 10 };
            var coupon = CreateCoupon();
            var resultDto = new GetCouponDto { Id = 1, Code = "SAVE10" };
            _mapper.Setup(m => m.Map<Coupon>(dto)).Returns(coupon);
            _mapper.Setup(m => m.Map<GetCouponDto>(coupon)).Returns(resultDto);

            var result = await _sut.CreateCopoun(dto);

            Assert.Same(resultDto, result);
            _coupons.Verify(r => r.CreateAsync(coupon), Times.Once);
            _uow.Verify(u => u.SaveChangesAsync(), Times.Once);
            _transaction.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
            _cache.Verify(c => c.RemoveAsync("Get_Coupons"), Times.Once);
        }

        [Fact]
        public async Task CreateCopoun_WhenRepositoryThrows_RollsBackAndRethrows()
        {
            var dto = new CreateCouponDto { Code = "SAVE10", Discount = 10 };
            _mapper.Setup(m => m.Map<Coupon>(dto)).Returns(CreateCoupon());
            _coupons.Setup(r => r.CreateAsync(It.IsAny<Coupon>())).ThrowsAsync(new InvalidOperationException("db"));

            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.CreateCopoun(dto));

            _transaction.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
            _cache.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task UpdateCopoun_WhenCouponExists_MapsSavesAndClearsCache()
        {
            var coupon = CreateCoupon();
            var dto = new UpdateCouponDto { Code = "SAVE20", Discount = 20, IsActive = true };
            _coupons.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(coupon);
            _mapper.Setup(m => m.Map(dto, coupon)).Returns(coupon);

            await _sut.UpdateCopoun(1, dto);

            _uow.Verify(u => u.SaveChangesAsync(), Times.Once);
            _transaction.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
            _cache.Verify(c => c.RemoveAsync("Get_Coupons"), Times.Once);
        }

        [Fact]
        public async Task UpdateCopoun_WhenCouponIsMissing_ThrowsNotFoundExceptionAndRollsBack()
        {
            _coupons.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Coupon?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(
                () => _sut.UpdateCopoun(1, new UpdateCouponDto()));

            Assert.Equal("Coupon not found", ex.Message);
            _transaction.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteCopoun_WhenCouponExists_DeletesAndClearsCache()
        {
            var coupon = CreateCoupon();
            _coupons.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(coupon);

            await _sut.DeleteCopoun(1);

            _coupons.Verify(r => r.DeleteAsync(coupon), Times.Once);
            _transaction.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
            _cache.Verify(c => c.RemoveAsync("Get_Coupons"), Times.Once);
        }

        [Fact]
        public async Task DeleteCopoun_WhenCouponIsMissing_ThrowsNotFoundException()
        {
            _coupons.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Coupon?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _sut.DeleteCopoun(1));

            Assert.Equal("Coupon not found", ex.Message);
            _coupons.Verify(r => r.DeleteAsync(It.IsAny<Coupon>()), Times.Never);
        }

        [Fact]
        public async Task ValidateCoupon_WhenCouponIsValid_ReturnsMappedDto()
        {
            var coupon = CreateCoupon();
            var dto = new GetCouponDto { Id = 1, Code = coupon.Code, IsActive = true, Discount = 10 };
            _coupons.Setup(r => r.FindElementAsync(It.IsAny<Expression<Func<Coupon, bool>>>()))
                .ReturnsAsync(coupon);
            _mapper.Setup(m => m.Map<GetCouponDto>(coupon)).Returns(dto);

            var result = await _sut.ValidateCoupon("SAVE10");

            Assert.Same(dto, result);
        }

        [Fact]
        public async Task ValidateCoupon_WhenCouponIsMissing_ThrowsNotFoundException()
        {
            _coupons.Setup(r => r.FindElementAsync(It.IsAny<Expression<Func<Coupon, bool>>>()))
                .ReturnsAsync((Coupon?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _sut.ValidateCoupon("NONE"));

            Assert.Equal("Coupon not found", ex.Message);
        }

        [Fact]
        public async Task ValidateCoupon_WhenCouponIsInactive_ThrowsBadRequestException()
        {
            _coupons.Setup(r => r.FindElementAsync(It.IsAny<Expression<Func<Coupon, bool>>>()))
                .ReturnsAsync(CreateCoupon(isActive: false));

            var ex = await Assert.ThrowsAsync<BadRequestException>(() => _sut.ValidateCoupon("SAVE10"));

            Assert.Equal("Coupon is inactive.", ex.Message);
        }

        [Fact]
        public async Task ValidateCoupon_WhenCouponIsExpired_ThrowsBadRequestException()
        {
            _coupons.Setup(r => r.FindElementAsync(It.IsAny<Expression<Func<Coupon, bool>>>()))
                .ReturnsAsync(CreateCoupon(expireDate: DateTime.UtcNow.AddMinutes(-1)));

            var ex = await Assert.ThrowsAsync<BadRequestException>(() => _sut.ValidateCoupon("SAVE10"));

            Assert.Equal("Coupon has expired.", ex.Message);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("SAVE")]
        public async Task SearchCoupons_WhenCalled_UsesSearchQueryAndReturnsPage(string? search)
        {
            var query = new List<Coupon>().AsQueryable();
            _coupons.Setup(r => r.SearchCoupons(It.IsAny<string>())).Returns(query);
            _coupons.Setup(r => r.GetAllPaged(1, 10, query))
                .ReturnsAsync((Enumerable.Empty<Coupon>(), 0));
            _mapper.Setup(m => m.Map<List<GetCouponDto>>(It.IsAny<IEnumerable<Coupon>>()))
                .Returns(new List<GetCouponDto>());

            var result = await _sut.SearchCoupons(search, 1, 10);

            Assert.Empty(result.Data);
            Assert.Equal(0, result.TotalCount);
            _coupons.Verify(r => r.SearchCoupons(It.IsAny<string>()), Times.Once);
        }
    }
}
