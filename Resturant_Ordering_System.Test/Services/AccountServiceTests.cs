using AutoMapper;
using Business_Layer.DTOs.RefreshTokenDtos;
using Business_Layer.DTOs.UserDTOs;
using Business_Layer.Exceptions;
using Business_Layer.Services;
using Resturant_Ordering_System.Application.DTOs.EmailDTOs;
using Resturant_Ordering_System.Application.Interfaces.IService;
using Domain_Layer.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MockQueryable;
using MockQueryable.Moq;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Resturant_Ordering_System.Test.Services
{
    public class AccountServiceTests
    {
        private const string JwtKey = "G7#vL9@xP2!mQ5rT8uW1zY4kN&cH6jF$sA3dE0bR^pX7nM!qK5tV9yC2wL8hJ1fZ";
        private const string JwtIssuer = "https://test-issuer/";
        private const string JwtAudience = "https://test-audience/";

        private readonly Mock<UserManager<AppUser>> _userManager;
        private readonly Mock<IConfiguration> _configuration;
        private readonly Mock<ILogger<AccountService>> _logger;
        private readonly Mock<IMapper> _mapper;
        private readonly Mock<IGmailService> _gmailService;
        private readonly AccountService _sut;

        public AccountServiceTests()
        {
            _userManager = CreateUserManagerMock();
            _configuration = new Mock<IConfiguration>();
            _logger = new Mock<ILogger<AccountService>>();
            _mapper = new Mock<IMapper>();
            _gmailService = new Mock<IGmailService>();

            _configuration.Setup(c => c["JWT:Key"]).Returns(JwtKey);
            _configuration.Setup(c => c["JWT:Issuer"]).Returns(JwtIssuer);
            _configuration.Setup(c => c["JWT:Audience"]).Returns(JwtAudience);

            _userManager
                .Setup(m => m.GetRolesAsync(It.IsAny<AppUser>()))
                .ReturnsAsync(new List<string>());
            _userManager
                .Setup(m => m.UpdateAsync(It.IsAny<AppUser>()))
                .ReturnsAsync(IdentityResult.Success);

            _sut = new AccountService(
                _userManager.Object,
                _configuration.Object,
                _logger.Object,
                _mapper.Object,
                _gmailService.Object);
        }

        private static Mock<UserManager<AppUser>> CreateUserManagerMock()
        {
            var store = new Mock<IUserStore<AppUser>>();
            return new Mock<UserManager<AppUser>>(
                store.Object,
                null!,
                null!,
                null!,
                null!,
                null!,
                null!,
                null!,
                null!);
        }

        private static AppUser CreateUser(
            string id = "user-1",
            string email = "user@test.com",
            string userName = "testuser")
        {
            return new AppUser
            {
                Id = id,
                Email = email,
                UserName = userName
            };
        }

        private static RefreshTokens CreateRefreshToken(
            string token,
            DateTime? expiresOn = null,
            DateTime? revokedOn = null)
        {
            return new RefreshTokens
            {
                Token = token,
                CreatedOn = DateTime.UtcNow.AddDays(-1),
                ExpiresOn = expiresOn ?? DateTime.UtcNow.AddDays(7),
                revokedOn = revokedOn
            };
        }

        private void SetupUsersQueryable(params AppUser[] users)
        {
            _userManager.Setup(m => m.Users).Returns(users.BuildMock());
        }

        private static JwtSecurityToken ReadJwt(string token)
        {
            return new JwtSecurityTokenHandler().ReadJwtToken(token);
        }

        #region GenrateAccessToken

        [Fact]
        public async Task GenrateAccessToken_WhenUserIsValid_ReturnsReadableJwt()
        {
            // Arrange
            var user = CreateUser();

            // Act
            var token = await _sut.GenrateAccessToken(user);

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(token));
            var jwt = ReadJwt(token);
            Assert.Equal(JwtIssuer, jwt.Issuer);
            Assert.Contains(JwtAudience, jwt.Audiences);
            Assert.Contains(jwt.Claims, c => c.Type == ClaimTypes.NameIdentifier && c.Value == user.Id);
            Assert.Contains(jwt.Claims, c => c.Type == ClaimTypes.Email && c.Value == user.Email);
            Assert.Contains(jwt.Claims, c => c.Type == ClaimTypes.Name && c.Value == user.UserName);
            Assert.Contains(jwt.Claims, c => c.Type == JwtRegisteredClaimNames.Jti);
            _userManager.Verify(m => m.GetRolesAsync(user), Times.Once);
        }

        [Theory]
        [InlineData("")]
        [InlineData("Admin")]
        [InlineData("Admin,Chef")]
        public async Task GenrateAccessToken_WhenUserHasRoles_EmbedsRoleClaims(string rolesCsv)
        {
            // Arrange
            var user = CreateUser();
            var roles = string.IsNullOrEmpty(rolesCsv)
                ? new List<string>()
                : rolesCsv.Split(',').ToList();
            _userManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(roles);

            // Act
            var token = await _sut.GenrateAccessToken(user);

            // Assert
            var jwt = ReadJwt(token);
            var roleClaims = jwt.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();
            Assert.Equal(roles, roleClaims);
        }

        [Fact]
        public async Task GenrateAccessToken_WhenUserIsNull_ThrowsNullReferenceException()
        {
            // Arrange / Act / Assert
            await Assert.ThrowsAsync<NullReferenceException>(() => _sut.GenrateAccessToken(null!));
        }

        [Fact]
        public async Task GenrateAccessToken_WhenJwtKeyIsMissing_ThrowsArgumentNullException()
        {
            // Arrange
            _configuration.Setup(c => c["JWT:Key"]).Returns((string?)null);
            var user = CreateUser();

            // Act / Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.GenrateAccessToken(user));
        }

        #endregion

        #region GenrateRefreshToken

        [Fact]
        public async Task GenrateRefreshToken_WhenCalled_ReturnsActiveTokenWithExpectedLifetime()
        {
            // Arrange
            var before = DateTime.UtcNow;

            // Act
            var refreshToken = await _sut.GenrateRefreshToken();

            // Assert
            var after = DateTime.UtcNow;
            Assert.False(string.IsNullOrWhiteSpace(refreshToken.Token));
            Assert.Equal(64, Convert.FromBase64String(refreshToken.Token).Length);
            Assert.Null(refreshToken.revokedOn);
            Assert.True(refreshToken.isActive);
            Assert.InRange(refreshToken.CreatedOn, before.AddSeconds(-1), after.AddSeconds(1));
            Assert.InRange(
                refreshToken.ExpiresOn,
                before.AddDays(7).AddSeconds(-1),
                after.AddDays(7).AddSeconds(1));
        }

        [Fact]
        public async Task GenrateRefreshToken_WhenCalledTwice_ReturnsDifferentTokens()
        {
            // Arrange / Act
            var first = await _sut.GenrateRefreshToken();
            var second = await _sut.GenrateRefreshToken();

            // Assert
            Assert.NotEqual(first.Token, second.Token);
        }

        #endregion

        #region HandleRefreshTokenAsync

        [Fact]
        public async Task HandleRefreshTokenAsync_WhenDtoIsNull_ThrowsArgumentNullException()
        {
            // Arrange / Act
            var ex = await Assert.ThrowsAsync<ArgumentNullException>(
                () => _sut.HandleRefreshTokenAsync(null!));

            // Assert
            Assert.Equal("refreshTokenDto", ex.ParamName);
        }

        [Fact]
        public async Task HandleRefreshTokenAsync_WhenTokenDoesNotMatchAnyUser_ThrowsUnauthorizedException()
        {
            // Arrange
            SetupUsersQueryable(CreateUser());
            var dto = new GenrateRefreshToken { Token = "unknown-token" };

            // Act
            var ex = await Assert.ThrowsAsync<UnauthorizedException>(
                () => _sut.HandleRefreshTokenAsync(dto));

            // Assert
            Assert.Equal("Invalid Refresh Token", ex.Message);
            _userManager.Verify(m => m.UpdateAsync(It.IsAny<AppUser>()), Times.Never);
        }

        [Theory]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        public async Task HandleRefreshTokenAsync_WhenTokenIsExpiredOrRevoked_ThrowsUnauthorizedException(
            bool isExpired,
            bool isRevoked)
        {
            // Arrange
            var storedToken = "refresh-token";
            var user = CreateUser();
            user.RefreshTokens.Add(CreateRefreshToken(
                storedToken,
                expiresOn: isExpired ? DateTime.UtcNow.AddMinutes(-5) : DateTime.UtcNow.AddDays(7),
                revokedOn: isRevoked ? DateTime.UtcNow.AddMinutes(-1) : null));
            SetupUsersQueryable(user);
            var dto = new GenrateRefreshToken { Token = storedToken };

            // Act
            var ex = await Assert.ThrowsAsync<UnauthorizedException>(
                () => _sut.HandleRefreshTokenAsync(dto));

            // Assert
            Assert.Equal("Invalid or Expired Refresh Token", ex.Message);
            _userManager.Verify(m => m.UpdateAsync(It.IsAny<AppUser>()), Times.Never);
        }

        [Fact]
        public async Task HandleRefreshTokenAsync_WhenTokenIsValid_RotatesTokensAndReturnsAuthenticationResponse()
        {
            // Arrange
            var storedToken = "valid-refresh-token";
            var user = CreateUser();
            var existing = CreateRefreshToken(storedToken);
            user.RefreshTokens.Add(existing);
            SetupUsersQueryable(user);
            var dto = new GenrateRefreshToken { Token = storedToken };

            // Act
            var result = await _sut.HandleRefreshTokenAsync(dto);

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(result.AccessToken));
            Assert.False(string.IsNullOrWhiteSpace(result.RefreshToken));
            Assert.NotEqual(storedToken, result.RefreshToken);
            Assert.Equal("Authentication Completed Successfulley !", result.message);
            Assert.NotNull(existing.revokedOn);
            Assert.Contains(user.RefreshTokens, t => t.Token == result.RefreshToken);
            Assert.Equal(2, user.RefreshTokens.Count);
            ReadJwt(result.AccessToken);
            _userManager.Verify(m => m.UpdateAsync(user), Times.Once);
        }

        [Fact]
        public async Task HandleRefreshTokenAsync_WhenDtoTokenIsNull_ThrowsUnauthorizedException()
        {
            // Arrange
            var user = CreateUser();
            user.RefreshTokens.Add(CreateRefreshToken("existing-token"));
            SetupUsersQueryable(user);
            var dto = new GenrateRefreshToken { Token = null! };

            // Act
            var ex = await Assert.ThrowsAsync<UnauthorizedException>(
                () => _sut.HandleRefreshTokenAsync(dto));

            // Assert
            Assert.Equal("Invalid Refresh Token", ex.Message);
        }

        #endregion

        #region Login

        [Fact]
        public async Task Login_WhenDtoIsNull_ThrowsArgumentNullException()
        {
            // Arrange / Act
            var ex = await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.Login(null!));

            // Assert
            Assert.Equal("loginDto", ex.ParamName);
        }

        [Fact]
        public async Task Login_WhenEmailDoesNotExist_ThrowsUnauthorizedException()
        {
            // Arrange
            var dto = new LoginDto { Email = "missing@test.com", Password = "Password1!" };
            _userManager.Setup(m => m.FindByEmailAsync(dto.Email)).ReturnsAsync((AppUser?)null);

            // Act
            var ex = await Assert.ThrowsAsync<UnauthorizedException>(() => _sut.Login(dto));

            // Assert
            Assert.Equal("Invalid Email or Password", ex.Message);
            _userManager.Verify(m => m.CheckPasswordAsync(It.IsAny<AppUser>(), It.IsAny<string>()), Times.Never);
            _userManager.Verify(m => m.UpdateAsync(It.IsAny<AppUser>()), Times.Never);
        }

        [Fact]
        public async Task Login_WhenPasswordIsInvalid_ThrowsUnauthorizedException()
        {
            // Arrange
            var user = CreateUser();
            var dto = new LoginDto { Email = user.Email!, Password = "WrongPassword1!" };
            _userManager.Setup(m => m.FindByEmailAsync(dto.Email)).ReturnsAsync(user);
            _userManager.Setup(m => m.CheckPasswordAsync(user, dto.Password)).ReturnsAsync(false);

            // Act
            var ex = await Assert.ThrowsAsync<UnauthorizedException>(() => _sut.Login(dto));

            // Assert
            Assert.Equal("Invalid Email or Password", ex.Message);
            _userManager.Verify(m => m.CheckPasswordAsync(user, dto.Password), Times.Once);
            _userManager.Verify(m => m.UpdateAsync(It.IsAny<AppUser>()), Times.Never);
        }

        [Fact]
        public async Task Login_WhenCredentialsAreValid_ReturnsTokensAndUpdatesUser()
        {
            // Arrange
            var user = CreateUser();
            var dto = new LoginDto { Email = user.Email!, Password = "CorrectPassword1!" };
            _userManager.Setup(m => m.FindByEmailAsync(dto.Email)).ReturnsAsync(user);
            _userManager.Setup(m => m.CheckPasswordAsync(user, dto.Password)).ReturnsAsync(true);

            // Act
            var result = await _sut.Login(dto);

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(result.AccessToken));
            Assert.False(string.IsNullOrWhiteSpace(result.RefreshToken));
            Assert.Null(result.message);
            ReadJwt(result.AccessToken);
            Assert.Empty(user.RefreshTokens);
            _userManager.Verify(m => m.UpdateAsync(user), Times.Once);
        }

        #endregion

        #region Register

        [Fact]
        public async Task Register_WhenDtoIsNull_ThrowsNullReferenceException()
        {
            // Arrange / Act / Assert
            // Email is read for logging before the null check, so this currently throws NRE.
            await Assert.ThrowsAsync<NullReferenceException>(() => _sut.Register(null!));
        }

        [Theory]
        [InlineData("DuplicateUserName", "Username is already taken.")]
        [InlineData("DuplicateEmail", "Email is already taken.")]
        [InlineData("PasswordTooShort", "Passwords must be at least 6 characters.")]
        public async Task Register_WhenCreateFails_ThrowsBadRequestException(string code, string description)
        {
            // Arrange
            var dto = new SignUpDto
            {
                UserName = "newuser",
                Email = "new@test.com",
                PhoneNumber = "01000000000",
                Password = "Password1!"
            };
            var mappedUser = CreateUser(email: dto.Email, userName: dto.UserName);
            _mapper.Setup(m => m.Map<AppUser>(dto)).Returns(mappedUser);
            _userManager
                .Setup(m => m.CreateAsync(mappedUser, dto.Password))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = code, Description = description }));

            // Act
            var ex = await Assert.ThrowsAsync<BadRequestException>(() => _sut.Register(dto));

            // Assert
            Assert.Equal($"{code}: {description}", ex.Message);
            _userManager.Verify(m => m.UpdateAsync(It.IsAny<AppUser>()), Times.Never);
        }

        [Fact]
        public async Task Register_WhenCreateSucceeds_ReturnsTokensAddsRefreshTokenAndUpdatesUser()
        {
            // Arrange
            var dto = new SignUpDto
            {
                UserName = "newuser",
                Email = "new@test.com",
                PhoneNumber = "01000000000",
                Password = "Password1!"
            };
            var mappedUser = CreateUser(email: dto.Email, userName: dto.UserName);
            _mapper.Setup(m => m.Map<AppUser>(dto)).Returns(mappedUser);
            _userManager
                .Setup(m => m.CreateAsync(mappedUser, dto.Password))
                .ReturnsAsync(IdentityResult.Success);
            _userManager
                .Setup(m => m.GenerateEmailConfirmationTokenAsync(It.IsAny<AppUser>()))
                .ReturnsAsync("confirmation-token");
            _gmailService
                .Setup(m => m.SendEmailAsync(It.IsAny<SendEmailRequestDto>()))
                .ReturnsAsync(new SendEmailResponseDto { Success = true, Message = "Email sent successfully" });

            // Act
            var result = await _sut.Register(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("User Registered Successfully", result.message);
            _mapper.Verify(m => m.Map<AppUser>(dto), Times.Once);
            _userManager.Verify(m => m.CreateAsync(mappedUser, dto.Password), Times.Once);
            _userManager.Verify(m => m.GenerateEmailConfirmationTokenAsync(mappedUser), Times.Once);
            _gmailService.Verify(m => m.SendEmailAsync(It.IsAny<SendEmailRequestDto>()), Times.Once);
        }

        #endregion

        #region ChangePassword

        [Fact]
        public async Task ChangePassword_WhenUserIsNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var dto = new ChangePasswordDto { oldPassword = "OldPass1!", newPassword = "NewPass1!" };
            _userManager.Setup(m => m.FindByIdAsync("missing-id")).ReturnsAsync((AppUser?)null);

            // Act
            var ex = await Assert.ThrowsAsync<NotFoundException>(
                () => _sut.ChangePassword(dto, "missing-id"));

            // Assert
            Assert.Equal("user Not Found", ex.Message);
            _userManager.Verify(
                m => m.ChangePasswordAsync(It.IsAny<AppUser>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task ChangePassword_WhenUserIdIsNullOrEmptyAndUserIsMissing_ThrowsNotFoundException(string? userId)
        {
            // Arrange
            var dto = new ChangePasswordDto { oldPassword = "OldPass1!", newPassword = "NewPass1!" };
            _userManager.Setup(m => m.FindByIdAsync(userId!)).ReturnsAsync((AppUser?)null);

            // Act
            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _sut.ChangePassword(dto, userId!));

            // Assert
            Assert.Equal("user Not Found", ex.Message);
        }

        [Fact]
        public async Task ChangePassword_WhenDtoIsNullAndUserExists_ThrowsNullReferenceException()
        {
            // Arrange
            var user = CreateUser();
            _userManager.Setup(m => m.FindByIdAsync(user.Id)).ReturnsAsync(user);

            // Act / Assert
            await Assert.ThrowsAsync<NullReferenceException>(() => _sut.ChangePassword(null!, user.Id));
        }

        [Theory]
        [InlineData("PasswordMismatch", "Incorrect password.")]
        [InlineData("PasswordTooShort", "Passwords must be at least 6 characters.")]
        public async Task ChangePassword_WhenIdentityFails_ThrowsBadRequestException(string code, string description)
        {
            // Arrange
            var user = CreateUser();
            var dto = new ChangePasswordDto { oldPassword = "OldPass1!", newPassword = "NewPass1!" };
            _userManager.Setup(m => m.FindByIdAsync(user.Id)).ReturnsAsync(user);
            _userManager
                .Setup(m => m.ChangePasswordAsync(user, dto.oldPassword, dto.newPassword))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = code, Description = description }));

            // Act
            var ex = await Assert.ThrowsAsync<BadRequestException>(() => _sut.ChangePassword(dto, user.Id));

            // Assert
            Assert.Equal($"{code}: {description}", ex.Message);
        }

        [Fact]
        public async Task ChangePassword_WhenIdentitySucceeds_ReturnsSuccessMessage()
        {
            // Arrange
            var user = CreateUser();
            var dto = new ChangePasswordDto { oldPassword = "OldPass1!", newPassword = "NewPass1!" };
            _userManager.Setup(m => m.FindByIdAsync(user.Id)).ReturnsAsync(user);
            _userManager
                .Setup(m => m.ChangePasswordAsync(user, dto.oldPassword, dto.newPassword))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _sut.ChangePassword(dto, user.Id);

            // Assert
            Assert.Equal("Password Changed Successfully", result);
            _userManager.Verify(m => m.ChangePasswordAsync(user, dto.oldPassword, dto.newPassword), Times.Once);
        }

        #endregion

        #region Logout

        [Fact]
        public async Task Logout_WhenUserIsNotFound_ThrowsNotFoundException()
        {
            // Arrange
            SetupUsersQueryable();

            // Act
            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _sut.Logout("missing-id"));

            // Assert
            Assert.Equal("user", ex.Message);
            _userManager.Verify(m => m.UpdateAsync(It.IsAny<AppUser>()), Times.Never);
        }

        [Fact]
        public async Task Logout_WhenUpdateFails_ThrowsException()
        {
            // Arrange
            var user = CreateUser();
            user.RefreshTokens.Add(CreateRefreshToken("active-token"));
            SetupUsersQueryable(user);
            _userManager
                .Setup(m => m.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "UpdateFailed", Description = "Failed" }));

            // Act
            var ex = await Assert.ThrowsAsync<Exception>(() => _sut.Logout(user.Id));

            // Assert
            Assert.Equal("Failed to logout user.", ex.Message);
        }

        [Fact]
        public async Task Logout_WhenUserExists_RevokesActiveTokensAndUpdatesUser()
        {
            // Arrange
            var user = CreateUser();
            var active = CreateRefreshToken("active-token");
            var alreadyRevoked = CreateRefreshToken("revoked-token", revokedOn: DateTime.UtcNow.AddHours(-1));
            var expired = CreateRefreshToken("expired-token", expiresOn: DateTime.UtcNow.AddDays(-1));
            user.RefreshTokens.Add(active);
            user.RefreshTokens.Add(alreadyRevoked);
            user.RefreshTokens.Add(expired);
            var previousRevokedOn = alreadyRevoked.revokedOn;
            SetupUsersQueryable(user);

            // Act
            await _sut.Logout(user.Id);

            // Assert
            Assert.NotNull(active.revokedOn);
            Assert.Equal(previousRevokedOn, alreadyRevoked.revokedOn);
            Assert.Null(expired.revokedOn);
            _userManager.Verify(m => m.UpdateAsync(user), Times.Once);
        }

        [Fact]
        public async Task Logout_WhenUserHasNoRefreshTokens_StillUpdatesUser()
        {
            // Arrange
            var user = CreateUser();
            SetupUsersQueryable(user);

            // Act
            await _sut.Logout(user.Id);

            // Assert
            Assert.Empty(user.RefreshTokens);
            _userManager.Verify(m => m.UpdateAsync(user), Times.Once);
        }

        #endregion
    }
}
