using AutoMapper;
using Business_Layer.DTOs.PaginatedDtos;
using Business_Layer.DTOs.UserDTOs;
using Business_Layer.Exceptions;
using Business_Layer.Interfaces;
using Business_Layer.Services;
using Domain_Layer.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using Resturant_Ordering_System.Application.DTOs.UserDTOs;
using Resturant_Ordering_System.Domain.Enums;
using Resturant_Ordering_System.Test.Helpers;
using System.Linq.Expressions;

namespace Resturant_Ordering_System.Test.Services
{
    public class UserManagementServiceTests
    {
        private readonly Mock<IUow> _uow = new();
        private readonly Mock<IGenaricRepo<AppUser, string>> _users = new();
        private readonly Mock<ICacheService> _cache = new();
        private readonly Mock<IMapper> _mapper = new();
        private readonly Mock<ILogger<UserManagementService>> _logger = new();
        private readonly Mock<UserManager<AppUser>> _userManager;
        private readonly UserManagementService _sut;

        public UserManagementServiceTests()
        {
            _userManager = TestMockHelpers.CreateUserManager();
            _uow.Setup(u => u.AppUserRepo).Returns(_users.Object);
            _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
            TestMockHelpers.SetupCacheToExecuteFactory<PaginatedResultDto<GetUserDto>>(_cache);
            _sut = new UserManagementService(
                _uow.Object,
                _cache.Object,
                _mapper.Object,
                _logger.Object,
                _userManager.Object);
        }

        private static AppUser CreateUser() =>
            new() { Id = "u1", UserName = "john", Email = "john@test.com", PhoneNumber = "010" };

        [Fact]
        public async Task GetUsersPaggedAsync_WhenCalled_ReturnsMappedPage()
        {
            _users.Setup(r => r.GetAllPaged(1, 10))
                .ReturnsAsync((new List<AppUser> { CreateUser() }.AsEnumerable(), 1));
            _mapper.Setup(m => m.Map<List<GetUserDto>>(It.IsAny<IEnumerable<AppUser>>()))
                .Returns(new List<GetUserDto> { new() { Id = "u1" } });

            var result = await _sut.GetUsersPaggedAsync(1, 10);

            Assert.Single(result.Data);
            Assert.Equal(1, result.TotalCount);
        }

        [Fact]
        public async Task GetUserInfo_WhenUserExists_ReturnsMappedDto()
        {
            var user = CreateUser();
            var dto = new GetUserDto { Id = "u1", UserName = "john" };
            _users.Setup(r => r.FindElementAsync(It.IsAny<Expression<Func<AppUser, bool>>>()))
                .ReturnsAsync(user);
            _mapper.Setup(m => m.Map<GetUserDto>(user)).Returns(dto);

            var result = await _sut.GetUserInfo("john");

            Assert.Same(dto, result);
        }

        [Fact]
        public async Task GetUserInfo_WhenUserIsMissing_ThrowsNotFoundException()
        {
            _users.Setup(r => r.FindElementAsync(It.IsAny<Expression<Func<AppUser, bool>>>()))
                .ReturnsAsync((AppUser?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetUserInfo("missing"));

            Assert.Equal("user Not Found", ex.Message);
        }

        [Fact]
        public async Task UpdateUserAsync_WhenUserExists_SavesAndClearsCache()
        {
            var user = CreateUser();
            var dto = new UpdateUserDto { UserName = "new", Email = "n@test.com", PhoneNumber = "011" };
            _users.Setup(r => r.FindElementAsync(It.IsAny<Expression<Func<AppUser, bool>>>()))
                .ReturnsAsync(user);
            _mapper.Setup(m => m.Map(dto, user)).Returns(user);
            _mapper.Setup(m => m.Map<UpdateUserDto>(user)).Returns(dto);

            var result = await _sut.updateUserAsync(dto, "u1");

            Assert.Same(dto, result);
            _uow.Verify(u => u.SaveChangesAsync(), Times.Once);
            _cache.Verify(c => c.RemoveAsync("Get_Users"), Times.Once);
            _cache.Verify(c => c.RemoveAsync("Get_User"), Times.Once);
        }

        [Fact]
        public async Task UpdateUserAsync_WhenUserIsMissing_ThrowsNotFoundException()
        {
            _users.Setup(r => r.FindElementAsync(It.IsAny<Expression<Func<AppUser, bool>>>()))
                .ReturnsAsync((AppUser?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(
                () => _sut.updateUserAsync(new UpdateUserDto(), "missing"));

            Assert.Equal("user Not Found", ex.Message);
        }

        [Fact]
        public async Task BanUser_WhenUserExists_LocksAccountAndReturnsTrue()
        {
            var user = CreateUser();
            _users.Setup(r => r.FindElementAsync(It.IsAny<Expression<Func<AppUser, bool>>>()))
                .ReturnsAsync(user);
            _userManager.Setup(m => m.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue))
                .ReturnsAsync(IdentityResult.Success);

            var result = await _sut.BanUser("u1");

            Assert.True(result);
            _cache.Verify(c => c.RemoveAsync("Get_Users"), Times.Once);
        }

        [Fact]
        public async Task BanUser_WhenUserIsMissing_ThrowsNotFoundException()
        {
            _users.Setup(r => r.FindElementAsync(It.IsAny<Expression<Func<AppUser, bool>>>()))
                .ReturnsAsync((AppUser?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _sut.BanUser("missing"));

            Assert.Equal("User Not Found", ex.Message);
        }

        [Fact]
        public async Task BanUser_WhenLockoutFails_ThrowsBadRequestException()
        {
            var user = CreateUser();
            _users.Setup(r => r.FindElementAsync(It.IsAny<Expression<Func<AppUser, bool>>>()))
                .ReturnsAsync(user);
            _userManager.Setup(m => m.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "lockout failed" }));

            var ex = await Assert.ThrowsAsync<BadRequestException>(() => _sut.BanUser("u1"));

            Assert.Equal("lockout failed", ex.Message);
        }

        [Fact]
        public async Task UnBanUser_WhenUserExists_ClearsLockoutAndReturnsTrue()
        {
            var user = CreateUser();
            _users.Setup(r => r.FindElementAsync(It.IsAny<Expression<Func<AppUser, bool>>>()))
                .ReturnsAsync(user);
            _userManager
                .Setup(m => m.SetLockoutEndDateAsync(user, It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(IdentityResult.Success);

            var result = await _sut.UnBanUser("u1");

            Assert.True(result);
        }

        [Fact]
        public async Task UnBanUser_WhenUserIsMissing_ThrowsNotFoundException()
        {
            _users.Setup(r => r.FindElementAsync(It.IsAny<Expression<Func<AppUser, bool>>>()))
                .ReturnsAsync((AppUser?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _sut.UnBanUser("missing"));

            Assert.Equal("User Not Found", ex.Message);
        }

        [Fact]
        public async Task UnBanUser_WhenLockoutFails_ThrowsBadRequestException()
        {
            var user = CreateUser();
            _users.Setup(r => r.FindElementAsync(It.IsAny<Expression<Func<AppUser, bool>>>()))
                .ReturnsAsync(user);
            _userManager
                .Setup(m => m.SetLockoutEndDateAsync(user, It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "unlock failed" }));

            var ex = await Assert.ThrowsAsync<BadRequestException>(() => _sut.UnBanUser("u1"));

            Assert.Equal("unlock failed", ex.Message);
        }

        [Fact]
        public async Task DeleteUser_WhenUserExists_DeletesAndClearsCache()
        {
            var user = CreateUser();
            _users.Setup(r => r.FindElementAsync(It.IsAny<Expression<Func<AppUser, bool>>>()))
                .ReturnsAsync(user);
            _userManager.Setup(m => m.DeleteAsync(user)).ReturnsAsync(IdentityResult.Success);

            await _sut.DeleteUser("u1");

            _userManager.Verify(m => m.DeleteAsync(user), Times.Once);
            _cache.Verify(c => c.RemoveAsync("Get_Users"), Times.Once);
        }

        [Fact]
        public async Task DeleteUser_WhenUserIsMissing_ThrowsNotFoundException()
        {
            _users.Setup(r => r.FindElementAsync(It.IsAny<Expression<Func<AppUser, bool>>>()))
                .ReturnsAsync((AppUser?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _sut.DeleteUser("missing"));

            Assert.Equal("User Not Found", ex.Message);
        }

        [Theory]
        [InlineData("Cheif")]
        [InlineData("Delivery")]
        public async Task GetUsersByRoleAsync_WhenCalled_MapsUsersInRole(string role)
        {
            var users = new List<AppUser> { CreateUser() };
            _userManager.Setup(m => m.GetUsersInRoleAsync(role)).ReturnsAsync(users);
            _mapper.Setup(m => m.Map<List<GetUserDto>>(users))
                .Returns(new List<GetUserDto> { new() { Id = "u1" } });

            var result = await _sut.GetUsersByRoleAsync(role);

            Assert.Single(result);
            _userManager.Verify(m => m.GetUsersInRoleAsync(role), Times.Once);
        }

        [Fact]
        public async Task AddEmployee_WhenCreateAndRoleSucceed_ReturnsTemporaryPassword()
        {
            var dto = new EmployeeDto
            {
                UserName = "chef",
                Email = "chef@test.com",
                PhoneNumber = "010",
                Role = EmployeeRole.Cheif
            };
            var user = new AppUser { Id = "e1", UserName = "chef" };
            _mapper.Setup(m => m.Map<AppUser>(dto)).Returns(user);
            _userManager.Setup(m => m.CreateAsync(user, It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
            _userManager.Setup(m => m.AddToRoleAsync(user, "Cheif")).ReturnsAsync(IdentityResult.Success);
            _mapper.Setup(m => m.Map<GetUserDto>(user)).Returns(new GetUserDto { Id = "e1", UserName = "chef" });

            var result = await _sut.AddEmployee(dto);

            Assert.False(string.IsNullOrWhiteSpace(result.TemporaryPassword));
            Assert.Equal(10, result.TemporaryPassword.Length);
            Assert.True(user.MustChangePassword);
            Assert.Equal("e1", result.getUserDto.Id);
        }

        [Fact]
        public async Task AddEmployee_WhenCreateFails_ThrowsException()
        {
            var dto = new EmployeeDto { UserName = "chef", Role = EmployeeRole.Cheif };
            var user = new AppUser();
            _mapper.Setup(m => m.Map<AppUser>(dto)).Returns(user);
            _userManager.Setup(m => m.CreateAsync(user, It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "email taken" }));

            var ex = await Assert.ThrowsAsync<Exception>(() => _sut.AddEmployee(dto));

            Assert.Equal("email taken", ex.Message);
            _userManager.Verify(m => m.AddToRoleAsync(It.IsAny<AppUser>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task AddEmployee_WhenRoleAssignmentFails_DeletesUserAndThrowsException()
        {
            var dto = new EmployeeDto { UserName = "chef", Role = EmployeeRole.Delivery };
            var user = new AppUser { Id = "e1" };
            _mapper.Setup(m => m.Map<AppUser>(dto)).Returns(user);
            _userManager.Setup(m => m.CreateAsync(user, It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
            _userManager.Setup(m => m.AddToRoleAsync(user, "Delivery"))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "role missing" }));
            _userManager.Setup(m => m.DeleteAsync(user)).ReturnsAsync(IdentityResult.Success);

            var ex = await Assert.ThrowsAsync<Exception>(() => _sut.AddEmployee(dto));

            Assert.Equal("role missing", ex.Message);
            _userManager.Verify(m => m.DeleteAsync(user), Times.Once);
        }
    }
}
