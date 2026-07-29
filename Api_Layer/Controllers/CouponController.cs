using Application.DTOs.CouponDTOs;
using Application.Interfaces.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api_Layer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CouponController : ControllerBase
    {
        private readonly ICouponService couponService;

        public CouponController(ICouponService couponService)
        {
            this.couponService = couponService;
        }

        [HttpGet("[action]")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetAllCopouns(int pageNum, int pageSize)
        {
            var result = await couponService.GetAllCopounsPagged(pageNum, pageSize);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await couponService.GetCopounById(id);
            return Ok(result);
        }

        [HttpGet("[action]")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Search(string? search, int pageNum, int pageSize)
        {
            var result = await couponService.SearchCoupons(search, pageNum, pageSize);
            return Ok(result);
        }

        [HttpGet("[action]")]
        [Authorize]
        public async Task<IActionResult> Validate(string code)
        {
            var result = await couponService.ValidateCoupon(code);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(CreateCouponDto dto)
        {
            var result = await couponService.CreateCopoun(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, UpdateCouponDto dto)
        {
            await couponService.UpdateCopoun(id, dto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            await couponService.DeleteCopoun(id);
            return NoContent();
        }
    }
}
