using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Resturant_Ordering_System.Application.DTOs.ReservationDtos;
using Resturant_Ordering_System.Application.Interfaces.IService;
using System.Security.Claims;

namespace Resturant_Ordering_System.Api_Layer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservationController : ControllerBase
    {
        private readonly IReservationService reservationService;

        public ReservationController(IReservationService reservationService)
        {
            this.reservationService = reservationService;
        }

        [HttpGet("[action]")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllReservation(int pageNum, int pageSize)
        {
            var result = await reservationService.GetAllReservation(pageNum, pageSize);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetReservationById(int id)
        {
            var result = await reservationService.GetReservationById(id);
            return Ok(result);
        }

        [HttpGet("[action]")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SearchReservations(string? search, int pageNum, int pageSize)
        {
            var result = await reservationService.SearchReservations(search, pageNum, pageSize);
            return Ok(result);
        }

        [HttpGet("[action]")]
        [Authorize]
        public async Task<IActionResult> GetMyReservations(int pageNum, int pageSize)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await reservationService.GetUserReservations(userId, pageNum, pageSize);
            return Ok(result);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateReservation([FromBody] CreateReservationDto reservationCreateDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await reservationService.CreateReservation(reservationCreateDto, userId);
            return Ok(result);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateReservation(int id, [FromBody] UpdateReservationDto reservationUpdateDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await reservationService.UpdateReservation(id, reservationUpdateDto, userId);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteReservation(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await reservationService.DeleteReservation(id, userId);
            return NoContent();
        }
    }
}