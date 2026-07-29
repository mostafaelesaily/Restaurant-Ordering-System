using Business_Layer.DTOs.PaginatedDtos;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using Application.DTOs.CouponDTOs;

namespace Application.Interfaces.IService
{
    public interface ICouponService
    {
        Task<PaginatedResultDto<GetCouponDto>> GetAllCopounsPagged(int pageNum, int PageSize);
        Task<PaginatedResultDto<GetCouponDto>> SearchCoupons(
        string? search,
        int pageNum,
        int pageSize);
        Task<GetCouponDto?> ValidateCoupon(string code);
        Task<GetCouponDto?> GetCopounById(int id);
        Task<GetCouponDto> CreateCopoun(CreateCouponDto dto);
        Task UpdateCopoun(int id, UpdateCouponDto dto);
        Task DeleteCopoun(int id);
        
    }
}
