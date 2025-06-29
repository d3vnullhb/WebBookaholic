using Bookaholic.Data;
using Bookaholic.Models;
using Bookaholic.ViewModels; 
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Bookaholic.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context; 

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context) 
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            var books = _context.Books
                .Include(b => b.Author)
                .OrderByDescending(b => b.CreatedAt)
                .Take(4)
                .ToList();

            var authors = _context.Authors
                .OrderByDescending(a => a.CreatedAt)
                .Take(6)
                .ToList();

            var model = new StoreViewModel
            {
                Books = books,
                Authors = authors
            };

            return View(model); 
        }
        public IActionResult About()
        {
            return View();
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
