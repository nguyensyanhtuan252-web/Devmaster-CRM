using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ITPRO_CRM.Data;
using System.Linq;

namespace ITPRO_CRM.Controllers
{
    // ─── ViewModels dùng trong BaoCaos ─────────────────────────────────────────
    public class StaffKpiViewModel
    {
        public string HoTen { get; set; } = "";
        public int? KpiThang { get; set; }
        public int SoHocVien { get; set; }
    }

    public class StaffPerformanceViewModel
    {
        public string HoTen { get; set; } = "";
        public int SoHocVien { get; set; }
        public decimal DoanhThu { get; set; }
    }

    public class RevenueByClassViewModel
    {
        public string TenLop { get; set; } = "";
        public decimal TongThu { get; set; }
    }

    public class SourceDataViewModel
    {
        public string Nguon { get; set; } = "";
        public int SoLuong { get; set; }
    }

    public class TopStudentViewModel
    {
        public string HoTen { get; set; } = "";
        public string? SoDienThoai { get; set; }
        public string? Email { get; set; }
        public int TrangThai { get; set; }
        public DateTime? NgayTao { get; set; }
        public string? TenLop { get; set; }
        public decimal TongHocPhi { get; set; }
    }
    // ───────────────────────────────────────────────────────────────────────────

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
            var startOfMonth = new DateTime(today.Year, today.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1);
            var startOfLastMonth = startOfMonth.AddMonths(-1);

            // ─── 1. KPI CARDS ───────────────────────────────────────────────────────

            // Tổng doanh thu lũy kế
            ViewBag.TongDoanhThu = await _context.PhieuThu.SumAsync(p => p.SoTien);

            // Doanh thu tháng này & tháng trước (để tính tăng trưởng)
            ViewBag.RevenueThisMonth = await _context.PhieuThu
                .Where(p => p.NgayThu >= startOfMonth && p.NgayThu < endOfMonth)
                .SumAsync(p => p.SoTien);

            ViewBag.RevenueLastMonth = await _context.PhieuThu
                .Where(p => p.NgayThu >= startOfLastMonth && p.NgayThu < startOfMonth)
                .SumAsync(p => p.SoTien);

            // Số học viên mới nhập học trong tháng
            ViewBag.HocVienMoi = await _context.HocVien
                .CountAsync(h => h.TrangThai == 2 && h.NgayTao >= startOfMonth && h.NgayTao < endOfMonth);

            // Tỷ lệ chốt (%)
            double tongHoSo = await _context.HocVien.CountAsync();
            double soHocVien = await _context.HocVien.CountAsync(h => h.TrangThai == 2);
            ViewBag.TiLeChot = tongHoSo > 0 ? Math.Round((soHocVien / tongHoSo) * 100, 1) : 0;

            // Lớp học
            ViewBag.TotalClasses = await _context.LopHoc.CountAsync();
            ViewBag.ActiveClasses = await _context.LopHoc.CountAsync(l => l.TrangThai == 1);

            // Tỷ lệ điểm danh trung bình
            var totalDiemDanh = await _context.DiemDanh.CountAsync();
            var coMatDiemDanh = await _context.DiemDanh.CountAsync(d => d.TrangThai == 1);
            ViewBag.AttendanceRate = totalDiemDanh > 0
                ? Math.Round(((double)coMatDiemDanh / totalDiemDanh) * 100, 1)
                : 0;

            // ─── 2. BIỂU ĐỒ DOANH THU 12 THÁNG ────────────────────────────────────
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

            ViewBag.RevenueData = revenueData;
            ViewBag.Months = months;

            // ─── 3. TRẠNG THÁI HỌC VIÊN (PIPELINE) ────────────────────────────────
            var leadCount = await _context.HocVien.CountAsync(h => h.TrangThai == 0);
            var pipeCount = await _context.HocVien.CountAsync(h => h.TrangThai == 1);
            var custCount = await _context.HocVien.CountAsync(h => h.TrangThai == 2);
            ViewBag.StudentStats = new List<int> { leadCount, pipeCount, custCount };

            // ─── 4. DOANH THU THEO LỚP HỌC (Top 8) ────────────────────────────────
            var revenueByClassRaw = await _context.PhieuThu
                .Where(p => p.LopHocId != null)
                .GroupBy(p => new { p.LopHocId, p.LopHoc.TenLop })
                .Select(g => new {
                    tenLop = g.Key.TenLop ?? "Không xác định",
                    tongThu = g.Sum(p => p.SoTien)
                })
                .OrderByDescending(x => x.tongThu)
                .Take(8)
                .ToListAsync();

            ViewBag.RevenueByClass = revenueByClassRaw
                .Select(x => new RevenueByClassViewModel { TenLop = x.tenLop, TongThu = x.tongThu })
                .ToList();

            // ─── 5. NGUỒN KHÁCH HÀNG ───────────────────────────────────────────────
            var sourceDataRaw = await _context.HocVien
                .Where(h => h.NguonGoc != null)
                .GroupBy(h => h.NguonGoc)
                .Select(g => new {
                    nguon = g.Key ?? "Khác",
                    soLuong = g.Count()
                })
                .OrderByDescending(x => x.soLuong)
                .ToListAsync();

            ViewBag.SourceData = sourceDataRaw
                .Select(x => new SourceDataViewModel { Nguon = x.nguon, SoLuong = x.soLuong })
                .ToList();

            // ─── 6. CHI TIẾT PHIẾU THU (Tab doanh thu) ─────────────────────────────
            ViewBag.PhieuThus = await _context.PhieuThu
                .Include(p => p.HocVien)
                .Include(p => p.LopHoc)
                .OrderByDescending(p => p.NgayThu)
                .Take(100)
                .ToListAsync();

            // ─── 7. TOP HỌC VIÊN THEO TỔNG HỌC PHÍ ────────────────────────────────
            var topStudentsRaw = await _context.HocVien
                .Include(h => h.PhieuThus)
                .Include(h => h.LopHoc)
                .Select(h => new {
                    h.HoTen,
                    h.SoDienThoai,
                    h.Email,
                    h.TrangThai,
                    h.NgayTao,
                    TenLop = h.LopHoc != null ? h.LopHoc.TenLop : null,
                    TongHocPhi = h.PhieuThus.Sum(p => p.SoTien)
                })
                .Where(h => h.TongHocPhi > 0)
                .OrderByDescending(h => h.TongHocPhi)
                .Take(20)
                .ToListAsync();

            ViewBag.TopStudents = topStudentsRaw.Select(h => new TopStudentViewModel
            {
                HoTen = h.HoTen,
                SoDienThoai = h.SoDienThoai,
                Email = h.Email,
                TrangThai = h.TrangThai,
                NgayTao = h.NgayTao,
                TenLop = h.TenLop,
                TongHocPhi = h.TongHocPhi
            }).ToList();

            // ─── 8. DANH SÁCH LỚP HỌC CHI TIẾT ────────────────────────────────────
            ViewBag.LopHocs = await _context.LopHoc
                .Include(l => l.HocViens)
                .OrderBy(l => l.TrangThai)
                .ThenByDescending(l => l.NgayKhaiGiang)
                .ToListAsync();

            // ─── 9. HIỆU SUẤT NHÂN VIÊN ────────────────────────────────────────────
            var staffPerfList = await _context.NhanVien
                .Where(nv => nv.TrangThai == true)
                .ToListAsync();

            ViewBag.StaffPerformance = staffPerfList.Select(nv => new StaffPerformanceViewModel
            {
                HoTen = nv.HoTen,
                SoHocVien = _context.HocVien.Count(h => h.NhanVienId == nv.Id && h.TrangThai == 2),
                DoanhThu = _context.PhieuThu
                    .Where(p => p.HocVien.NhanVienId == nv.Id
                             && p.NgayThu >= startOfMonth && p.NgayThu < endOfMonth)
                    .Sum(p => (decimal?)p.SoTien) ?? 0
            }).ToList();

            // ─── 10. KPI NHÂN VIÊN ─────────────────────────────────────────────────
            ViewBag.StaffKpi = staffPerfList.Select(nv => new StaffKpiViewModel
            {
                HoTen = nv.HoTen,
                KpiThang = nv.KpiThang,
                SoHocVien = _context.HocVien.Count(h =>
                    h.NhanVienId == nv.Id
                    && h.TrangThai == 2
                    && h.NgayTao >= startOfMonth && h.NgayTao < endOfMonth)
            }).ToList();

            return View();
        }
    }
}
