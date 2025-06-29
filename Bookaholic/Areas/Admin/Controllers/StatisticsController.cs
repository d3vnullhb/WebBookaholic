using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Bookaholic.Data;
using System;
using System.Linq;

namespace Bookaholic.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class StatisticsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StatisticsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Tổng số đơn hàng
            var totalOrders = _context.Orders.Count();

            // Tổng doanh thu (bỏ qua đơn bị huỷ)
            var totalRevenue = _context.Orders
                .Where(o => o.OrderStatus != "Đã hủy")
                .Sum(o => o.TotalAmount);

            // Số đơn theo trạng thái
            var orderStatusCounts = _context.Orders
                .GroupBy(o => o.OrderStatus)
                .Select(g => new
                {
                    Status = g.Key,
                    Count = g.Count()
                }).ToList();

            // Top 5 sách bán chạy theo số lượng
            var topBooks = _context.OrderDetails
                .Include(od => od.Book)
                .GroupBy(od => od.Book.Title)
                .Select(g => new
                {
                    BookTitle = g.Key,
                    TotalSold = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(x => x.TotalSold)
                .Take(5)
                .ToList();

            // Doanh thu theo tháng (năm hiện tại)
            var revenueByMonth = _context.Orders
                .Where(o => o.OrderStatus != "Đã hủy" && o.OrderDate.Year == DateTime.Now.Year)
                .GroupBy(o => o.OrderDate.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    Total = g.Sum(o => o.TotalAmount)
                })
                .OrderBy(g => g.Month)
                .ToList();

            // Gán dữ liệu sang ViewBag
            ViewBag.TotalOrders = totalOrders;
            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.OrderStatusCounts = orderStatusCounts;
            ViewBag.TopBooks = topBooks;
            ViewBag.RevenueByMonth = revenueByMonth;

            return View();
        }
    }
}
