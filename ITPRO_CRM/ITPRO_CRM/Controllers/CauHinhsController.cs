using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ITPRO_CRM.Data;
using ITPRO_CRM.Models;
using ITPRO_CRM.Filters; // Để dùng được ổ khóa [PhanQuyen]

namespace ITPRO_CRM.Controllers
{
    public class CauHinhsController : Controller
    {
        private readonly ITPRO_CRMContext _context;

        public CauHinhsController(ITPRO_CRMContext context)
        {
            _context = context;
        }

        // 1. TRANG XEM CẤU HÌNH: Ai cũng vào được
        public async Task<IActionResult> Index()
        {
            var cauHinh = await _context.CauHinh.FirstOrDefaultAsync();
            if (cauHinh == null)
            {
                cauHinh = new CauHinh { TenTrungTam = "ITPRO ACADEMY" };
                _context.CauHinh.Add(cauHinh);
                await _context.SaveChangesAsync();
            }
            return View(cauHinh);
        }

        // 2. XỬ LÝ LƯU CẤU HÌNH: Chỉ Admin mới được phép
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PhanQuyen(LoaiVaiTro.Admin)] // Ổ khóa chặn các quyền khác ở bước lưu
        public async Task<IActionResult> Index(int id, CauHinh cauHinh)
        {
            if (id != cauHinh.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cauHinh);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Đã cập nhật cấu hình hệ thống thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.CauHinh.Any(e => e.Id == cauHinh.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            // Nếu có lỗi dữ liệu, trả về trang hiện tại để hiện thông báo lỗi
            return View(cauHinh);
        }
    }
}