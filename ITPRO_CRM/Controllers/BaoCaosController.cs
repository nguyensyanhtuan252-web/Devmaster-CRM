using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ITPRO_CRM.Data;
using System.Linq; // Để dùng hàm Sum, Count

namespace ITPRO_CRM.Controllers
{
    public class BaoCaosController : Controller
    {
        private readonly ITPRO_CRMContext _context;

        public BaoCaosController(ITPRO_CRMContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var today = DateTime.Now;

            // --- 1. TÍNH TOÁN CÁC THẺ KPI TRÊN CÙNG ---

            // Tổng doanh thu lũy kế từ trước đến nay
            ViewBag.TongDoanhThu = await _context.PhieuThu.SumAsync(p => p.SoTien);

            // Số học viên mới nhập học (TrangThai == 2) trong tháng này
            ViewBag.HocVienMoi = await _context.HocVien
                .CountAsync(h => h.TrangThai == 2 && h.NgayTao.Month == today.Month && h.NgayTao.Year == today.Year);

            // Tính tỷ lệ chốt (%) = (Số học viên / Tổng số hồ sơ) * 100
            double tongHoSo = await _context.HocVien.CountAsync();
            double soHocVien = await _context.HocVien.CountAsync(h => h.TrangThai == 2);
            ViewBag.TiLeChot = tongHoSo > 0 ? Math.Round((soHocVien / tongHoSo) * 100, 1) : 0;


            // --- 2. THỐNG KÊ DOANH THU 12 THÁNG (BIỂU ĐỒ ĐƯỜNG) ---
            var revenueData = new List<decimal>();
            var months = new List<string>();

            for (int i = 11; i >= 0; i--)
            {
                var monthDate = today.AddMonths(-i);
                months.Add(monthDate.ToString("MM/yyyy"));

                var total = await _context.PhieuThu
                    .Where(p => p.NgayThu.Month == monthDate.Month && p.NgayThu.Year == monthDate.Year)
                    .SumAsync(p => p.SoTien);

                revenueData.Add(total);
            }


            // --- 3. THỐNG KÊ TRẠNG THÁI (BIỂU ĐỒ TRÒN) ---
            var leadCount = await _context.HocVien.CountAsync(h => h.TrangThai == 0); // Tiềm năng
            var pipeCount = await _context.HocVien.CountAsync(h => h.TrangThai == 1); // Cơ hội
            var custCount = await _context.HocVien.CountAsync(h => h.TrangThai == 2); // Học viên


            // --- 4. GỬI DỮ LIỆU SANG VIEW ---
            ViewBag.RevenueData = revenueData;
            ViewBag.Months = months;
            ViewBag.StudentStats = new List<int> { leadCount, pipeCount, custCount };

            return View();
        }
    }
}