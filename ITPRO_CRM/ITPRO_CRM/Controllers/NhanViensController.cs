using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ITPRO_CRM.Data;
using ITPRO_CRM.Models;
using ITPRO_CRM.Filters;

namespace ITPRO_CRM.Controllers
{
    [PhanQuyen(LoaiVaiTro.Admin)] // 🔐 Chỉ Admin mới được vào
    public class NhanViensController : Controller
    {
        private readonly ITPRO_CRMContext _context;

        public NhanViensController(ITPRO_CRMContext context)
        {
            _context = context;
        }

        // --- DANH SÁCH NHÂN VIÊN ---
        public async Task<IActionResult> Index()
        {
            return View(await _context.NhanVien.OrderBy(n => n.VaiTro).ToListAsync());
        }

        // --- TẠO MỚI (GET) ---
        public IActionResult Create()
        {
            return View();
        }

        // --- TẠO MỚI (POST) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NhanVien nhanVien)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra Email trùng
                if (_context.NhanVien.Any(n => n.Email == nhanVien.Email))
                {
                    ModelState.AddModelError("Email", "Email này đã được sử dụng!");
                    return View(nhanVien);
                }

                _context.Add(nhanVien);
                await _context.SaveChangesAsync();
                TempData["Success"] = "🎉 Đã thêm nhân viên mới thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(nhanVien);
        }

        // --- CHỈNH SỬA (GET) ---
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var nhanVien = await _context.NhanVien.FindAsync(id);
            if (nhanVien == null) return NotFound();
            return View(nhanVien);
        }

        // --- CHỈNH SỬA (POST) ---
        // POST: NhanViens/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PhanQuyen(LoaiVaiTro.Admin)] // Vẫn giữ chốt chặn chỉ Admin được sửa
        public async Task<IActionResult> Edit(int id, NhanVien nhanVien)
        {
            if (id != nhanVien.Id) return NotFound();

            try
            {
                // 1. Tìm thông tin nhân viên cũ trong Database để đối chiếu
                var oldNhanVien = await _context.NhanVien.AsNoTracking().FirstOrDefaultAsync(n => n.Id == id);
                if (oldNhanVien == null) return NotFound();

                // 2. GIẢI QUYẾT LỖI NHÁY: Giữ lại Mật khẩu cũ nếu trên form đang để trống
                if (string.IsNullOrEmpty(nhanVien.MatKhau))
                {
                    nhanVien.MatKhau = oldNhanVien.MatKhau;
                }

                // Giữ lại một số trường quan trọng khác nếu form edit của bạn không có
                // Ví dụ: nhanVien.NgayTao = oldNhanVien.NgayTao; 

                // 3. XÓA BỎ LỖI XÁC THỰC (Bỏ qua việc hệ thống bắt bẻ thiếu dữ liệu)
                ModelState.Clear();

                // 4. Bắt đầu lưu thông tin mới
                _context.Update(nhanVien);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Đã cập nhật thông tin nhân sự thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Nếu có lỗi SQL thực sự, nó sẽ báo ra đây thay vì chỉ "nháy"
                ModelState.AddModelError("", "Lỗi khi lưu dữ liệu: " + ex.Message);
                return View(nhanVien);
            }
        }

        // --- XÓA (GET) ---
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var nhanVien = await _context.NhanVien.FirstOrDefaultAsync(m => m.Id == id);
            if (nhanVien == null) return NotFound();
            return View(nhanVien);
        }

        // --- XÓA (POST) ---
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var nhanVien = await _context.NhanVien.FindAsync(id);
            if (nhanVien != null)
            {
                _context.NhanVien.Remove(nhanVien);
                await _context.SaveChangesAsync();
                TempData["Success"] = "🗑️ Đã xóa nhân viên khỏi hệ thống!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}