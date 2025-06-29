using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Bookaholic.Data;
using Bookaholic.Models;

namespace Bookaholic.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ContactsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ContactsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string search, DateTime? fromDate, DateTime? toDate, int page = 1)
        {
            int pageSize = 10;

            var query = _context.Contacts.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c => c.FullName.Contains(search) || c.Email.Contains(search));
            }

            if (fromDate.HasValue)
                query = query.Where(c => c.SentAt >= fromDate.Value);

            if (toDate.HasValue)
            {
                var inclusiveToDate = toDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(c => c.SentAt <= inclusiveToDate);
            }

            int totalContacts = await query.CountAsync();

            var contacts = await query
                .OrderByDescending(c => c.SentAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalContacts / pageSize);
            ViewBag.Search = search;
            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");

            return View(contacts);
        }


        public async Task<IActionResult> Details(int id)
        {
            var contact = await _context.Contacts.FindAsync(id);
            if (contact == null) return NotFound();

            return View(contact);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteOld(DateTime? fromDate, DateTime? toDate, string search)
        {
            var query = _context.Contacts.AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(c => c.FullName.Contains(search) || c.Email.Contains(search));

            if (fromDate.HasValue)
                query = query.Where(c => c.SentAt >= fromDate.Value);

            if (toDate.HasValue)
            {
                var inclusiveToDate = toDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(c => c.SentAt <= inclusiveToDate);
            }

            _context.Contacts.RemoveRange(query);
            await _context.SaveChangesAsync();

            TempData["Message"] = "🧹 Đã xoá các liên hệ theo bộ lọc hiện tại!";
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var contact = await _context.Contacts.FindAsync(id);
            if (contact == null)
            {
                TempData["Message"] = "⚠️ Không tìm thấy liên hệ cần xoá.";
                return RedirectToAction(nameof(Index));
            }

            _context.Contacts.Remove(contact);
            await _context.SaveChangesAsync();

            TempData["Message"] = "🗑 Đã xoá liên hệ thành công.";
            return RedirectToAction(nameof(Index));
        }
    }
}
