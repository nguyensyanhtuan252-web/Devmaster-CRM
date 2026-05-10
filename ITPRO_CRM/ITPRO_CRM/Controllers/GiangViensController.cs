using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ITPRO_CRM.Data;
using ITPRO_CRM.Models;
using ITPRO_CRM.Filters; // 👈 Thêm để dùng bộ lọc

namespace ITPRO_CRM.Controllers
{
    // Cấp quyền xem: Admin, Kế toán và Sale đều vào được trang chủ và chi tiết
    [PhanQuyen(LoaiVaiTro.Admin, LoaiVaiTro.KeToan, LoaiVaiTro.Sale)]
    public class GiangViensController : Controller
    {
        private readonly ITPRO_CRMContext _context;

        public GiangViensController(ITPRO_CRMContext context)
        {
            _context = context;
        }

        // GET: GiangViens
        public async Task<IActionResult> Index()
        {
            // Kiểm tra đăng nhập (Logic của bạn)
            if (HttpContext.Session.GetString("UserName") == null)
            {
                return RedirectToAction("Login", "Access");
            }

            return View(await _context.GiangVien.ToListAsync());
        }

        // GET: GiangViens/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var giangVien = await _context.GiangVien.FirstOrDefaultAsync(m => m.Id == id);
            if (giangVien == null) return NotFound();

            return View(giangVien);
        }

        // 🔐 CHỐT CHẶN: Chỉ Admin mới có quyền truy cập các hàm thay đổi dữ liệu bên dưới

        // GET: GiangViens/Create
        [PhanQuyen(LoaiVaiTro.Admin)]
        public IActionResult Create()
        {
            return View();
        }

        // POST: GiangViens/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PhanQuyen(LoaiVaiTro.Admin)]
        public async Task<IActionResult> Create([Bind("Id,HoTen,Email,SoDienThoai,AnhDaiDien,BangCap,KinhNghiem,ThanhTich,ChuyenMon,TrangThai")] GiangVien giangVien)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(giangVien.AnhDaiDien))
                {
                    giangVien.AnhDaiDien = "https://ui-avatars.com/api/?name=" + Uri.EscapeDataString(giangVien.HoTen) + "&background=random&size=128";
                }

                _context.Add(giangVien);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(giangVien);
        }

        // GET: GiangViens/Edit/5
        [PhanQuyen(LoaiVaiTro.Admin)]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var giangVien = await _context.GiangVien.FindAsync(id);
            if (giangVien == null) return NotFound();

            return View(giangVien);
        }

        // POST: GiangViens/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PhanQuyen(LoaiVaiTro.Admin)]
        public async Task<IActionResult> Edit(int id, [Bind("Id,HoTen,Email,SoDienThoai,AnhDaiDien,BangCap,KinhNghiem,ThanhTich,ChuyenMon,TrangThai")] GiangVien giangVien)
        {
            if (id != giangVien.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    if (string.IsNullOrEmpty(giangVien.AnhDaiDien))
                    {
                        giangVien.AnhDaiDien = "https://ui-avatars.com/api/?name=" + Uri.EscapeDataString(giangVien.HoTen) + "&background=random";
                    }

                    _context.Update(giangVien);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GiangVienExists(giangVien.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(giangVien);
        }

        // GET: GiangViens/Delete/5
        [PhanQuyen(LoaiVaiTro.Admin)]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var giangVien = await _context.GiangVien.FirstOrDefaultAsync(m => m.Id == id);
            if (giangVien == null) return NotFound();

            return View(giangVien);
        }

        // POST: GiangViens/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [PhanQuyen(LoaiVaiTro.Admin)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var giangVien = await _context.GiangVien.FindAsync(id);
            if (giangVien != null)
            {
                _context.GiangVien.Remove(giangVien);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool GiangVienExists(int id)
        {
            return _context.GiangVien.Any(e => e.Id == id);
        }
    }
}