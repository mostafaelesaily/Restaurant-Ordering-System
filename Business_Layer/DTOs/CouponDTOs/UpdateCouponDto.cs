using System;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Application.DTOs.CouponDTOs
{
    public class UpdateCouponDto
    {
        [Required]
        [MaxLength(100)]
        public string Code { get; set; }

        [Required]
        public decimal Discount { get; set; }

        public DateTime ExpireDate { get; set; }

        public int MaxUsage { get; set; }

        public bool IsActive { get; set; }
    }
}
