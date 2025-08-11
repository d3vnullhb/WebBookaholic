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
       
        var userId = _userManager.GetUserId(HttpContext.User);

       
        if (string.IsNullOrEmpty(userId))
        {
            return View(0);
        }

       
        var count = await Task.Run(() =>
            _context.CartItems
                .Where(c => c.UserId == userId)
                .Sum(c => (int?)c.Quantity) ?? 0);

        return View(count);
    }
}
