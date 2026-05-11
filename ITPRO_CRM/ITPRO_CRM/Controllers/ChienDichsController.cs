using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ITPRO_CRM.Data;
using ITPRO_CRM.Models;
using ITPRO_CRM.Filters; // 👈 Thêm để dùng [PhanQuyen]

namespace ITPRO_CRM.Controllers
{
    // Cho phép Admin, Kế toán và Sale vào xem
    [PhanQuyen(LoaiVaiTro.Admin, LoaiVaiTro.KeToan, LoaiVaiTro.Sale)]
    public class ChienDichsController : Controller
    {
        private readonly ITPRO_CRMContext _context;

        public ChienDichsController(ITPRO_CRMContext context)
        {
            _context = context;
        }

        // GET: ChienDichs - Ai cũng xem được danh sách
        public async Task<IActionResult> Index()
        {
            return View(await _context.ChienDich.ToListAsync());
        }

        // GET: ChienDichs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var chienDich = await _context.ChienDich.FirstOrDefaultAsync(m => m.Id == id);
            if (chienDich == null) return NotFound();

            return View(chienDich);
        }

        // 🔐 CHỈ ADMIN MỚI ĐƯỢC TẠO/SỬA/XÓA CHIẾN DỊCH
        [PhanQuyen(LoaiVaiTro.Admin)]
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PhanQuyen(LoaiVaiTro.Admin)]
        // BỔ SUNG MaTracking và NhanVienSaleId vào Bind
        public async Task<IActionResult> Create([Bind("Id,TenChienDich,LoaiChienDich,NgayBatDau,NgayKetThuc,NganSach,DoanhThuKyVong,DangHoatDong,MoTa,MaTracking,NhanVienSaleId")] ChienDich chienDich)
        {
            if (ModelState.IsValid)
            {
                // Sinh mã Tracking chống trùng lặp (Thêm ss - giây)
                if (string.IsNullOrEmpty(chienDich.MaTracking))
                {
                    chienDich.MaTracking = "camp-" + DateTime.Now.ToString("MMddHHmmss");
                }

                _context.Add(chienDich);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(chienDich);
        }

        [PhanQuyen(LoaiVaiTro.Admin)]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var chienDich = await _context.ChienDich.FindAsync(id);
            if (chienDich == null) return NotFound();
            return View(chienDich);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PhanQuyen(LoaiVaiTro.Admin)]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TenChienDich,LoaiChienDich,NgayBatDau,NgayKetThuc,NganSach,DoanhThuKyVong,DangHoatDong,MoTa,MaTracking,NhanVienSaleId")] ChienDich chienDich)
        {
            if (id != chienDich.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(chienDich);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ChienDichExists(chienDich.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(chienDich);
        }

        [PhanQuyen(LoaiVaiTro.Admin)]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var chienDich = await _context.ChienDich.FirstOrDefaultAsync(m => m.Id == id);
            if (chienDich == null) return NotFound();
            return View(chienDich);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [PhanQuyen(LoaiVaiTro.Admin)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var chienDich = await _context.ChienDich.FindAsync(id);
            if (chienDich != null) _context.ChienDich.Remove(chienDich);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ChienDichExists(int id) => _context.ChienDich.Any(e => e.Id == id);

        // Báo cáo chiến dịch (Sale xem được để biết hiệu quả nguồn khách)
        public async Task<IActionResult> BaoCao()
        {
            var rawData = await _context.ChienDich
                .Include(c => c.HocViens).ThenInclude(h => h.LopHoc)
                .ToListAsync();

            var reportData = rawData.Select(x => new
            {
                Ten = x.TenChienDich,
                ChiPhi = x.NganSach,
                DoanhThu = x.HocViens.Sum(h => h.LopHoc?.HocPhi ?? 0),
                SoKhach = x.HocViens.Count
            }).ToList();

            ViewBag.ChartLabel = Newtonsoft.Json.JsonConvert.SerializeObject(reportData.Select(x => x.Ten));
            ViewBag.ChartChiPhi = Newtonsoft.Json.JsonConvert.SerializeObject(reportData.Select(x => x.ChiPhi));
            ViewBag.ChartDoanhThu = Newtonsoft.Json.JsonConvert.SerializeObject(reportData.Select(x => x.DoanhThu));

            return View();
        }
    }
}