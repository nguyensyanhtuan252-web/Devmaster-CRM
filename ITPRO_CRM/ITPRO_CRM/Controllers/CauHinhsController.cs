using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ITPRO_CRM.Data;
using ITPRO_CRM.Models;

namespace ITPRO_CRM.Controllers
{
    public class CauHinhsController : Controller
    {
        private readonly ITPRO_CRMContext _context;

        public CauHinhsController(ITPRO_CRMContext context)
        {
            _context = context;
        }

        // Trang cấu hình chính (Tất cả dồn về đây)
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

        // Xử lý lưu cấu hình ngay tại Index
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(int id, CauHinh cauHinh)
        {
            if (id != cauHinh.Id) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(cauHinh);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Cập nhật cấu hình thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(cauHinh);
        }
    }
}