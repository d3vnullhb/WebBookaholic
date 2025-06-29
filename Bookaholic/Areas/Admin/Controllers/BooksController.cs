using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Bookaholic.Data;
using Bookaholic.Models;
using Microsoft.AspNetCore.Authorization;

namespace Bookaholic.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class BooksController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BooksController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Books
        public async Task<IActionResult> Index(string search, int page = 1)
        {
            int pageSize = 10;

            var query = _context.Books
                .Include(b => b.Author)
                .Include(b => b.Category)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(b => b.Title.Contains(search));
            }

            int totalBooks = await query.CountAsync();
            var books = await query
                .OrderBy(b => b.Title)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalBooks / pageSize);
            ViewBag.Search = search;

            return View(books);
        }


        // GET: Admin/Books/Create
        public IActionResult Create()
        {
            LoadSelectLists();
            return View();
        }

        // POST: Admin/Books/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Book book)
        {
            if (!book.AuthorId.HasValue || !book.CategoryId.HasValue)
            {
                ModelState.AddModelError("", "Tác giả và thể loại là bắt buộc.");
            }

            if (!ModelState.IsValid)
            {
                LoadSelectLists(book.AuthorId, book.CategoryId);
                return View(book);
            }

            if (book.ImageFile != null && book.ImageFile.Length > 0)
            {
                var fileName = Path.GetFileName(book.ImageFile.FileName);
                var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/books");
                Directory.CreateDirectory(folder);
                var filePath = Path.Combine(folder, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await book.ImageFile.CopyToAsync(stream);

                book.ImageUrl = "/images/books/" + fileName;
            }

            book.CreatedAt = DateTime.Now;
            _context.Add(book);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Books/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound();

            LoadSelectLists(book.AuthorId, book.CategoryId);
            return View(book);
        }

        // POST: Admin/Books/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Book book)
        {
            if (id != book.BookId) return NotFound();

            if (!ModelState.IsValid)
            {
                LoadSelectLists(book.AuthorId, book.CategoryId);
                return View(book);
            }

            try
            {
                var existingBook = await _context.Books.FindAsync(id);
                if (existingBook == null) return NotFound();

                // Cập nhật thủ công
                existingBook.Title = book.Title;
                existingBook.AuthorId = book.AuthorId ?? 0;
                existingBook.Translator = book.Translator;
                existingBook.Publisher = book.Publisher;
                existingBook.Size = book.Size;
                existingBook.Pages = book.Pages;
                existingBook.PublishYear = book.PublishYear;
                existingBook.Price = book.Price;
                existingBook.OriginalPrice = book.OriginalPrice;
                existingBook.Quantity = book.Quantity;
                existingBook.Description = book.Description;
                existingBook.CategoryId = book.CategoryId ?? 0;

                if (book.ImageFile != null && book.ImageFile.Length > 0)
                {
                    var fileName = Path.GetFileName(book.ImageFile.FileName);
                    var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/books");
                    Directory.CreateDirectory(folder);
                    var filePath = Path.Combine(folder, fileName);

                    using var stream = new FileStream(filePath, FileMode.Create);
                    await book.ImageFile.CopyToAsync(stream);

                    existingBook.ImageUrl = "/images/books/" + fileName;
                }

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BookExists(book.BookId)) return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Books/Details/5
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

        // GET: Admin/Books/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var book = await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Category)
                .FirstOrDefaultAsync(m => m.BookId == id);

            if (book == null) return NotFound();

            return View(book);
        }

        // POST: Admin/Books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book != null)
            {
                _context.Books.Remove(book);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool BookExists(int id)
        {
            return _context.Books.Any(e => e.BookId == id);
        }

        private void LoadSelectLists(int? selectedAuthor = null, int? selectedCategory = null)
        {
            var authors = _context.Authors.ToList();
            var categories = _context.Categories.ToList();

            Console.WriteLine($"[DEBUG] Số tác giả: {authors.Count}");
            Console.WriteLine($"[DEBUG] Số thể loại: {categories.Count}");

            ViewBag.AuthorId = new SelectList(
                authors,
                nameof(Author.AuthorId),
                nameof(Author.Name),
                selectedAuthor
            );

            ViewBag.CategoryId = new SelectList(
                categories,
                nameof(Category.CategoryId),
                nameof(Category.CategoryName),
                selectedCategory
            );
        }
    }
}
