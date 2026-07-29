using System;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Application.DTOs.CouponDTOs
{
    public class CreateCouponDto
    {
        [Required]
        [MaxLength(100)]
        public string Code { get; set; }

        [Required]
        public decimal Discount { get; set; }

        public DateTime ExpireDate { get; set; } = DateTime.UtcNow.AddDays(30);

        public int MaxUsage { get; set; } = 1;

        public bool IsActive { get; set; } = true;
    }
}
