using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Bookaholic.Data;
using Bookaholic.Models;

namespace Bookaholic.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Orders
        public async Task<IActionResult> Index(string search, int page = 1)
        {
            int pageSize = 10;

            var query = _context.Orders
                .Include(o => o.User)
                .Include(o => o.Voucher)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(o => (o.User.FirstName + " " + o.User.LastName).Contains(search));
            }

            query = query.OrderByDescending(o => o.OrderDate);

            int totalOrders = await query.CountAsync();

            var orders = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalOrders / pageSize);
            ViewBag.Search = search;

            return View(orders);
        }


        // GET: Admin/Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Voucher)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Book)
                .FirstOrDefaultAsync(m => m.OrderId == id);

            if (order == null)
                return NotFound();

            return View(order);
        }

        // GET: Admin/Orders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var order = await _context.Orders.FindAsync(id);
            if (order == null)
                return NotFound();

            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Email", order.UserId);
            ViewData["VoucherId"] = new SelectList(_context.Vouchers, "VoucherId", "Code", order.VoucherId);
            ViewBag.OrderStatusList = new SelectList(new[]
            {
                "Chờ xác nhận", "Đã xác nhận", "Đang giao", "Hoàn tất", "Đã hủy"
            }, order.OrderStatus);

            return View(order);
        }

        // POST: Admin/Orders/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OrderId,UserId,FullName,Phone,Address,OrderDate,ShippingAddress,ShippingFee,VoucherId,DiscountAmount,TotalAmount,PaymentMethod,PaymentStatus,OrderStatus,Note")] Order order)
        {
            if (id != order.OrderId)
                return NotFound();

           
            ModelState.Remove("User");
            ModelState.Remove("Voucher");
            ModelState.Remove("OrderDetails");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(order.OrderId))
                        return NotFound();
                    else
                        throw;
                }
            }
         
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Email", order.UserId);
            ViewData["VoucherId"] = new SelectList(_context.Vouchers, "VoucherId", "Code", order.VoucherId);
            ViewBag.OrderStatusList = new SelectList(new[]
            {
        "Chờ xác nhận", "Đã xác nhận", "Đang giao", "Hoàn tất", "Đã hủy"
    }, order.OrderStatus);

            return View(order);
        }

        // GET: Admin/Orders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Voucher)
                .FirstOrDefaultAsync(m => m.OrderId == id);

            if (order == null)
                return NotFound();

            return View(order);
        }

        // POST: Admin/Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

          
            if (order.OrderStatus != "Hoàn tất")
            {
                TempData["Error"] = "❗ Chỉ được xoá đơn hàng đã hoàn tất.";
                return RedirectToAction(nameof(Index));
            }

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            TempData["Success"] = "🗑️ Đơn hàng đã được xoá.";
            return RedirectToAction(nameof(Index));
        }


        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.OrderId == id);
        }
    }
}
