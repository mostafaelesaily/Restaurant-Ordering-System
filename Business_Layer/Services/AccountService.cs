using AutoMapper;
using Business_Layer.DTOs.Authentication;
using Business_Layer.DTOs.RefreshTokenDtos;
using Business_Layer.DTOs.UserDTOs;
using Business_Layer.Exceptions;
using Business_Layer.Interfaces.IService;
using Domain_Layer.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Business_Layer.Services
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<AppUser> userManager;
        private readonly IConfiguration configuration;
        private readonly ILogger<AccountService> logger;
        private readonly IMapper mapper; 

        public AccountService
            (
             UserManager<AppUser> userManager,
             IConfiguration configuration,
             ILogger<AccountService> logger,
             IMapper mapper
            )
        {
            this.userManager = userManager;
            this.configuration = configuration;
            this.logger = logger;
            this.mapper = mapper;
        }
        public async Task<string> ChangePassword(ChangePasswordDto changePasswordDto, string userId)
        {
            logger.LogInformation("Attempting to change password for user with ID: {UserId}", userId);
            var user =  await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                logger.LogWarning("user with id {userId} Not Found",userId);
                throw new NotFoundException("user Not Found");
            }
            var result = await userManager.ChangePasswordAsync(user,
                changePasswordDto.oldPassword,
                changePasswordDto.newPassword
                );
            if (!result.Succeeded)
            {
                logger.LogWarning("Password change failed for user with ID: {UserId}. Errors: {Errors}",
                   userId, string.Join(", ", result.Errors.Select(e => $"{e.Code}: {e.Description}")));
                var errors = string.Join(", ", result.Errors.Select(e => $"{e.Code}: {e.Description}"));
                throw new BadRequestException(errors);
            }
            logger.LogInformation("Password changed successfully for user with ID: {UserId}", userId);
            return "Password Changed Successfully";
        }

        public async Task<string> GenrateAccessToken(AppUser appUser)
        {
            var claims = new List<Claim>() 
            {
             new Claim(ClaimTypes.NameIdentifier, appUser.Id),
             new Claim(ClaimTypes.Email, appUser.Email),
             new Claim(ClaimTypes.Name, appUser.UserName),
             new Claim(JwtRegisteredClaimNames.Jti , Guid.NewGuid().ToString())
            };
            var roles = await userManager.GetRolesAsync(appUser);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Key"]));
            var sigKey = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken
            (
                issuer: configuration["JWT:Issuer"],
                audience: configuration["JWT:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(3),
                signingCredentials: sigKey
            );
            var _token = new JwtSecurityTokenHandler().WriteToken(token);
            return _token;
        }

        public async Task<RefreshTokens> GenrateRefreshToken()
        {
            var RefreshToken = new RefreshTokens()
            {
                CreatedOn = DateTime.UtcNow,
                ExpiresOn = DateTime.UtcNow.AddDays(7),
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64))
            };
            return RefreshToken;

        }

        public async Task<AuthenticationResponseDto> HandleRefreshTokenAsync(GenrateRefreshToken refreshTokenDto)
        {
            if (refreshTokenDto == null) throw new ArgumentNullException(nameof(refreshTokenDto));
            var user = userManager.Users.FirstOrDefault(u => u.RefreshTokens.Any(t => t.Token == refreshTokenDto.Token));
            if (user == null) throw new UnauthorizedException("Invalid Refresh Token");

            var refreshToken = user.RefreshTokens.FirstOrDefault(t => t.Token == refreshTokenDto.Token);
            if (refreshToken == null || refreshToken.ExpiresOn < DateTime.UtcNow || refreshToken.revokedOn != null)
            {
                throw new UnauthorizedException("Invalid or Expired Refresh Token");
            }
            refreshToken.revokedOn = DateTime.UtcNow;
            var newAccessToken = await GenrateAccessToken(user);
            var newRefreshToken = await GenrateRefreshToken();
            user.RefreshTokens.Add(newRefreshToken);
            await userManager.UpdateAsync(user);
            return new AuthenticationResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken.Token,
                ExpireOn = DateTime.UtcNow.AddHours(3),
                message = "Authentication Completed Successfulley !"
            };
        }

        public async Task<AuthenticationResponseDto> Login(LoginDto loginDto)
        {
            if (loginDto == null) throw new ArgumentNullException(nameof(loginDto));
            var user = await userManager.FindByEmailAsync(loginDto.Email);
            logger.LogInformation("Attempting login for user with email: {Email}", loginDto.Email);
            if (user == null) throw new UnauthorizedException("Invalid Email or Password");
            var isPasswordValid = await userManager.CheckPasswordAsync(user, loginDto.Password);

            if (!isPasswordValid)
            {
                logger.LogWarning("Login failed for user with email: {Email}. " +
                    "Invalid credentials.", loginDto.Email);
                throw new UnauthorizedException("Invalid Email or Password");
            }
            var Accesstoken = await GenrateAccessToken(user);
            var RefreshToken = await GenrateRefreshToken();
            await userManager.UpdateAsync(user);
            logger.LogInformation("User logged in successfully with email: {Email}", loginDto.Email);
            return new AuthenticationResponseDto
            {
                AccessToken = Accesstoken,
                RefreshToken = RefreshToken.Token,
                ExpireOn = DateTime.UtcNow.AddHours(3)
            };
        }

        public async Task Logout(string userId)
        {
            logger.LogInformation("Trying To Logging User Out with iD :{userId} ",userId);
           var user =  await userManager.Users.
                Include(r => r.RefreshTokens)
                .FirstOrDefaultAsync(u => u.Id == userId);
           if (user == null)
            {
                logger.LogInformation("User With id {userId} notFound", userId);
                throw new NotFoundException(nameof(user));
            }
            foreach (var token in user.RefreshTokens.Where(t => t.isActive))
            {
                token.revokedOn = DateTime.UtcNow;
            }
           var result =  await userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                logger.LogError("Failed to logout user with ID: {UserId}", userId);
                throw new Exception("Failed to logout user.");
            }
            logger.LogInformation("User with ID: {UserId} logged out successfully.", userId);
        }

        public async Task<AuthenticationResponseDto> Register(SignUpDto RegisterDto)
        {
            logger.LogInformation("Registering new user with email: {Email}", RegisterDto.Email);
            if (RegisterDto == null) { throw new ArgumentNullException(nameof(RegisterDto)); }
            var user =  mapper.Map<AppUser>(RegisterDto);
            var result = await userManager.CreateAsync(user,RegisterDto.Password);
            if (!result.Succeeded)
            {
                logger.LogWarning("User registration failed for email: {Email}. Errors: {Errors}",
                RegisterDto.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
                var errors = string.Join(", ", result.Errors.Select(e => $"{e.Code}: {e.Description}"));
                throw new BadRequestException(errors);
            }
            var Accesstoken = await GenrateAccessToken(user);
            var RefreshToken = await GenrateRefreshToken();
            user.RefreshTokens.Add(RefreshToken);
            await userManager.UpdateAsync(user);
            logger.LogInformation("User registered successfully with email: {Email}", RegisterDto.Email);
            return new AuthenticationResponseDto
            {
                AccessToken = Accesstoken ,
                RefreshToken = RefreshToken.Token,
                ExpireOn = DateTime.UtcNow.AddHours(3),
                message = "User Registered Successfully"
            };

        }
    }
}
