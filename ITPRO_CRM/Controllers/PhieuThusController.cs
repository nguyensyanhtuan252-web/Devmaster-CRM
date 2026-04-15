using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ITPRO_CRM.Data;
using ITPRO_CRM.Models;

namespace ITPRO_CRM.Controllers
{
    public class PhieuThusController : Controller
    {
        private readonly ITPRO_CRMContext _context;

        public PhieuThusController(ITPRO_CRMContext context)
        {
            _context = context;
        }

        // GET: PhieuThus
        public async Task<IActionResult> Index()
        {
            var iTPRO_CRMContext = _context.PhieuThu.Include(p => p.HocVien).Include(p => p.LopHoc);
            return View(await iTPRO_CRMContext.ToListAsync());
        }

        // GET: PhieuThus/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var phieuThu = await _context.PhieuThu
                .Include(p => p.HocVien)
                .Include(p => p.LopHoc)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (phieuThu == null)
            {
                return NotFound();
            }

            return View(phieuThu);
        }

        // GET: PhieuThus/Create
        // GET: PhieuThus/Create
        // GET: PhieuThus/Create
        public IActionResult Create()
        {
            // Lấy dữ liệu học viên
            var danhSachHocVien = _context.HocVien
                .Select(h => new
                {
                    Id = h.Id,
                    MaHocVien = string.IsNullOrEmpty(h.MaHocVien) ? "" : h.MaHocVien,
                    HoTen = h.HoTen + (string.IsNullOrEmpty(h.SoDienThoai) ? "" : $" ({h.SoDienThoai})")
                }).ToList();

            // 👉 GỬI 2 DANH SÁCH SANG VIEW
            ViewBag.ListMaHV = new SelectList(danhSachHocVien, "Id", "MaHocVien");
            ViewBag.HocVienId = new SelectList(danhSachHocVien, "Id", "HoTen");
            ViewBag.LopHocId = new SelectList(_context.LopHoc, "Id", "TenLop");

            return View();
        }

        // POST: PhieuThus/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,HocVienId,LopHocId,SoTien,NgayThu,NoiDung,NguoiThu,HinhThuc")] PhieuThu phieuThu)
        {
            // Tự sinh mã phiếu: PT + yyyyMMddHHmm (Ví dụ: PT202601280930)
            phieuThu.MaPhieu = "PT" + DateTime.Now.ToString("yyyyMMddHHmm");

            if (ModelState.IsValid)
            {
                _context.Add(phieuThu);
                await _context.SaveChangesAsync();
                // Sau khi thu tiền xong, quay về trang danh sách phiếu thu
                return RedirectToAction(nameof(Index));
            }
            ViewData["HocVienId"] = new SelectList(_context.HocVien, "Id", "HoTen", phieuThu.HocVienId);
            ViewData["LopHocId"] = new SelectList(_context.LopHoc, "Id", "TenLop", phieuThu.LopHocId);
            return View(phieuThu);
        }

        // GET: PhieuThus/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var phieuThu = await _context.PhieuThu.FindAsync(id);
            if (phieuThu == null)
            {
                return NotFound();
            }
            ViewData["HocVienId"] = new SelectList(_context.HocVien, "Id", "HoTen", phieuThu.HocVienId);
            ViewData["LopHocId"] = new SelectList(_context.LopHoc, "Id", "TenLop", phieuThu.LopHocId);
            return View(phieuThu);
        }

        // POST: PhieuThus/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,MaPhieu,HocVienId,LopHocId,SoTien,NgayThu,NoiDung,NguoiThu,HinhThuc")] PhieuThu phieuThu)
        {
            if (id != phieuThu.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(phieuThu);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PhieuThuExists(phieuThu.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["HocVienId"] = new SelectList(_context.HocVien, "Id", "HoTen", phieuThu.HocVienId);
            ViewData["LopHocId"] = new SelectList(_context.LopHoc, "Id", "TenLop", phieuThu.LopHocId);
            return View(phieuThu);
        }

        // GET: PhieuThus/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var phieuThu = await _context.PhieuThu
                .Include(p => p.HocVien)
                .Include(p => p.LopHoc)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (phieuThu == null)
            {
                return NotFound();
            }

            return View(phieuThu);
        }

        // POST: PhieuThus/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var phieuThu = await _context.PhieuThu.FindAsync(id);
            if (phieuThu != null)
            {
                _context.PhieuThu.Remove(phieuThu);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PhieuThuExists(int id)
        {
            return _context.PhieuThu.Any(e => e.Id == id);
        }
    }
}
