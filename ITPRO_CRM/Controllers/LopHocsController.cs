using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ITPRO_CRM.Models; // Đảm bảo đúng tên Project của bạn
using ITPRO_CRM.Data;

namespace ITPRO_CRM.Controllers
{
    public class LopHocsController : Controller
    {
        private readonly ITPRO_CRMContext _context;

        public LopHocsController(ITPRO_CRMContext context)
        {
            _context = context;
        }

        // GET: LopHocs
        public async Task<IActionResult> Index()
        {
            // Include HocViens để đếm sĩ số ở trang Index
            return View(await _context.LopHoc.Include(l => l.HocViens).ToListAsync());
        }

        // GET: LopHocs/Details/5
        // GET: LopHocs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var lopHoc = await _context.LopHoc
                .Include(l => l.HocViens)
                // 👇 THÊM DÒNG NÀY: Để lấy lịch sử điểm danh ra xem
                .Include(l => l.DiemDanhs)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (lopHoc == null) return NotFound();

            return View(lopHoc);
        }

        // GET: LopHocs/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: LopHocs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TenLop,NgayKhaiGiang,NgayKetThuc,LichHoc,HocPhi,SiSoToiDa,TrangThai")] LopHoc lopHoc)
        {
            // Lưu ý: Đã xóa SiSoHienTai khỏi Bind vì nó là Read-only
            if (ModelState.IsValid)
            {
                _context.Add(lopHoc);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(lopHoc);
        }

        // GET: LopHocs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var lopHoc = await _context.LopHoc.FindAsync(id);
            if (lopHoc == null) return NotFound();
            return View(lopHoc);
        }

        // POST: LopHocs/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TenLop,NgayKhaiGiang,NgayKetThuc,LichHoc,HocPhi,SiSoToiDa,TrangThai")] LopHoc lopHoc)
        {
            if (id != lopHoc.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(lopHoc);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LopHocExists(lopHoc.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(lopHoc);
        }

        // GET: LopHocs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var lopHoc = await _context.LopHoc
                .FirstOrDefaultAsync(m => m.Id == id);
            if (lopHoc == null) return NotFound();

            return View(lopHoc);
        }

        // POST: LopHocs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var lopHoc = await _context.LopHoc.FindAsync(id);
            if (lopHoc != null)
            {
                _context.LopHoc.Remove(lopHoc);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LopHocExists(int id)
        {
            return _context.LopHoc.Any(e => e.Id == id);
        }
    }
}