using Bookaholic.Data;
using Bookaholic.Models;
using Bookaholic.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Bookaholic.Controllers
{
    public class BooksController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BooksController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? categoryId, int page = 1, string sort = "default", string? search = null)
        {
            int pageSize = 8;

            var booksQuery = _context.Books
                .Include(b => b.Author)
                .Include(b => b.Category)
                .AsQueryable();

           
            if (!string.IsNullOrEmpty(search))
            {
                booksQuery = booksQuery.Where(b => b.Title.Contains(search));
            }

            
            if (categoryId.HasValue)
            {
                booksQuery = booksQuery.Where(b => b.CategoryId == categoryId.Value);
            }

            
            booksQuery = sort switch
            {
                "newest" => booksQuery.OrderByDescending(b => b.CreatedAt),
                "priceAsc" => booksQuery.OrderBy(b => b.OriginalPrice),
                "priceDesc" => booksQuery.OrderByDescending(b => b.OriginalPrice),
                _ => booksQuery.OrderBy(b => b.BookId),
            };

           
            int totalBooks = await booksQuery.CountAsync();
            var books = await booksQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var categories = await _context.Categories.ToListAsync();
            string categoryName = categoryId.HasValue
                ? categories.FirstOrDefault(c => c.CategoryId == categoryId)?.CategoryName ?? "Danh sách Sách"
                : "Tất cả Sách";

            var viewModel = new BookListViewModel
            {
                Books = books,
                Categories = categories,
                SelectedCategoryId = categoryId,
                CurrentCategoryName = categoryName
            };

            ViewData["Sort"] = sort;
            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = (int)Math.Ceiling((double)totalBooks / pageSize);

            return View(viewModel);
        }


        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var book = await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Category)
                .FirstOrDefaultAsync(m => m.BookId == id);

            if (book == null) return NotFound();

            return View(book);
        }      
    }
}
