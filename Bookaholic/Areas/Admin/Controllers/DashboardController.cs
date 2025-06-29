using Bookaholic.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bookaholic.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["TotalBooks"] = await _context.Books.CountAsync();
            ViewData["TotalOrders"] = await _context.Orders.CountAsync();
            ViewData["TotalUsers"] = await _context.Users.CountAsync();

            return View();
        }
    }
}
