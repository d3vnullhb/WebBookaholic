using Microsoft.AspNetCore.Mvc;
using Bookaholic.Data;
using Bookaholic.Models;

namespace Bookaholic.Controllers
{
    public class ContactsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ContactsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Contacts
        public IActionResult Index()
        {
            return View();
        }

        // POST: /Contacts
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(Contact contact)
        {
            if (ModelState.IsValid)
            {
                contact.SentAt = DateTime.Now;
                _context.Contacts.Add(contact);
                await _context.SaveChangesAsync();

                ViewBag.Message = "✅ Gửi liên hệ thành công!";
                ViewBag.FormSubmitted = true;
                ModelState.Clear(); 
            }

            return View();
        }
    }
}
