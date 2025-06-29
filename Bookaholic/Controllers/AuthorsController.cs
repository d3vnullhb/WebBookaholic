using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Bookaholic.Data;

namespace Bookaholic.Controllers
{
    public class AuthorsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuthorsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Authors
        public async Task<IActionResult> Index(string startsWith, int page = 1)
        {
            int pageSize = 6;

            var query = _context.Authors.AsQueryable();

            if (!string.IsNullOrEmpty(startsWith))
            {
                query = query.Where(a => a.Name.StartsWith(startsWith));
            }

            int totalAuthors = await query.CountAsync();
            var authors = await query
                .OrderBy(a => a.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalAuthors / pageSize);
            ViewBag.StartsWith = startsWith;

            return View(authors);
        }

        // GET: /Authors/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var author = await _context.Authors
                .Include(a => a.Books) 
                .FirstOrDefaultAsync(a => a.AuthorId == id);

            if (author == null) return NotFound();

            return View(author);
        }
    }
}
