using Bookaholic.Data;
using Bookaholic.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Bookaholic.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class BookReviewsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public BookReviewsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
        }

        // GET: Admin/BookReviews
        public async Task<IActionResult> Index(int page = 1)
        {
            int pageSize = 10;

            var query = _context.BookReviews
                .Include(r => r.Book)
                .Include(r => r.User)
                .OrderByDescending(r => r.CreatedAt);

            int totalReviews = await query.CountAsync();
            var reviews = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalReviews / pageSize);

            return View(reviews);
        }


        // GET: Admin/BookReviews/Create
        public IActionResult Create()
        {
            ViewBag.BookId = new SelectList(_context.Books, "BookId", "Title");
            return View();
        }

        // POST: Admin/BookReviews/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookReview review)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                ModelState.AddModelError("UserId", "Không xác định được người dùng.");
            }
            else
            {
                review.UserId = user.Id;
                ModelState.Remove("UserId"); // ✅ FIX lỗi required khi binding
            }

            if (review.BookId == 0)
            {
                ModelState.AddModelError("BookId", "Vui lòng chọn sách.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.BookId = new SelectList(_context.Books, "BookId", "Title", review.BookId);
                return View(review);
            }

            var folderPath = Path.Combine(_env.WebRootPath, "images/reviews");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            if (review.ImageFile != null && review.ImageFile.Length > 0)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(review.ImageFile.FileName);
                var path = Path.Combine(folderPath, fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await review.ImageFile.CopyToAsync(stream);
                }

                review.ImageUrl = "/images/reviews/" + fileName;
            }

            review.CreatedAt = DateTime.Now;

            _context.BookReviews.Add(review);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/BookReviews/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var review = await _context.BookReviews.FindAsync(id);
            if (review == null) return NotFound();

            ViewBag.BookId = new SelectList(_context.Books, "BookId", "Title", review.BookId);
            return View(review);
        }

        // POST: Admin/BookReviews/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BookReview review)
        {
            if (id != review.ReviewId) return NotFound();

            var existing = await _context.BookReviews.FindAsync(id);
            if (existing == null) return NotFound();

            // ⚠️ Gán lại UserId để tránh lỗi Required
            review.UserId = existing.UserId;
            ModelState.Remove("UserId");

            if (!ModelState.IsValid)
            {
                ViewBag.BookId = new SelectList(_context.Books, "BookId", "Title", review.BookId);
                return View(review);
            }

            // Cập nhật dữ liệu
            existing.Title = review.Title;
            existing.Content = review.Content;
            existing.BookId = review.BookId;
            existing.IsPublished = review.IsPublished;

            if (review.ImageFile != null && review.ImageFile.Length > 0)
            {
                var folderPath = Path.Combine(_env.WebRootPath, "images/reviews");
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                var fileName = Path.GetFileName(review.ImageFile.FileName);
                var path = Path.Combine(folderPath, fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await review.ImageFile.CopyToAsync(stream);
                }

               
                existing.ImageUrl = "/images/reviews/" + fileName;
            }



            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        // GET: Admin/BookReviews/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var review = await _context.BookReviews
                .Include(r => r.Book)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.ReviewId == id);

            if (review == null) return NotFound();

            return View(review);
        }

        // GET: Admin/BookReviews/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var review = await _context.BookReviews
                .Include(r => r.Book)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.ReviewId == id);

            if (review == null) return NotFound();

            return View(review);
        }

        // POST: Admin/BookReviews/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var review = await _context.BookReviews.FindAsync(id);
            if (review != null)
            {
                if (!string.IsNullOrEmpty(review.ImageUrl))
                {
                    var filePath = Path.Combine(_env.WebRootPath, review.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                        System.IO.File.Delete(filePath);
                }

                _context.BookReviews.Remove(review);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
