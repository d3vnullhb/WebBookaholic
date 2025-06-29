using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bookaholic.Models
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }

        [Required, StringLength(100)]
        public string FullName { get; set; }

        [Required, StringLength(15)]
        public string Phone { get; set; }

        [Required, StringLength(255)]
        public string Address { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.Now;

        [StringLength(500)]
        public string? ShippingAddress { get; set; }

        [Range(0, int.MaxValue)]
        public int ShippingFee { get; set; } = 20000;

        public int? VoucherId { get; set; }

        [ForeignKey("VoucherId")]
        public Voucher Voucher { get; set; }

        [Range(0, int.MaxValue)]
        public int DiscountAmount { get; set; } = 0;

        [Range(0, double.MaxValue)]
        public decimal TotalAmount { get; set; }

        [StringLength(50)]
        public string PaymentMethod { get; set; }

        [Required, StringLength(50)]
        public string PaymentStatus { get; set; } 

        [Required, StringLength(50)]
        public string OrderStatus { get; set; } = "Chờ xác nhận";

        [StringLength(500)]
        public string? Note { get; set; }

        // Navigation
        public ICollection<OrderDetail> OrderDetails { get; set; }
    }
}
