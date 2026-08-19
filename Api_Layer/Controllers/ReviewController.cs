using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Resturant_Ordering_System.Application.DTOs.ReviewDTOs;
using Resturant_Ordering_System.Application.Interfaces.IService;
using System.Security.Claims;

namespace Resturant_Ordering_System.Api_Layer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService reviewService;

        public ReviewController(IReviewService reviewService)
        {
            this.reviewService = reviewService;
        }

        [HttpGet("[action]")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllReviews(int pageNum, int pageSize)
        {
            var result = await reviewService.GetAllReviews(pageNum, pageSize);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetReviewById(int id)
        {
            var result = await reviewService.GetReviewById(id);
            return Ok(result);
        }

        [HttpGet("[action]")]
        [Authorize]
        public async Task<IActionResult> SearchReviews(string? search, int pageNum, int pageSize)
        {
            var result = await reviewService.SearchReviews(search, pageNum, pageSize);
            return Ok(result);
        }

        [HttpGet("[action]")]
        [Authorize]
        public async Task<IActionResult> GetMenuItemReviews(int menuItemId, int pageNum, int pageSize)
        {
            var result = await reviewService.GetMenuItemReviews(menuItemId, pageNum, pageSize);
            return Ok(result);
        }

        [HttpGet("[action]")]
        [Authorize]
        public async Task<IActionResult> GetMyReviews(int pageNum, int pageSize)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await reviewService.GetUserReviews(userId, pageNum, pageSize);
            return Ok(result);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateReview([FromBody] CreateReviewDto reviewCreateDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await reviewService.CreateReview(reviewCreateDto, userId);
            return Ok(result);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateReview(int id, [FromBody] UpdateReviewDto reviewUpdateDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await reviewService.UpdateReview(id, reviewUpdateDto, userId);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await reviewService.DeleteReview(id, userId);
            return NoContent();
        }
    }
}