using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ITPRO_CRM.Models;
using ITPRO_CRM.Data;
using Microsoft.AspNetCore.Http; // Bắt buộc cho Session
using ITPRO_CRM.Filters; // Cho [PhanQuyen]

namespace ITPRO_CRM.Controllers
{
    [PhanQuyen(LoaiVaiTro.Admin, LoaiVaiTro.KeToan, LoaiVaiTro.Sale, LoaiVaiTro.GiangVien)]
    public class LopHocsController : Controller
    {
        private readonly ITPRO_CRMContext _context;

        public LopHocsController(ITPRO_CRMContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? search, int? trangThai, int? giangVienId, int? khoaHocId)
        {
            var roleId = HttpContext.Session.GetInt32("VaiTro");

            ViewBag.Search = search;
            ViewBag.TrangThai = trangThai;
            ViewBag.GiangVienId = giangVienId;
            ViewBag.KhoaHocId = khoaHocId;

            // ĐỌC CHUẨN TỪ BẢNG GiangVien ĐỂ KHỚP VỚI DATABASE
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

            var allLops = await _context.LopHoc.Include(l => l.HocViens).ToListAsync();
            ViewBag.TongSoLop = allLops.Count;
            ViewBag.LopDangHoc = allLops.Count(l => l.TrangThai == 1);
            ViewBag.LopSapMo = allLops.Count(l => l.TrangThai == 0);
            ViewBag.TongHocVien = allLops.Sum(l => l.HocViens?.Count ?? 0);

            return View(danhSach);
        }

        public async Task<IActionResult> Details(int? id)
        {
            var roleId = HttpContext.Session.GetInt32("VaiTro");
            if (id == null) return NotFound();

            var lopHoc = await _context.LopHoc
                .Include(l => l.HocViens)
                .Include(l => l.GiangVien)
                .Include(l => l.KhoaHoc)
                .Include(l => l.DiemDanhs).ThenInclude(d => d.HocVien)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (lopHoc == null) return NotFound();

            if (lopHoc.DiemDanhs != null && lopHoc.DiemDanhs.Any())
            {
                ViewBag.TongBuoiDiemDanh = lopHoc.DiemDanhs.Select(d => d.NgayDiemDanh.Date).Distinct().Count();
                ViewBag.TyLeCoMat = lopHoc.DiemDanhs.Any() ? (int)((double)lopHoc.DiemDanhs.Count(d => d.TrangThai == 1) / lopHoc.DiemDanhs.Count() * 100) : 0;
            }
            else { ViewBag.TongBuoiDiemDanh = 0; ViewBag.TyLeCoMat = 0; }

            // ẨN DOANH THU LỚP NẾU KHÔNG PHẢI ADMIN HOẶC KẾ TOÁN
            ViewBag.DoanhThuUocTinh = (roleId == 0 || roleId == 2) ? (lopHoc.HocViens?.Count ?? 0) * lopHoc.HocPhi : 0;

            return View(lopHoc);
        }

        // =========================================================
        // 🔐 KHÓA CỨNG: CÁC HÀM QUẢN TRỊ (CHỈ ADMIN MỚI VƯỢT QUA ĐƯỢC)
        // =========================================================
        public async Task<IActionResult> Create()
        {
            if (HttpContext.Session.GetInt32("VaiTro") != 0) return RedirectToAction("Index", "Home");
            await LoadDropdowns();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TenLop,KhoaHocId,GiangVienId,NgayKhaiGiang,NgayKetThuc,LichHoc,GioBatDau,GioKetThuc,HocPhi,SiSoToiDa,PhongHoc,MoTa,TrangThai")] LopHoc lopHoc)
        {
            if (HttpContext.Session.GetInt32("VaiTro") != 0) return RedirectToAction("Index", "Home");
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

        public async Task<IActionResult> Edit(int? id)
        {
            if (HttpContext.Session.GetInt32("VaiTro") != 0) return RedirectToAction("Index", "Home");
            if (id == null) return NotFound();
            var lopHoc = await _context.LopHoc.FindAsync(id);
            if (lopHoc == null) return NotFound();
            await LoadDropdowns(lopHoc.GiangVienId, lopHoc.KhoaHocId);
            return View(lopHoc);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TenLop,KhoaHocId,GiangVienId,NgayKhaiGiang,NgayKetThuc,LichHoc,GioBatDau,GioKetThuc,HocPhi,SiSoToiDa,PhongHoc,MoTa,TrangThai,NgayTao")] LopHoc lopHoc)
        {
            if (HttpContext.Session.GetInt32("VaiTro") != 0) return RedirectToAction("Index", "Home");
            if (id != lopHoc.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try { _context.Update(lopHoc); await _context.SaveChangesAsync(); TempData["Success"] = $"Đã cập nhật thành công!"; }
                catch (DbUpdateConcurrencyException) { if (!LopHocExists(lopHoc.Id)) return NotFound(); else throw; }
                return RedirectToAction(nameof(Index));
            }
            await LoadDropdowns(lopHoc.GiangVienId, lopHoc.KhoaHocId);
            return View(lopHoc);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (HttpContext.Session.GetInt32("VaiTro") != 0) return RedirectToAction("Index", "Home");
            if (id == null) return NotFound();
            var lopHoc = await _context.LopHoc.Include(l => l.GiangVien).Include(l => l.KhoaHoc).Include(l => l.HocViens).FirstOrDefaultAsync(m => m.Id == id);
            if (lopHoc == null) return NotFound();
            return View(lopHoc);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (HttpContext.Session.GetInt32("VaiTro") != 0) return RedirectToAction("Index", "Home");
            var lopHoc = await _context.LopHoc.FindAsync(id);
            if (lopHoc != null) { _context.LopHoc.Remove(lopHoc); TempData["Success"] = "Đã xóa thành công."; }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> DoiTrangThai(int id, int trangThai)
        {
            if (HttpContext.Session.GetInt32("VaiTro") != 0) return Json(new { success = false });
            var lopHoc = await _context.LopHoc.FindAsync(id);
            if (lopHoc == null) return NotFound();
            lopHoc.TrangThai = trangThai;
            await _context.SaveChangesAsync();
            return Json(new { success = true, trangThai = lopHoc.TrangThai, text = lopHoc.TrangThaiText });
        }

        private bool LopHocExists(int id) => _context.LopHoc.Any(e => e.Id == id);

        private async Task LoadDropdowns(int? giangVienId = null, int? khoaHocId = null)
        {
            // BẢO ĐẢM LẤY TỪ BẢNG GIẢNG VIÊN ĐỂ TRÁNH LỖI FOREIGN KEY
            ViewBag.GiangVienId = new SelectList(
                await _context.GiangVien.Where(g => g.TrangThai == 1).OrderBy(g => g.HoTen).ToListAsync(),
                "Id", "HoTen", giangVienId);

            ViewBag.KhoaHocId = new SelectList(
                await _context.KhoaHoc.Where(k => k.TrangThai).OrderBy(k => k.TenKhoaHoc).ToListAsync(),
                "Id", "TenKhoaHoc", khoaHocId);
        }
    }
}