using Business_Layer.DTOs.PaginatedDtos;
using Resturant_Ordering_System.Application.DTOs.ReviewDTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Resturant_Ordering_System.Application.Interfaces.IService
{
    public interface IReviewService
    {
        Task<PaginatedResultDto<ReviewDetailsDto>> GetAllReviews(int pageNum, int pageSize);
        Task<PaginatedResultDto<ReviewDetailsDto>> GetMenuItemReviews(int menuItemId, int pageNum, int pageSize);
        Task<PaginatedResultDto<ReviewDetailsDto>> GetUserReviews(string userId, int pageNum, int pageSize);
        Task<PaginatedResultDto<ReviewDetailsDto>> SearchReviews(string? search, int pageNum, int pageSize);
        Task<ReviewDetailsDto> GetReviewById(int reviewId);
        Task<ReviewDetailsDto> CreateReview(CreateReviewDto reviewCreateDto , string customerId);
        Task UpdateReview(int reviewId, UpdateReviewDto reviewUpdateDto, string customerId);
        Task DeleteReview(int reviewId, string customerId);
    }
}
