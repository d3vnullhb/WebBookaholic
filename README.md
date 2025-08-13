# Bookaholic

Website quản lý sách và người dùng, được xây dựng bằng **ASP.NET Core MVC** + **Entity Framework Core**.

## 🛠 Yêu cầu hệ thống
- **.NET 6.0 SDK** hoặc mới hơn
- **SQL Server** (bản Express hoặc bản đầy đủ)
- **Visual Studio 2022** 
- **SQL Server Management Studio (SSMS)** 

---

## 📥 Cài đặt và chạy dự án

```bash
# 1. Clone dự án từ GitHub
git clone <link_repo_github>

# 2.Giải nén và mở file Bookaholic.sln bằng Visual Studio

# 3. Sửa chuỗi kết nối trong appsettings.json cho phù hợp máy bạn
"ConnectionStrings": {
  "DefaultConnection": "Server=**Server của bạn**;Database=BookaholicDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True"
}
# 4. Restore file Backup

# 5. Nhấn F5 hoặc chọn IIS Express để chạy website

#    Sau đó truy cập: https://localhost:<port>
TK Admin: admin@bookaholic.vn MK: Admin123@
