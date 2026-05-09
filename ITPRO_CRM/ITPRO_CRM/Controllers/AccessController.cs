using ITPRO_CRM.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ITPRO_CRM.Controllers
{
    public class AccessController : Controller
    {
        private readonly ITPRO_CRMContext _context;

        public AccessController(ITPRO_CRMContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("UserName") != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string user, string pass)
        {
            var nhanVien = await _context.NhanVien
                                .FirstOrDefaultAsync(n => n.Email == user && n.MatKhau == pass);

            if (nhanVien != null)
            {
                if (nhanVien.TrangThai == false)
                {
                    ViewBag.Error = "Tài khoản này đã bị khóa!";
                    return View();
                }

                // 1. Lưu Email (Dùng để tìm kiếm trong HomeController)
                HttpContext.Session.SetString("UserName", nhanVien.Email);

                // 2. Lưu ID (Để đếm KPI cá nhân và Lead cá nhân)
                HttpContext.Session.SetInt32("UserId", nhanVien.Id);

                // 3. Lưu vai trò
                HttpContext.Session.SetInt32("VaiTro", nhanVien.VaiTro);

                // ✅ THÊM MỚI: Lưu tên hiển thị và vai trò cho Layout
                HttpContext.Session.SetString("HoTen", nhanVien.HoTen);
                HttpContext.Session.SetString("UserRole", nhanVien.VaiTro == 0 ? "Quản trị viên" : "Nhân viên Sale");

                return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewBag.Error = "Email hoặc mật khẩu không chính xác!";
                ViewBag.User = user;
                return View();
            }
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}