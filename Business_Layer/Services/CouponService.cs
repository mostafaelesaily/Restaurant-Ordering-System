using Application.DTOs.CouponDTOs;
using Application.Interfaces.IService;
using AutoMapper;
using Business_Layer.DTOs.PaginatedDtos;
using Business_Layer.Exceptions;
using Business_Layer.Interfaces;
using Domain_Layer.Entities;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class CouponService : ICouponService
    {
        private readonly IUow uow;
        private readonly ICacheService cacheService;
        private readonly ILogger<CouponService> logger;
        private readonly IMapper mapper;

        public CouponService(IUow uow, ICacheService cacheService, ILogger<CouponService> logger, IMapper mapper)
        {
            this.uow = uow;
            this.cacheService = cacheService;
            this.logger = logger;
            this.mapper = mapper;
        }

        public async Task<PaginatedResultDto<GetCouponDto>> GetAllCopounsPagged(int pageNum, int PageSize)
        {
            logger.LogInformation("Attempting to get coupons page {pageNum} size {pageSize}", pageNum, PageSize);
            var cacheKey = $"Get_Coupons_pageNum:{pageNum}_pageSize:{PageSize}";

            var result = await cacheService.GetOrSetAsync(cacheKey, async () =>
            {
               
                var coupons = await uow.couponRepo.GetAllPaged(pageNum, PageSize);
                return new PaginatedResultDto<GetCouponDto>
                {
                    Data = mapper.Map<List<GetCouponDto>>(coupons.Data),
                    PageNumber = pageNum,
                    PageSize = PageSize,
                    TotalCount = coupons.TotalCount
                };
            });

            return result!;
        }

        public async Task<GetCouponDto?> GetCopounById(int id)
        {
            logger.LogInformation("Attempting to get coupon with id {id}", id);
            var coupon = await uow.couponRepo.GetByIdAsync(id);
            if (coupon == null)
            {
                logger.LogWarning("Coupon with id {id} not found", id);
                throw new NotFoundException("Coupon not found");
            }
            return mapper.Map<GetCouponDto>(coupon);
        }

        public async Task<GetCouponDto> CreateCopoun(CreateCouponDto dto)
        {
            logger.LogInformation("Attempting to create coupon");
            await using var transaction = await uow.BeginTransactionAsync();
            try
            {
                var coupon = mapper.Map<Coupon>(dto);
                await uow.couponRepo.CreateAsync(coupon);
                await uow.SaveChangesAsync();
                await transaction.CommitAsync();
                await cacheService.RemoveAsync("Get_Coupons");
                return mapper.Map<GetCouponDto>(coupon);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task UpdateCopoun(int id, UpdateCouponDto dto)
        {
            logger.LogInformation("Attempting to update coupon with id {id}", id);
            await using var transaction = await uow.BeginTransactionAsync();
            try
            {
                var coupon = await uow.couponRepo.GetByIdAsync(id);
                if (coupon == null)
                {
                    logger.LogWarning("Coupon with id {id} not found", id);
                    throw new NotFoundException("Coupon not found");
                }
                mapper.Map(dto, coupon);
                await uow.SaveChangesAsync();
                await transaction.CommitAsync();
                await cacheService.RemoveAsync("Get_Coupons");
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task DeleteCopoun(int id)
        {
            logger.LogInformation("Attempting to delete coupon with id {id}", id);
            await using var transaction = await uow.BeginTransactionAsync();
            try
            {
                var coupon = await uow.couponRepo.GetByIdAsync(id);
                if (coupon == null)
                {
                    logger.LogWarning("Coupon with id {id} not found", id);
                    throw new NotFoundException("Coupon not found");
                }
                await uow.couponRepo.DeleteAsync(coupon);
                await uow.SaveChangesAsync();
                await transaction.CommitAsync();
                await cacheService.RemoveAsync("Get_Coupons");
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        public async Task<GetCouponDto?> ValidateCoupon(string code)
        {
            logger.LogInformation("Attempting to get coupon with code {code}", code);
            var coupon = await uow.couponRepo.FindElementAsync(x => x.Code == code);
            if (coupon == null)
            {
                logger.LogWarning("Coupon with code {code} not found", code);
                throw new NotFoundException("Coupon not found");
            }
            if (coupon.IsActive == false)
            {
                throw new BadRequestException("Coupon is inactive.");

            }
            if (coupon.ExpireDate < DateTime.UtcNow)
                throw new BadRequestException("Coupon has expired.");
            return mapper.Map<GetCouponDto>(coupon);
        }
        public async Task<PaginatedResultDto<GetCouponDto>> SearchCoupons(
        string? search,
        int pageNum,
        int pageSize)
        {
            logger.LogInformation(
                 "Attempting To Search Coupons : page {pageNum} And size {pageSize}",
                 pageNum,
                 pageSize);
            var cacheKey = $"Search_Coupon_{search}_page:{pageNum}_size:{pageSize}";
            var result = await cacheService.GetOrSetAsync(cacheKey,
                async() =>
                {
                    var query =  uow.couponRepo.SearchCoupons(search);
                    var Coupons = await uow.couponRepo.GetAllPaged(pageNum, pageSize, query);
                    return new PaginatedResultDto<GetCouponDto>
                    {
                        Data = mapper.Map<List<GetCouponDto>>(Coupons.Data),
                        PageNumber = pageNum,
                        PageSize = pageSize,
                        TotalCount = Coupons.TotalCount
                    };
                }
                );
            return result;
        }
    }
}
