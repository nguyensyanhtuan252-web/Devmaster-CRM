using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ITPRO_CRM.Data;
using System.Linq;
using ITPRO_CRM.Models;
using ITPRO_CRM.Filters;
using Microsoft.AspNetCore.Http;

namespace ITPRO_CRM.Controllers
{
    // ─── ViewModels (Giữ nguyên của Tuấn) ─────────────────────────────────────────
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

    [PhanQuyen(LoaiVaiTro.Admin, LoaiVaiTro.KeToan, LoaiVaiTro.Sale)]
    public class BaoCaosController : Controller
    {
        private readonly ITPRO_CRMContext _context;

        public BaoCaosController(ITPRO_CRMContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? month, int? year, int? staffId)
        {
            // 1. Lấy thông tin quyền từ Session
            var roleId = HttpContext.Session.GetInt32("VaiTro");
            var userId = HttpContext.Session.GetInt32("UserId") ?? 0; // Đảm bảo không bị null

            if (userId == 0 && HttpContext.Session.GetString("UserName") == null)
                return RedirectToAction("Login", "Access");

            // 2. Thiết lập tham số lọc
            var today = DateTime.Now;
            int targetMonth = month ?? today.Month;
            int targetYear = year ?? today.Year;

            int? filterStaffId = (staffId == 0 || staffId == null) ? null : staffId;

            ViewBag.SelectedMonth = targetMonth;
            ViewBag.SelectedYear = targetYear;
            ViewBag.SelectedStaff = (roleId == (int)LoaiVaiTro.Sale) ? userId : (staffId ?? 0);

            // 3. Xử lý thời gian lọc 
            DateTime startOfMonth, endOfMonth, startOfLastMonth, endOfLastMonth;
            if (targetMonth == 0)
            {
                startOfMonth = new DateTime(targetYear, 1, 1);
                endOfMonth = new DateTime(targetYear + 1, 1, 1);
                startOfLastMonth = startOfMonth.AddYears(-1);
                endOfLastMonth = endOfMonth.AddYears(-1);
            }
            else
            {
                startOfMonth = new DateTime(targetYear, targetMonth, 1);
                endOfMonth = startOfMonth.AddMonths(1);
                startOfLastMonth = startOfMonth.AddMonths(-1);
                endOfLastMonth = startOfMonth;
            }

            // 4. Khởi tạo Query gốc
            var phieuThuQuery = _context.PhieuThu.AsQueryable();
            var hocVienQuery = _context.HocVien.AsQueryable();

            // 🔥 BẢO MẬT TUYỆT ĐỐI: KHÓA CỨNG QUERY THEO ROLE
            if (roleId == (int)LoaiVaiTro.Sale)
            {
                // Ép buộc query chỉ lấy dữ liệu của chính Sale này
                phieuThuQuery = phieuThuQuery.Where(p => p.HocVien != null && p.HocVien.NhanVienId == userId);
                hocVienQuery = hocVienQuery.Where(h => h.NhanVienId == userId);
            }
            else if (filterStaffId.HasValue)
            {
                // Admin hoặc kế toán lọc theo nhân viên cụ thể
                phieuThuQuery = phieuThuQuery.Where(p => p.HocVien != null && p.HocVien.NhanVienId == filterStaffId);
                hocVienQuery = hocVienQuery.Where(h => h.NhanVienId == filterStaffId);
            }

            // ─── 1. KPI CARDS ─────────
            ViewBag.TongDoanhThu = await phieuThuQuery.SumAsync(p => (decimal?)p.SoTien) ?? 0;

            ViewBag.RevenueThisMonth = await phieuThuQuery
                .Where(p => p.NgayThu >= startOfMonth && p.NgayThu < endOfMonth)
                .SumAsync(p => (decimal?)p.SoTien) ?? 0;

            ViewBag.RevenueLastMonth = await phieuThuQuery
                .Where(p => p.NgayThu >= startOfLastMonth && p.NgayThu < endOfLastMonth)
                .SumAsync(p => (decimal?)p.SoTien) ?? 0;

            ViewBag.HocVienMoi = await hocVienQuery
                .CountAsync(h => h.TrangThai == 2 && h.NgayTao >= startOfMonth && h.NgayTao < endOfMonth);

            double tongHoSo = await hocVienQuery.CountAsync();
            double soHocVien = await hocVienQuery.CountAsync(h => h.TrangThai == 2);
            ViewBag.TiLeChot = tongHoSo > 0 ? Math.Round((soHocVien / tongHoSo) * 100, 1) : 0;

            ViewBag.TotalClasses = await _context.LopHoc.CountAsync();
            ViewBag.ActiveClasses = await _context.LopHoc.CountAsync(l => l.TrangThai == 1);

            var totalDiemDanh = _context.DiemDanh.Include(d => d.HocVien).AsQueryable();
            // Lọc điểm danh theo Sale
            if (roleId == (int)LoaiVaiTro.Sale)
                totalDiemDanh = totalDiemDanh.Where(d => d.HocVien != null && d.HocVien.NhanVienId == userId);
            else if (filterStaffId.HasValue)
                totalDiemDanh = totalDiemDanh.Where(d => d.HocVien != null && d.HocVien.NhanVienId == filterStaffId);

            var totalCount = await totalDiemDanh.CountAsync();
            var coMatCount = await totalDiemDanh.CountAsync(d => d.TrangThai == 1);
            ViewBag.AttendanceRate = totalCount > 0 ? Math.Round(((double)coMatCount / totalCount) * 100, 1) : 0;

            // ─── 2. BIỂU ĐỒ DOANH THU & HỌC VIÊN MỚI ──────────────────
            var revenueData = new List<decimal>();
            var newStudentsData = new List<int>();
            var months = new List<string>();

            for (int i = 11; i >= 0; i--)
            {
                var monthDate = today.AddMonths(-i);
                months.Add(monthDate.ToString("MM/yyyy"));

                var rQuery = _context.PhieuThu.Where(p => p.NgayThu.Month == monthDate.Month && p.NgayThu.Year == monthDate.Year);
                var sQuery = _context.HocVien.Where(h => h.TrangThai == 2 && h.NgayTao != null && h.NgayTao.Value.Month == monthDate.Month && h.NgayTao.Value.Year == monthDate.Year);

                if (roleId == (int)LoaiVaiTro.Sale)
                {
                    rQuery = rQuery.Where(p => p.HocVien != null && p.HocVien.NhanVienId == userId);
                    sQuery = sQuery.Where(h => h.NhanVienId == userId);
                }
                else if (filterStaffId.HasValue)
                {
                    rQuery = rQuery.Where(p => p.HocVien != null && p.HocVien.NhanVienId == filterStaffId);
                    sQuery = sQuery.Where(h => h.NhanVienId == filterStaffId);
                }

                revenueData.Add(await rQuery.SumAsync(p => (decimal?)p.SoTien) ?? 0);
                newStudentsData.Add(await sQuery.CountAsync());
            }

            ViewBag.RevenueData = revenueData;
            ViewBag.NewStudentsData = newStudentsData;
            ViewBag.Months = months;

            // 🌟 Lọc danh sách nhân viên: Sale chỉ thấy chính mình
            var nvQuery = _context.NhanVien.Where(nv => nv.TrangThai == true).AsQueryable();
            if (roleId == (int)LoaiVaiTro.Sale) { nvQuery = nvQuery.Where(nv => nv.Id == userId); }
            ViewBag.DanhSachNhanVien = await nvQuery.ToListAsync();

            // ─── 3. TRẠNG THÁI HỌC VIÊN ───────────────────────────
            ViewBag.StudentStats = new List<int> {
                await hocVienQuery.CountAsync(h => h.TrangThai == 0 && h.NgayTao >= startOfMonth && h.NgayTao < endOfMonth),
                await hocVienQuery.CountAsync(h => h.TrangThai == 1 && h.NgayTao >= startOfMonth && h.NgayTao < endOfMonth),
                await hocVienQuery.CountAsync(h => h.TrangThai == 2 && h.NgayTao >= startOfMonth && h.NgayTao < endOfMonth)
            };

            // ─── 4. DOANH THU THEO LỚP ───────────────────────────
            var revenueByClassRaw = await phieuThuQuery
                .Where(p => p.LopHocId != null && p.NgayThu >= startOfMonth && p.NgayThu < endOfMonth)
                .GroupBy(p => new { p.LopHocId, p.LopHoc.TenLop })
                .Select(g => new {
                    tenLop = g.Key.TenLop ?? "Không xác định",
                    tongThu = g.Sum(p => (decimal?)p.SoTien) ?? 0
                })
                .OrderByDescending(x => x.tongThu).Take(8).ToListAsync();

            ViewBag.RevenueByClass = revenueByClassRaw
                .Select(x => new RevenueByClassViewModel { TenLop = x.tenLop, TongThu = x.tongThu }).ToList();

            // ─── 5. NGUỒN KHÁCH HÀNG ──────────────────────────────
            var sourceDataRaw = await hocVienQuery
                .Where(h => h.NguonGoc != null && h.NgayTao >= startOfMonth && h.NgayTao < endOfMonth)
                .GroupBy(h => h.NguonGoc)
                .Select(g => new {
                    nguon = g.Key ?? "Khác",
                    soLuong = g.Count()
                })
                .OrderByDescending(x => x.soLuong).ToListAsync();

            ViewBag.SourceData = sourceDataRaw
                .Select(x => new SourceDataViewModel { Nguon = x.nguon, SoLuong = x.soLuong }).ToList();

            // ─── 6. CHI TIẾT PHIẾU THU ────────────────────────────
            ViewBag.PhieuThus = await phieuThuQuery
                .Where(p => p.NgayThu >= startOfMonth && p.NgayThu < endOfMonth)
                .Include(p => p.HocVien).Include(p => p.LopHoc)
                .OrderByDescending(p => p.NgayThu).Take(100).ToListAsync();

            // ─── 7. TOP HỌC VIÊN ──────────────────────────────────
            var topStudentsRaw = await hocVienQuery
                .Include(h => h.PhieuThus).Include(h => h.LopHoc)
                .Select(h => new {
                    h.HoTen,
                    h.SoDienThoai,
                    h.Email,
                    h.TrangThai,
                    h.NgayTao,
                    TenLop = h.LopHoc != null ? h.LopHoc.TenLop : null,
                    TongHocPhi = h.PhieuThus.Sum(p => (decimal?)p.SoTien) ?? 0
                })
                .Where(h => h.TongHocPhi > 0).OrderByDescending(h => h.TongHocPhi).Take(20).ToListAsync();

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

            // ─── 8. DANH SÁCH LỚP HỌC ────────────────────────────────
            ViewBag.LopHocs = await _context.LopHoc.Include(l => l.HocViens).OrderBy(l => l.TrangThai).ThenByDescending(l => l.NgayKhaiGiang).ToListAsync();

            // ─── 9. HIỆU SUẤT & 10. KPI ──────────────────────────────────
            var staffPerfList = await nvQuery.ToListAsync();
            ViewBag.StaffPerformance = staffPerfList.Select(nv => new StaffPerformanceViewModel
            {
                HoTen = nv.HoTen,
                SoHocVien = _context.HocVien.Count(h => h.NhanVienId == nv.Id && h.TrangThai == 2 && h.NgayTao >= startOfMonth && h.NgayTao < endOfMonth),
                DoanhThu = _context.PhieuThu.Where(p => p.HocVien != null && p.HocVien.NhanVienId == nv.Id && p.NgayThu >= startOfMonth && p.NgayThu < endOfMonth).Sum(p => (decimal?)p.SoTien) ?? 0
            }).ToList();

            ViewBag.StaffKpi = staffPerfList.Select(nv => new StaffKpiViewModel
            {
                HoTen = nv.HoTen,
                KpiThang = nv.KpiThang,
                SoHocVien = _context.HocVien.Count(h => h.NhanVienId == nv.Id && h.TrangThai == 2 && h.NgayTao >= startOfMonth && h.NgayTao < endOfMonth)
            }).ToList();

            return View();
        }
    }
}