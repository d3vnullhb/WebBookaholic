using Bookaholic.Data;
using Bookaholic.Models;
using Bookaholic.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Bookaholic.Controllers
{
    [Authorize]
    public class CartItemsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartItemsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddToCart(int bookId)
        {
            var userId = _userManager.GetUserId(User);

            var existingItem = await _context.CartItems
                .FirstOrDefaultAsync(c => c.BookId == bookId && c.UserId == userId);

            if (existingItem != null)
            {
                existingItem.Quantity++;
                _context.Update(existingItem);
            }
            else
            {
                var newItem = new CartItem
                {
                    BookId = bookId,
                    UserId = userId,
                    Quantity = 1,
                    CreatedAt = DateTime.Now
                };
                _context.Add(newItem);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("ViewCart");
        }
        public async Task<IActionResult> ViewCart()
        {
            var userId = _userManager.GetUserId(User);
            var cartItems = await _context.CartItems
                .Include(c => c.Book)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            var viewModel = new CartViewModel
            {
                Items = cartItems.Select(c => new CartItemViewModel
                {
                    CartItemId = c.CartItemId,
                    Book = c.Book,
                    Quantity = c.Quantity
                }).ToList()
            };

            return View("Index", viewModel);
        }
   
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int cartItemId, int change)
        {
            var item = await _context.CartItems.FindAsync(cartItemId);
            var userId = _userManager.GetUserId(User);

            if (item != null && item.UserId == userId)
            {
                item.Quantity += change;
                if (item.Quantity <= 0)
                {
                    _context.CartItems.Remove(item);
                }
                else
                {
                    item.CreatedAt = DateTime.Now;
                    _context.Update(item);
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(ViewCart));
        }

        [HttpPost]
        public async Task<IActionResult> Remove(int cartItemId)
        {
            var item = await _context.CartItems.FindAsync(cartItemId);
            var userId = _userManager.GetUserId(User);

            if (item != null && item.UserId == userId)
            {
                _context.CartItems.Remove(item);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(ViewCart));
        }
    }
}
