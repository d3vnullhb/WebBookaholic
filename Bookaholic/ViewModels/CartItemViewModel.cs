using Bookaholic.Models;
using System.Collections.Generic;
using System.Linq;

namespace Bookaholic.ViewModels
{
    public class CartItemViewModel
    {
        public int CartItemId { get; set; }
        public Book Book { get; set; }
        public int Quantity { get; set; }

        // Tính tổng giá cho từng mặt hàng
        public decimal Subtotal => Book.OriginalPrice * Quantity;
    }

    public class CartViewModel
    {
        public List<CartItemViewModel> Items { get; set; } = new List<CartItemViewModel>();

        // Tổng cộng toàn bộ giỏ hàng
        public decimal Total => Items.Sum(i => i.Subtotal);
    }
}