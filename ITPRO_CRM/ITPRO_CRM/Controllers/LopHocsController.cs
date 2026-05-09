using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ITPRO_CRM.Models;
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

        // =========================================================
        // GET: LopHocs — Dashboard danh sách lớp (có lọc & tìm kiếm)
        // =========================================================
        public async Task<IActionResult> Index(string? search, int? trangThai, int? giangVienId, int? khoaHocId)
        {
            ViewBag.Search = search;
            ViewBag.TrangThai = trangThai;
            ViewBag.GiangVienId = giangVienId;
            ViewBag.KhoaHocId = khoaHocId;

            // Dropdown filters
            ViewBag.GiangViens = new SelectList(await _context.GiangVien.OrderBy(g => g.HoTen).ToListAsync(), "Id", "HoTen");
            ViewBag.KhoaHocs = new SelectList(await _context.KhoaHoc.OrderBy(k => k.TenKhoaHoc).ToListAsync(), "Id", "TenKhoaHoc");

            var query = _context.LopHoc
                .Include(l => l.HocViens)
                .Include(l => l.GiangVien)
                .Include(l => l.KhoaHoc)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(l => l.TenLop.Contains(search) || (l.MoTa != null && l.MoTa.Contains(search)));

            if (trangThai.HasValue)
                query = query.Where(l => l.TrangThai == trangThai.Value);

            if (giangVienId.HasValue)
                query = query.Where(l => l.GiangVienId == giangVienId.Value);

            if (khoaHocId.HasValue)
                query = query.Where(l => l.KhoaHocId == khoaHocId.Value);

            var danhSach = await query.OrderByDescending(l => l.NgayKhaiGiang).ToListAsync();

            // Thống kê cho dashboard header
            var allLops = await _context.LopHoc.Include(l => l.HocViens).ToListAsync();
            ViewBag.TongSoLop = allLops.Count;
            ViewBag.LopDangHoc = allLops.Count(l => l.TrangThai == 1);
            ViewBag.LopSapMo = allLops.Count(l => l.TrangThai == 0);
            ViewBag.TongHocVien = allLops.Sum(l => l.HocViens?.Count ?? 0);

            return View(danhSach);
        }

        // =========================================================
        // GET: LopHocs/Details/5
        // =========================================================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var lopHoc = await _context.LopHoc
                .Include(l => l.HocViens)
                .Include(l => l.GiangVien)
                .Include(l => l.KhoaHoc)
                .Include(l => l.DiemDanhs)
                    .ThenInclude(d => d.HocVien)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (lopHoc == null) return NotFound();

            // Tính thống kê điểm danh
            if (lopHoc.DiemDanhs != null && lopHoc.DiemDanhs.Any())
            {
                ViewBag.TongBuoiDiemDanh = lopHoc.DiemDanhs.Select(d => d.NgayDiemDanh.Date).Distinct().Count();
                ViewBag.TyLeCoMat = lopHoc.DiemDanhs.Any()
                    ? (int)((double)lopHoc.DiemDanhs.Count(d => d.TrangThai == 1) / lopHoc.DiemDanhs.Count() * 100)
                    : 0;
            }
            else
            {
                ViewBag.TongBuoiDiemDanh = 0;
                ViewBag.TyLeCoMat = 0;
            }

            // Tính doanh thu thu được (số HV * học phí) — sơ bộ
            ViewBag.DoanhThuUocTinh = (lopHoc.HocViens?.Count ?? 0) * lopHoc.HocPhi;

            return View(lopHoc);
        }

        // =========================================================
        // GET: LopHocs/Create
        // =========================================================
        public async Task<IActionResult> Create()
        {
            await LoadDropdowns();
            return View();
        }

        // POST: LopHocs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TenLop,KhoaHocId,GiangVienId,NgayKhaiGiang,NgayKetThuc,LichHoc,GioBatDau,GioKetThuc,HocPhi,SiSoToiDa,PhongHoc,MoTa,TrangThai")] LopHoc lopHoc)
        {
            if (ModelState.IsValid)
            {
                lopHoc.NgayTao = DateTime.Now;
                _context.Add(lopHoc);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Đã mở lớp <strong>{lopHoc.TenLop}</strong> thành công!";
                return RedirectToAction(nameof(Index));
            }
            await LoadDropdowns(lopHoc.GiangVienId, lopHoc.KhoaHocId);
            return View(lopHoc);
        }

        // =========================================================
        // GET: LopHocs/Edit/5
        // =========================================================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var lopHoc = await _context.LopHoc.FindAsync(id);
            if (lopHoc == null) return NotFound();
            await LoadDropdowns(lopHoc.GiangVienId, lopHoc.KhoaHocId);
            return View(lopHoc);
        }

        // POST: LopHocs/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TenLop,KhoaHocId,GiangVienId,NgayKhaiGiang,NgayKetThuc,LichHoc,GioBatDau,GioKetThuc,HocPhi,SiSoToiDa,PhongHoc,MoTa,TrangThai,NgayTao")] LopHoc lopHoc)
        {
            if (id != lopHoc.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(lopHoc);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = $"Đã cập nhật lớp <strong>{lopHoc.TenLop}</strong> thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LopHocExists(lopHoc.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            await LoadDropdowns(lopHoc.GiangVienId, lopHoc.KhoaHocId);
            return View(lopHoc);
        }

        // =========================================================
        // GET/POST: LopHocs/Delete/5
        // =========================================================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var lopHoc = await _context.LopHoc
                .Include(l => l.GiangVien)
                .Include(l => l.KhoaHoc)
                .Include(l => l.HocViens)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (lopHoc == null) return NotFound();
            return View(lopHoc);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var lopHoc = await _context.LopHoc.FindAsync(id);
            if (lopHoc != null)
            {
                _context.LopHoc.Remove(lopHoc);
                TempData["Success"] = $"Đã xóa lớp <strong>{lopHoc.TenLop}</strong>.";
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // =========================================================
        // AJAX: Đổi trạng thái nhanh (không cần vào trang Edit)
        // =========================================================
        [HttpPost]
        public async Task<IActionResult> DoiTrangThai(int id, int trangThai)
        {
            var lopHoc = await _context.LopHoc.FindAsync(id);
            if (lopHoc == null) return NotFound();
            lopHoc.TrangThai = trangThai;
            await _context.SaveChangesAsync();
            return Json(new { success = true, trangThai = lopHoc.TrangThai, text = lopHoc.TrangThaiText });
        }

        // =========================================================
        // Helpers
        // =========================================================
        private bool LopHocExists(int id) => _context.LopHoc.Any(e => e.Id == id);

        private async Task LoadDropdowns(int? giangVienId = null, int? khoaHocId = null)
        {
            ViewBag.GiangVienId = new SelectList(
                await _context.GiangVien.Where(g => g.TrangThai == 1).OrderBy(g => g.HoTen).ToListAsync(),
                "Id", "HoTen", giangVienId);

            ViewBag.KhoaHocId = new SelectList(
                await _context.KhoaHoc.Where(k => k.TrangThai).OrderBy(k => k.TenKhoaHoc).ToListAsync(),
                "Id", "TenKhoaHoc", khoaHocId);
        }
    }
}
