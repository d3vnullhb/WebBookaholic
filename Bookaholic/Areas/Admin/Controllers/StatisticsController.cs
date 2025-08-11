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

        public IActionResult Index(DateTime? fromDate, DateTime? toDate)
        {
            var orders = _context.Orders
                .Where(o => o.OrderStatus != "Đã hủy")
                .AsQueryable();

           
            if (fromDate.HasValue)
                orders = orders.Where(o => o.OrderDate >= fromDate.Value);

            if (toDate.HasValue)
                orders = orders.Where(o => o.OrderDate <= toDate.Value);

            
            var totalOrders = orders.Count();

            
            var totalRevenue = orders
                     .Where(o => o.OrderStatus == "Hoàn tất") 
                     .Sum(o => o.TotalAmount);

           
            var orderStatusCounts = orders
                .GroupBy(o => o.OrderStatus)
                .Select(g => new
                {
                    Status = g.Key,
                    Count = g.Count()
                }).ToList();

            
            var topBooks = _context.OrderDetails
                .Include(od => od.Book)
                .Where(od => orders.Select(o => o.OrderId).Contains(od.OrderId))
                .GroupBy(od => od.Book.Title)
                .Select(g => new
                {
                    BookTitle = g.Key,
                    TotalSold = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(x => x.TotalSold)
                .Take(5)
                .ToList();

         
            ViewBag.TotalOrders = totalOrders;
            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.OrderStatusCounts = orderStatusCounts;
            ViewBag.TopBooks = topBooks;

            return View();
        }
    }
}
