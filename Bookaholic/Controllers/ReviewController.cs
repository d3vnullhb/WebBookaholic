using Bookaholic.Data;
using Bookaholic.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using X.PagedList;
using X.PagedList.Extensions;

namespace Bookaholic.Controllers
{
    public class ReviewController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReviewController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Review?page=1
        public async Task<IActionResult> Index(int? page)
        {
            int pageSize = 9;
            int pageNumber = page ?? 1;

            var reviews = _context.BookReviews
                .Where(r => r.IsPublished)
                .Include(r => r.Book)
                .Include(r => r.User)
                .OrderByDescending(r => r.CreatedAt)
                .AsQueryable();

            var pagedReviews = reviews.ToPagedList(pageNumber, pageSize); 
            return View(pagedReviews);
        }
        // GET: /Review/Detail/5
        public async Task<IActionResult> Details(int id)
        {
            var review = await _context.BookReviews
                .Include(r => r.Book)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.ReviewId == id && r.IsPublished);

            if (review == null)
                return NotFound();

            return View(review);
        }
    }
}
