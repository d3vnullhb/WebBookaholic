using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Bookaholic.ViewModels
{
    public class CheckOutViewModel
    {
        // ==== THÔNG TIN NGƯỜI NHẬN ====
        [Required(ErrorMessage = "Họ và tên là bắt buộc")]
        public string FullName { get; set; } = "";

        [Required(ErrorMessage = "Địa chỉ giao hàng là bắt buộc")]
        public string Address { get; set; } = "";

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [RegularExpression(@"^(0|\+84)(3[2-9]|5[6|8|9]|7[0|6-9]|8[1-5]|9[0-9])[0-9]{7}$", ErrorMessage = "Số điện thoại không hợp lệ")]
        public string Phone { get; set; }
        public string? Note { get; set; }

        // ==== GIỎ HÀNG ====
        public List<CartItemViewModel> Items { get; set; } = new();

        public decimal Total => Items?.Sum(i => i.Subtotal) ?? 0;

        // ==== GIAO HÀNG ====
        public int ShippingFee { get; set; } = 20000;

        // ==== THANH TOÁN ====
        [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán")]
        public string PaymentMethod { get; set; } = "COD"; // hoặc "VNPAY"

        public decimal GrandTotal => Total + ShippingFee;
    }
}
