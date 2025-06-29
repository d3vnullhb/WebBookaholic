using Bookaholic.Data;
using Bookaholic.Models;
using Bookaholic.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using static NuGet.Packaging.PackagingConstants;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ApplicationDbContext _context;
    public AccountController(UserManager<ApplicationUser> userManager,
                             SignInManager<ApplicationUser> signInManager,
                             RoleManager<IdentityRole> roleManager,
                             ApplicationDbContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _context = context;
    }

    // ===== REGISTER =====
    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                FirstName = model.FirstName,
                LastName = model.LastName
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {                
                if (!await _roleManager.RoleExistsAsync("User"))
                    await _roleManager.CreateAsync(new IdentityRole("User"));

                await _userManager.AddToRoleAsync(user, "User");
                await _signInManager.SignInAsync(user, false);

                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
    }

    // ===== LOGIN =====
    [HttpGet]
    public IActionResult Login(string returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user != null)
        {        
            if (await _userManager.IsLockedOutAsync(user))
            {
                ModelState.AddModelError(string.Empty, "Tài khoản của bạn đã bị khóa.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
              user.UserName,
              model.Password,
              model.RememberMe,
              lockoutOnFailure: false);
            if (result.Succeeded)
            {
                if (await _userManager.IsInRoleAsync(user, "Admin"))
                    return RedirectToAction("Dashboard", "Admin");

                return RedirectToLocal(returnUrl);
            }
        }     
        ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng.");
        return View(model);
    }


    private IActionResult RedirectToLocal(string returnUrl)
    {
        if (Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);
        return RedirectToAction("Index", "Home");
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }
    [HttpGet]
    public async Task<IActionResult> Info()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return RedirectToAction("Login");

        return View(user);
    }
    [HttpGet]
    public async Task<IActionResult> EditInfo()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return RedirectToAction("Login");

        return View(user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditInfo(ApplicationUser updatedUser)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return RedirectToAction("Login");

       
        user.FirstName = updatedUser.FirstName;
        user.LastName = updatedUser.LastName;
        user.PhoneNumber = updatedUser.PhoneNumber;

        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded)
        {
            TempData["Success"] = "✅ Cập nhật thông tin thành công.";
            return RedirectToAction("Info");
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);

        return View(updatedUser);
    }

    [HttpGet]
    public async Task<IActionResult> MyOrders()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return RedirectToAction("Login");

        var orders = await _context.Orders
            .Where(o => o.UserId == user.Id)
            .OrderByDescending(o => o.OrderDate)
            .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Book)
            .ToListAsync();

        return View("MyOrder", orders);
    }
    [Authorize]
    public async Task<IActionResult> OrderDetails(int id)
    {
        var user = await _userManager.GetUserAsync(User);

        var order = await _context.Orders
            .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Book)
           .FirstOrDefaultAsync(o => o.OrderId == id && o.UserId == user.Id);

        if (order == null)
        {
            return NotFound();
        }

        return View("OrderDetails", order);
    }

    [HttpGet]
    public IActionResult ChangePassword()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return RedirectToAction("Login");

        var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);

        if (result.Succeeded)
        {
            await _signInManager.RefreshSignInAsync(user);
            TempData["Success"] = "✅ Đổi mật khẩu thành công.";
            return RedirectToAction("Info");
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);

        return View(model);
    }
    public async Task<IActionResult> Addresses()
    {
        var userId = _userManager.GetUserId(User);
        var addresses = await _context.UserAddresses
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.IsDefault)
            .ToListAsync();
        return View(addresses);
    }

    // GET: Thêm địa chỉ mới
    public IActionResult CreateAddress()
    {
        return View();
    }

    // POST: Thêm địa chỉ mới
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateAddress(UserAddress address)
    {
        address.UserId = _userManager.GetUserId(User);

        if (address.IsDefault)
        {
            var currentDefaults = _context.UserAddresses
                .Where(a => a.UserId == address.UserId && a.IsDefault);
            foreach (var a in currentDefaults)
                a.IsDefault = false;
        }

        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors);
            foreach (var err in errors)
            {
                Console.WriteLine("❌ Validation Error: " + err.ErrorMessage);
            }
            return View(address);
        }

        _context.Add(address);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Addresses));
    }


    // GET: Chỉnh sửa địa chỉ
    public async Task<IActionResult> EditAddress(int id)
    {
        var userId = _userManager.GetUserId(User);
        var address = await _context.UserAddresses.FindAsync(id);
        if (address == null || address.UserId != userId)
            return NotFound();
        return View(address);
    }

    // POST: Chỉnh sửa địa chỉ
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditAddress(int id, [Bind("Id,Province,District,Ward,Detail,IsDefault")] UserAddress updated)
    {
        var userId = _userManager.GetUserId(User);
        var address = await _context.UserAddresses.FindAsync(id);
        if (address == null || address.UserId != userId)
            return NotFound();

        if (updated.IsDefault)
        {
            var currentDefaults = _context.UserAddresses
                .Where(a => a.UserId == userId && a.IsDefault);
            foreach (var a in currentDefaults)
                a.IsDefault = false;
        }

        if (ModelState.IsValid)
        {
            address.Province = updated.Province;
            address.District = updated.District;
            address.Ward = updated.Ward;
            address.Detail = updated.Detail;
            address.IsDefault = updated.IsDefault;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Addresses));
        }

        return View(updated);
    }

    // Xoá địa chỉ
    public async Task<IActionResult> DeleteAddress(int id)
    {
        var userId = _userManager.GetUserId(User);
        var address = await _context.UserAddresses.FindAsync(id);
        if (address == null || address.UserId != userId)
            return NotFound();

        _context.UserAddresses.Remove(address);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Addresses));
    }
}
