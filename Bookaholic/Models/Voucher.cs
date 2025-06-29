using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Bookaholic.Models
{
    public class Voucher
    {
        [Key]
        public int VoucherId { get; set; }

        [Required, StringLength(50)]
        public string Code { get; set; }

        [Required, StringLength(20)]
        public string DiscountType { get; set; } // "percent", "money", "freeship"

        [Required, Range(0, int.MaxValue)]
        public int DiscountValue { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? MinOrderAmount { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<Order> Orders { get; set; }
    }
}
