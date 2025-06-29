using Bookaholic.Data;
using Bookaholic.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

public class CartBadgeViewComponent : ViewComponent
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public CartBadgeViewComponent(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        // Lấy user hiện tại
        var userId = _userManager.GetUserId(HttpContext.User);

        // Nếu chưa đăng nhập thì trả 0
        if (string.IsNullOrEmpty(userId))
        {
            return View(0);
        }

        // Lấy tổng số lượng sp trong giỏ
        var count = await Task.Run(() =>
            _context.CartItems
                .Where(c => c.UserId == userId)
                .Sum(c => (int?)c.Quantity) ?? 0);

        return View(count);
    }
}
