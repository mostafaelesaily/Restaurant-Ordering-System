using AutoMapper;
using Business_Layer.DTOs.NotificationDTOs;
using Business_Layer.DTOs.PaginatedDtos;
using Business_Layer.Exceptions;
using Business_Layer.Interfaces;
using Domain_Layer.Entities;
using Microsoft.Extensions.Logging;
using Resturant_Ordering_System.Application.DTOs.ReviewDTOs;
using Resturant_Ordering_System.Application.Interfaces.IService;

namespace Resturant_Ordering_System.Application.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IUow uow;
        private readonly ICacheService cacheService;
        private readonly IMapper mapper;
        private readonly ILogger<ReviewService> logger;
        private readonly INotificationService notificationService;
        private readonly ISendNotificationService sendNotificationService;
        public ReviewService(IUow uow, ICacheService cacheService, IMapper mapper, ILogger<ReviewService> logger,
            INotificationService notificationService, ISendNotificationService sendNotificationService)
        {
            this.uow = uow;
            this.cacheService = cacheService;
            this.mapper = mapper;
            this.logger = logger;
            this.notificationService = notificationService;
            this.sendNotificationService = sendNotificationService;
        }

        public async Task<PaginatedResultDto<ReviewDetailsDto>> GetAllReviews(int pageNum, int pageSize)
        {
            logger.LogInformation("Attempting to get reviews page {pageNum} size {pageSize}", pageNum, pageSize);
            var cacheKey = $"Get_Reviews_pageNum:{pageNum}_pageSize:{pageSize}";

            var result = await cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                var query =  uow.Reviews.GetReviewsWithDetails();
                var reviews = await uow.Reviews.GetAllPaged(pageNum, pageSize,query);
                return new PaginatedResultDto<ReviewDetailsDto>
                {
                    Data = mapper.Map<List<ReviewDetailsDto>>(reviews.Data),
                    PageNumber = pageNum,
                    PageSize = pageSize,
                    TotalCount = reviews.TotalCount
                };
            });

            return result!;
        }

        public async Task<ReviewDetailsDto> GetReviewById(int reviewId)
        {
            logger.LogInformation("Attempting to get review with id {id}", reviewId);
            var review = await uow.Reviews.GetReviewWithDetails(reviewId);
            if (review == null)
            {
                logger.LogWarning("Review with id {id} not found", reviewId);
                throw new NotFoundException("Review not found");
            }
            return mapper.Map<ReviewDetailsDto>(review);
        }

        public async Task<PaginatedResultDto<ReviewDetailsDto>> GetMenuItemReviews(int menuItemId, int pageNum, int pageSize)
        {
            logger.LogInformation("Attempting to get reviews for menu item {menuItemId} page {pageNum} size {pageSize}",
                menuItemId, pageNum, pageSize);
            var cacheKey = $"Reviews_MenuItem_{menuItemId}_page:{pageNum}_size:{pageSize}";

            var result = await cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                var query = uow.Reviews.GetMenuItemReviews(menuItemId);
                var reviews = await uow.Reviews.GetAllPaged(pageNum, pageSize, query);
                return new PaginatedResultDto<ReviewDetailsDto>
                {
                    Data = mapper.Map<List<ReviewDetailsDto>>(reviews.Data),
                    PageNumber = pageNum,
                    PageSize = pageSize,
                    TotalCount = reviews.TotalCount
                };
            });

            return result!;
        }

        public async Task<PaginatedResultDto<ReviewDetailsDto>> GetUserReviews(string userId, int pageNum, int pageSize)
        {
            logger.LogInformation("Attempting to get reviews for user {userId} page {pageNum} size {pageSize}",
                userId, pageNum, pageSize);
            var cacheKey = $"Reviews_User_{userId}_page:{pageNum}_size:{pageSize}";

            var result = await cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                var query = uow.Reviews.GetUserReviews(userId);
                var reviews = await uow.Reviews.GetAllPaged(pageNum, pageSize, query);
                return new PaginatedResultDto<ReviewDetailsDto>
                {
                    Data = mapper.Map<List<ReviewDetailsDto>>(reviews.Data),
                    PageNumber = pageNum,
                    PageSize = pageSize,
                    TotalCount = reviews.TotalCount
                };
            });

            return result!;
        }

        public async Task<PaginatedResultDto<ReviewDetailsDto>> SearchReviews(string? search, int pageNum, int pageSize)
        {
            logger.LogInformation("Attempting to search reviews with key {search} page {pageNum} size {pageSize}",
                search, pageNum, pageSize);
            var cacheKey = $"Search_Reviews_{search}_page:{pageNum}_size:{pageSize}";

            var result = await cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                var query = uow.Reviews.SearchReviews(search);
                var reviews = await uow.Reviews.GetAllPaged(pageNum, pageSize, query);
                return new PaginatedResultDto<ReviewDetailsDto>
                {
                    Data = mapper.Map<List<ReviewDetailsDto>>(reviews.Data),
                    PageNumber = pageNum,
                    PageSize = pageSize,
                    TotalCount = reviews.TotalCount
                };
            });

            return result!;
        }

        public async Task<ReviewDetailsDto> CreateReview(CreateReviewDto reviewCreateDto, string customerId)
        {
            logger.LogInformation("Attempting to create review for customer {customerId} and menu item {menuItemId}",
                customerId, reviewCreateDto.MenuItemId);

            await using var transaction = await uow.BeginTransactionAsync();
            try
            {
                var user = await uow.AppUserRepo.GetByIdAsync(customerId);
                if (user == null)
                {
                    logger.LogInformation("Customer with id {customerId} does not exist", customerId);
                    throw new NotFoundException("Customer not found");
                }

                var menuItem = await uow.MenuItems.GetByIdAsync(reviewCreateDto.MenuItemId);
                if (menuItem == null)
                {
                    logger.LogInformation("Menu item with id {menuItemId} does not exist", reviewCreateDto.MenuItemId);
                    throw new NotFoundException("Menu item not found");
                }

                var userReviews =  uow.Reviews.GetUserReviews(customerId);

                var alreadyReviewed = userReviews.Any(r =>
                r.CustomerId == customerId &&
                r.MenuItems.id == reviewCreateDto.MenuItemId);

                if (alreadyReviewed)
                {
                    logger.LogInformation(
                        "Customer with id : {customerId} Already Has Review On Product With Id : {productId}",
                        customerId,
                        reviewCreateDto.MenuItemId);

                    throw new BadRequestException("Review Already Exist");
                }

                var review = mapper.Map<Reviews>(reviewCreateDto);
                review.CustomerId = customerId;
                review.CreatedAt = DateTime.UtcNow;
                await uow.Reviews.CreateAsync(review);
                var notificationDto = new CreateNotificationDto
                {
                    Title = "New Review Created",
                    Message = $"A new review has been created for menu item" +
                    $" {menuItem.name} by {user.UserName}.",
                    UserId = user.Id,
                };
                await uow.SaveChangesAsync();
                await transaction.CommitAsync();
                await notificationService.CreateAsync(notificationDto);
                await sendNotificationService.SendToUserAsync(customerId,
                    "A new review has been" +
                 " created for a menu item you reviewed.");
                await cacheService.RemoveAsync("Get_Reviews");
                logger.LogInformation("Review created successfully with ID {id}", review.Id);
                return mapper.Map<ReviewDetailsDto>(review);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task UpdateReview(int reviewId, UpdateReviewDto reviewUpdateDto, string customerId)
        {
            logger.LogInformation("Attempting to update review with id {id}", reviewId);
            await using var transaction = await uow.BeginTransactionAsync();
            try
            {
                    var review = await uow.Reviews.GetByIdAsync(reviewId);
                if (review == null || review.CustomerId != customerId)
                {
                    logger.LogWarning("Review with id {id} not found for customer {customerId}", reviewId, customerId);
                    throw new NotFoundException("Review not found");
                }

                review.Rating = reviewUpdateDto.Rating;
                review.Comment = reviewUpdateDto.Comment;
                var updateNotificationDto = new CreateNotificationDto
                {
                    Title = "Review Updated",
                    Message = $"Your review with ID {reviewId} has been updated successfully.",
                    UserId = customerId,
                };
                await uow.SaveChangesAsync();
                await transaction.CommitAsync();
                await uow.Reviews.UpdateAsync(review);
                await notificationService.CreateAsync(updateNotificationDto);
                await sendNotificationService.SendToUserAsync(customerId,
                    "Your review has" +
                " been updated successfully.");
                await cacheService.RemoveAsync("Get_Reviews");
                logger.LogInformation("Review with ID {id} updated successfully", reviewId);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task DeleteReview(int reviewId, string customerId)
        {
            logger.LogInformation("Attempting to delete review with id {id}", reviewId);
            await using var transaction = await uow.BeginTransactionAsync();
            try
            {
                var review = await uow.Reviews.GetByIdAsync(reviewId);
                if (review == null || review.CustomerId != customerId)
                {
                    logger.LogWarning("Review with id {id} not found", reviewId);
                    throw new NotFoundException("Review not found");
                }

                await uow.Reviews.DeleteAsync(review);
                var deleteNotificationDto = new CreateNotificationDto
                {
                    Title = "Review Deleted",
                    Message = $"Your review with ID {reviewId} has been deleted successfully.",
                    UserId = customerId,
                };
                await notificationService.CreateAsync(deleteNotificationDto);
                await sendNotificationService.SendToUserAsync(customerId, "Your review has" +
                    " been deleted successfully.");
                await uow.SaveChangesAsync();
                await transaction.CommitAsync();
                await cacheService.RemoveAsync("Get_Reviews");
                logger.LogInformation("Review with ID {id} deleted successfully", reviewId);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}