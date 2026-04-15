using ITPRO_CRM.Data;
using ITPRO_CRM.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ITPRO_CRM.Controllers
{
    public class ProfileController : Controller
    {
        private readonly ITPRO_CRMContext _context;

        public ProfileController(ITPRO_CRMContext context)
        {
            _context = context;
        }

        // 1. XEM VÀ CẬP NHẬT HỒ SƠ
        public async Task<IActionResult> Index()
        {
            var userName = HttpContext.Session.GetString("UserName");
            if (userName == null) return RedirectToAction("Login", "Access");

            // Tìm nhân viên theo Email
            var nhanVien = await _context.NhanVien.FirstOrDefaultAsync(n => n.Email == userName);

            if (nhanVien == null) return NotFound("Không tìm thấy thông tin nhân viên.");

            return View(nhanVien);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(NhanVien model)
        {
            var user = await _context.NhanVien.FindAsync(model.Id);
            if (user != null)
            {
                // Cập nhật các thông tin có trong Model của bạn
                user.HoTen = model.HoTen;           // Sửa TenNhanVien -> HoTen
                user.SoDienThoai = model.SoDienThoai;
                // Bỏ qua DiaChi vì model không có

                _context.Update(user);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Cập nhật hồ sơ thành công!";
                return RedirectToAction("Index");
            }
            return View("Index", model);
        }

        // 2. ĐỔI MẬT KHẨU
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(string MatKhauCu, string MatKhauMoi, string XacNhanMatKhau)
        {
            var userName = HttpContext.Session.GetString("UserName");
            var user = await _context.NhanVien.FirstOrDefaultAsync(n => n.Email == userName);

            if (user == null) return RedirectToAction("Login", "Access");

            // Kiểm tra mật khẩu cũ
            if (user.MatKhau != MatKhauCu)
            {
                TempData["Error"] = "Mật khẩu cũ không chính xác!";
                return View();
            }

            // Kiểm tra xác nhận mật khẩu
            if (MatKhauMoi != XacNhanMatKhau)
            {
                TempData["Error"] = "Mật khẩu xác nhận không khớp!";
                return View();
            }

            // Đổi mật khẩu
            user.MatKhau = MatKhauMoi;
            _context.Update(user);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đổi mật khẩu thành công!";
            return View();
        }
    }
}