using Bookaholic.Data;
using Bookaholic.Models;
using Bookaholic.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ===== 1. Kết nối DB
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// ===== 2. Đăng ký Identity  
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// ===== 3. Thêm MVC + Razor Pages 
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IVnPayService, VnPayService>();
builder.Services.AddRazorPages();
var app = builder.Build();

// ===== 4. Tạo role và tài khoản mặc định
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    string[] roles = { "Admin", "User" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    // --- Admin mặc định ---
    string adminEmail = "admin@bookaholic.vn";
    string adminPassword = "Admin123@";
    var admin = await userManager.FindByEmailAsync(adminEmail);
    if (admin == null)
    {
        var newAdmin = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FirstName = "Admin",
            LastName = "Bookaholic"
        };
        var result = await userManager.CreateAsync(newAdmin, adminPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(newAdmin, "Admin");
        }
    }

    // --- User mặc định ---
    string userEmail = "user@bookaholic.vn";
    string userPassword = "User123@";
    var user = await userManager.FindByEmailAsync(userEmail);
    if (user == null)
    {
        var newUser = new ApplicationUser
        {
            UserName = userEmail,
            Email = userEmail,
            FirstName = "User",
            LastName = "Demo"
        };
        var result = await userManager.CreateAsync(newUser, userPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(newUser, "User");
        }
    }
}

    // ===== 5. Cấu hình HTTP pipeline
    if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// ===== 6. Cấu hình route
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"); 
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
