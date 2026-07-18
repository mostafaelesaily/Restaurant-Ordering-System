using Business_Layer.DTOs.Authentication;
using Business_Layer.DTOs.RefreshTokenDtos;
using Business_Layer.DTOs.UserDTOs;
using Domain_Layer.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Business_Layer.Interfaces.IService
{
    public interface IAccountService
    {
        public Task<string> GenrateAccessToken(AppUser appUser);
        public Task<RefreshTokens> GenrateRefreshToken();
        public Task<AuthenticationResponseDto> HandleRefreshTokenAsync(GenrateRefreshToken refreshTokenDto);
        public Task<AuthenticationResponseDto> Register(SignUpDto signUpDto);
        public Task<AuthenticationResponseDto> Login(LoginDto loginDto);
        public Task<string> ChangePassword(ChangePasswordDto changePasswordDto, string userId);
        public Task Logout(string userId);
    }
}
