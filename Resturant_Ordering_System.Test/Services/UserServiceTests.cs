using AutoMapper;
using Business_Layer.DTOs.UserDTOs;
using Business_Layer.Exceptions;
using Business_Layer.Services;
using Domain_Layer.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using Resturant_Ordering_System.Test.Helpers;

namespace Resturant_Ordering_System.Test.Services
{
    public class UserServiceTests
    {
        private readonly Mock<UserManager<AppUser>> _userManager;
        private readonly Mock<ILogger<UserService>> _logger;
        private readonly Mock<IMapper> _mapper;
        private readonly UserService _sut;

        public UserServiceTests()
        {
            _userManager = TestMockHelpers.CreateUserManager();
            _logger = new Mock<ILogger<UserService>>();
            _mapper = new Mock<IMapper>();
            _sut = new UserService(_userManager.Object, _logger.Object, _mapper.Object);
        }

        private static AppUser CreateUser(string id = "user-1") =>
            new() { Id = id, Email = "user@test.com", UserName = "testuser" };

        [Fact]
        public async Task GetMyProfile_WhenUserExists_ReturnsMappedDto()
        {
            var user = CreateUser();
            var dto = new GetUserDto { Id = user.Id, Email = user.Email!, UserName = user.UserName! };
            _userManager.Setup(m => m.FindByIdAsync(user.Id)).ReturnsAsync(user);
            _mapper.Setup(m => m.Map<GetUserDto>(user)).Returns(dto);

            var result = await _sut.GetMyProfile(user.Id);

            Assert.Same(dto, result);
            _mapper.Verify(m => m.Map<GetUserDto>(user), Times.Once);
        }

        [Fact]
        public async Task GetMyProfile_WhenUserIsMissing_ThrowsNotFoundException()
        {
            _userManager.Setup(m => m.FindByIdAsync("missing")).ReturnsAsync((AppUser?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetMyProfile("missing"));

            Assert.Equal("User Not Found", ex.Message);
            _mapper.Verify(m => m.Map<GetUserDto>(It.IsAny<AppUser>()), Times.Never);
        }

        [Fact]
        public async Task UpdateMyProfile_WhenUserExists_MapsUpdatesAndReturnsDto()
        {
            var user = CreateUser();
            var input = new UpdateUserDto { UserName = "new-name", Email = "new@test.com", PhoneNumber = "010" };
            var output = new UpdateUserDto { UserName = "new-name", Email = "new@test.com", PhoneNumber = "010" };
            _userManager.Setup(m => m.FindByIdAsync(user.Id)).ReturnsAsync(user);
            _mapper.Setup(m => m.Map(input, user)).Returns(user);
            _userManager.Setup(m => m.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);
            _mapper.Setup(m => m.Map<UpdateUserDto>(user)).Returns(output);

            var result = await _sut.UpdateMyProfile(input, user.Id);

            Assert.Same(output, result);
            _userManager.Verify(m => m.UpdateAsync(user), Times.Once);
        }

        [Fact]
        public async Task UpdateMyProfile_WhenUserIsMissing_ThrowsNotFoundException()
        {
            _userManager.Setup(m => m.FindByIdAsync("missing")).ReturnsAsync((AppUser?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(
                () => _sut.UpdateMyProfile(new UpdateUserDto(), "missing"));

            Assert.Equal("User Not Found", ex.Message);
            _userManager.Verify(m => m.UpdateAsync(It.IsAny<AppUser>()), Times.Never);
        }

        [Fact]
        public async Task UpdateMyProfile_WhenIdentityUpdateFails_ThrowsException()
        {
            var user = CreateUser();
            _userManager.Setup(m => m.FindByIdAsync(user.Id)).ReturnsAsync(user);
            _mapper.Setup(m => m.Map(It.IsAny<UpdateUserDto>(), user)).Returns(user);
            _userManager
                .Setup(m => m.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Failed" }));

            var ex = await Assert.ThrowsAsync<Exception>(
                () => _sut.UpdateMyProfile(new UpdateUserDto(), user.Id));

            Assert.Equal("Failed to update user.", ex.Message);
        }

        [Fact]
        public async Task DeleteMyAccount_WhenUserExists_DeletesUser()
        {
            var user = CreateUser();
            _userManager.Setup(m => m.FindByIdAsync(user.Id)).ReturnsAsync(user);
            _userManager.Setup(m => m.DeleteAsync(user)).ReturnsAsync(IdentityResult.Success);

            await _sut.DeleteMyAccount(user.Id);

            _userManager.Verify(m => m.DeleteAsync(user), Times.Once);
        }

        [Fact]
        public async Task DeleteMyAccount_WhenUserIsMissing_ThrowsNotFoundException()
        {
            _userManager.Setup(m => m.FindByIdAsync("missing")).ReturnsAsync((AppUser?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _sut.DeleteMyAccount("missing"));

            Assert.Equal("User Not Found", ex.Message);
            _userManager.Verify(m => m.DeleteAsync(It.IsAny<AppUser>()), Times.Never);
        }

        [Theory]
        [InlineData("DuplicateUserName", "Username is already taken.")]
        [InlineData("ConcurrencyFailure", "Optimistic concurrency failure.")]
        public async Task DeleteMyAccount_WhenIdentityDeleteFails_ThrowsBadRequestException(
            string code,
            string description)
        {
            var user = CreateUser();
            _userManager.Setup(m => m.FindByIdAsync(user.Id)).ReturnsAsync(user);
            _userManager
                .Setup(m => m.DeleteAsync(user))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = code, Description = description }));

            var ex = await Assert.ThrowsAsync<BadRequestException>(() => _sut.DeleteMyAccount(user.Id));

            Assert.Equal(description, ex.Message);
        }
    }
}
