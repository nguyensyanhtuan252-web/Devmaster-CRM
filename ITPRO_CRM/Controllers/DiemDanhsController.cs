using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ITPRO_CRM.Data;
using ITPRO_CRM.Models;

namespace ITPRO_CRM.Controllers
{
    public class DiemDanhsController : Controller
    {
        private readonly ITPRO_CRMContext _context;

        public DiemDanhsController(ITPRO_CRMContext context)
        {
            _context = context;
        }

        // 1. Hiển thị bảng điểm danh của lớp trong ngày hôm nay
        // GET: DiemDanhs/Index?lopId=5
        public async Task<IActionResult> Index(int lopId)
        {
            var today = DateTime.Now.Date;

            // Lấy thông tin lớp
            var lop = await _context.LopHoc
                .Include(l => l.HocViens)
                .FirstOrDefaultAsync(l => l.Id == lopId);

            if (lop == null) return NotFound();

            ViewBag.LopHoc = lop;
            ViewBag.Ngay = today;

            // Kiểm tra xem hôm nay đã điểm danh chưa?
            var daDiemDanh = await _context.DiemDanh
                .Where(d => d.LopHocId == lopId && d.NgayDiemDanh.Date == today)
                .ToListAsync();

            if (daDiemDanh.Count == 0)
            {
                // Nếu chưa điểm danh -> Tạo danh sách nháp (mặc định Có mặt hết)
                var listNhap = new List<DiemDanh>();
                if (lop.HocViens != null)
                {
                    foreach (var hv in lop.HocViens)
                    {
                        listNhap.Add(new DiemDanh
                        {
                            LopHocId = lopId,
                            HocVienId = hv.Id,
                            HocVien = hv,
                            TrangThai = 1 // Mặc định có mặt
                        });
                    }
                }
                return View(listNhap);
            }

            // Nếu đã điểm danh rồi -> Load lại dữ liệu cũ để xem/sửa
            // Cần map lại object HocVien để hiển thị tên
            foreach (var dd in daDiemDanh)
            {
                dd.HocVien = lop.HocViens?.FirstOrDefault(hv => hv.Id == dd.HocVienId);
            }
            return View(daDiemDanh);
        }

        // 2. Lưu kết quả điểm danh
        [HttpPost]
        public async Task<IActionResult> Save(List<DiemDanh> listDiemDanh, int lopId)
        {
            var today = DateTime.Now.Date;

            // Xóa dữ liệu cũ của ngày hôm nay (nếu có) để lưu lại cái mới
            var oldData = await _context.DiemDanh
                .Where(d => d.LopHocId == lopId && d.NgayDiemDanh.Date == today)
                .ToListAsync();

            _context.DiemDanh.RemoveRange(oldData);

            // Lưu danh sách mới
            foreach (var item in listDiemDanh)
            {
                item.Id = 0; // Reset ID để thêm mới
                item.NgayDiemDanh = today; // Chốt ngày hôm nay
                _context.DiemDanh.Add(item);
            }

            await _context.SaveChangesAsync();

            // Quay về trang chi tiết lớp học
            return RedirectToAction("Details", "LopHocs", new { id = lopId });
        }
    }
}