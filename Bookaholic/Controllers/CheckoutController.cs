using Bookaholic.Data;
using Bookaholic.Models;
using Bookaholic.Services;
using Bookaholic.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bookaholic.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IVnPayService _vnPayService;
        private readonly UserManager<ApplicationUser> _userManager;

        public CheckoutController(ApplicationDbContext context, IVnPayService vnPayService, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _vnPayService = vnPayService;
            _userManager = userManager;
        }

        // GET: /Checkout
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var userId = _userManager.GetUserId(User);

            var cartItems = await _context.CartItems
                .Include(c => c.Book)
                .Where(c => c.UserId == userId)
                .ToListAsync();
            var defaultAddress = await _context.UserAddresses
                .Where(a => a.UserId == userId && a.IsDefault)
                .FirstOrDefaultAsync();

            var viewModel = new CheckOutViewModel
            {
                Email = user.Email,
                FullName = user.FirstName + " " + user.LastName,
                Phone = user.PhoneNumber,
                Address = defaultAddress?.Detail,
                Items = cartItems.Select(c => new CartItemViewModel
                {
                    Book = c.Book,
                    Quantity = c.Quantity,
                    CartItemId = c.CartItemId
                }).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> PlaceOrder(CheckOutViewModel model)
        {
            var userId = _userManager.GetUserId(User);

            var cartItems = await _context.CartItems
                .Include(c => c.Book)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            if (!cartItems.Any())
                return RedirectToAction("ViewCart", "CartItems");

            model.Items = cartItems.Select(c => new CartItemViewModel
            {
                Book = c.Book,
                Quantity = c.Quantity,
                CartItemId = c.CartItemId
            }).ToList();

            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            if (model.PaymentMethod == "COD")
            {
                var order = new Order
                {
                    UserId = userId,
                    FullName = model.FullName,
                    Address = model.Address,
                    Phone = model.Phone,
                    Note = model.Note,
                    OrderDate = DateTime.Now,
                    PaymentMethod = "COD",
                    PaymentStatus = "Pending",
                    OrderStatus = "Chưa xác nhận",
                    ShippingFee = model.ShippingFee,
                    TotalAmount = model.GrandTotal
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                foreach (var item in cartItems)
                {
                    _context.OrderDetails.Add(new OrderDetail
                    {
                        OrderId = order.OrderId,
                        BookId = item.BookId,
                        Quantity = item.Quantity,
                        UnitPrice = (decimal)item.Book.OriginalPrice
                    });
                }

                _context.CartItems.RemoveRange(cartItems);
                await _context.SaveChangesAsync();

                var response = new PaymentResponseModel
                {
                    Success = true,
                    PaymentMethod = "COD",
                    OrderDescription = $"Thanh toán khi nhận hàng - Đơn hàng của {model.FullName}",
                    OrderId = order.OrderId.ToString()
                };

                return RedirectToAction("PaymentResult", response);
            }

            else if (model.PaymentMethod == "VNPAY")
            {              
                TempData["FullName"] = model.FullName;
                TempData["ShippingPhone"] = model.Phone;
                TempData["ShippingAddress"] = model.Address;
                TempData["Note"] = model.Note;
                TempData["ShippingFee"] = model.ShippingFee;

                var vnpModel = new PaymentInformationModel
                {
                    Amount = (double)model.GrandTotal,
                    OrderDescription = $"Thanh toán đơn hàng của {model.FullName}",
                    OrderType = "billpayment",
                    Name = model.FullName
                };

                var paymentUrl = _vnPayService.CreatePaymentUrl(vnpModel, HttpContext);
                return Redirect(paymentUrl);
            }

            return View("Index", model);
        }

        // POST: /Checkout/CreatePayment
        [HttpPost]
        public IActionResult CreatePayment(PaymentInformationModel model)
        {
            var url = _vnPayService.CreatePaymentUrl(model, HttpContext);
            return Redirect(url);
        }
        public IActionResult PaymentResult(PaymentResponseModel? response = null)
        {
            if (response == null)
            {
                ViewBag.Message = TempData["Success"] ?? "Đặt hàng thành công!";
                response = new PaymentResponseModel
                {
                    Success = true
                };
            }
            else
            {
                if (response.Success)
                    ViewBag.Message = "✅ Thanh toán thành công!";
                else
                    ViewBag.Message = "❌ Thanh toán thất bại!";
            }
            return View(response);
        }
        // GET: /Checkout/PaymentReturn
        [HttpGet]
        public async Task<IActionResult> PaymentReturn()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);
            var userId = _userManager.GetUserId(User);

            if (response.Success)
            {
               
                var fullName = TempData["FullName"]?.ToString() ?? "Không rõ";
                var address = TempData["ShippingAddress"]?.ToString() ?? "Không rõ";
                var phone = TempData["ShippingPhone"]?.ToString() ?? "Không rõ";
                var note = TempData["Note"]?.ToString();
                var shippingFee = Convert.ToInt32(TempData["ShippingFee"] ?? 20000);

               
                var cartItems = await _context.CartItems
                    .Include(c => c.Book)
                    .Where(c => c.UserId == userId)
                    .ToListAsync();

                var total = cartItems.Sum(x => x.Book.Price * x.Quantity);

               
                var order = new Order
                {
                    UserId = userId,
                    FullName = fullName,
                    Address = address,
                    Phone = phone,
                    Note = note,
                    OrderDate = DateTime.Now,
                    PaymentMethod = "VNPAY",
                    PaymentStatus = "Paid",
                    OrderStatus = "Chưa xác nhận",
                    ShippingFee = shippingFee,
                    TotalAmount = (total ?? 0) + shippingFee
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                
                foreach (var item in cartItems)
                {
                    _context.OrderDetails.Add(new OrderDetail
                    {
                        OrderId = order.OrderId,
                        BookId = item.BookId,
                        Quantity = item.Quantity,
                        UnitPrice = item.Book.OriginalPrice
                    });
                }

                
                _context.CartItems.RemoveRange(cartItems);
                await _context.SaveChangesAsync();

                ViewBag.Message = "✅ Thanh toán thành công!";
            }
            else
            {
                ViewBag.Message = "❌ Thanh toán thất bại!";
            }

            return View("PaymentResult", response);
        }
    }
}
