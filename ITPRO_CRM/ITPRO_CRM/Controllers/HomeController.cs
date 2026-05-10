using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ITPRO_CRM.Data;
using ITPRO_CRM.Models;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITPRO_CRM.Controllers
{
    public class HomeController : Controller
    {
        private readonly ITPRO_CRMContext _context;

        public HomeController(ITPRO_CRMContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // 1. KIỂM TRA ĐĂNG NHẬP
            var userName = HttpContext.Session.GetString("UserName");
            if (userName == null) return RedirectToAction("Login", "Access");
            ViewBag.UserName = HttpContext.Session.GetString("HoTen") ?? userName;
            ViewBag.UserRole = HttpContext.Session.GetString("UserRole") ?? "Nhân viên";
            ViewBag.UserAvatar = (HttpContext.Session.GetString("HoTen") ?? "A")[0].ToString().ToUpper();

            // 2. LẤY THÔNG TIN NHÂN VIÊN VÀ XÁC ĐỊNH QUYỀN
            var currentEmployee = await _context.NhanVien.FirstOrDefaultAsync(n => n.HoTen == userName || n.Email == userName);
            int currentUserId = currentEmployee?.Id ?? 0;
            bool isAdmin = (currentEmployee?.VaiTro == 0 || userName == "admin@devmaster.edu.vn");

            ViewBag.IsAdmin = isAdmin;

            // 3. THIẾT LẬP THỜI GIAN
            var today = DateTime.Today;
            var startOfMonth = new DateTime(today.Year, today.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1);

            // 4. TÍNH TOÁN KPI (SỐ LIỆU THỰC TẾ)
            var kpiQuery = _context.HocVien
                .Where(h => h.TrangThai == 2) // Chỉ đếm học viên đã chốt (Học viên chính thức)
                .Where(h => h.NgayTao >= startOfMonth && h.NgayTao < endOfMonth);

            int myKpiTarget = 0;
            int myNewStudents = 0;

            if (isAdmin)
            {
                // 1. Lấy danh sách nhân viên Sale
                var allSales = await _context.NhanVien.Where(n => n.VaiTro != 0).ToListAsync();
                ViewBag.AllStaff = allSales;

                // 2. TÍNH TOÁN TIẾN ĐỘ TỪNG NGƯỜI
                var staffProgress = await _context.HocVien
                    .Where(h => h.TrangThai == 2 && h.NgayTao >= startOfMonth && h.NgayTao < endOfMonth)
                    .GroupBy(h => h.NhanVienId)
                    .Select(g => new { StaffId = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.StaffId ?? 0, x => x.Count);

                ViewBag.StaffProgress = staffProgress;

                // 3. TÍNH TỔNG KPI TRUNG TÂM
                myKpiTarget = allSales.Sum(n => n.KpiThang ?? 0);

                // 4. TỔNG THỰC TẾ TRUNG TÂM
                myNewStudents = await kpiQuery.CountAsync();
            }
            else
            {
                myKpiTarget = currentEmployee?.KpiThang ?? 0;
                myNewStudents = await kpiQuery.Where(h => h.NhanVienId == currentUserId).CountAsync();
            }

            ViewBag.MyNewStudents = myNewStudents;
            ViewBag.MyKpiTarget = myKpiTarget;
            ViewBag.KpiPercentage = myKpiTarget > 0 ? (myNewStudents * 100) / myKpiTarget : 0;

            // 5. TÍNH TOÁN LEAD TIỀM NĂNG 
            var leadQuery = _context.HocVien.Where(h => h.TrangThai == 0);
            if (!isAdmin) leadQuery = leadQuery.Where(h => h.NhanVienId == currentUserId);
            ViewBag.TotalLeads = await leadQuery.CountAsync();

            // 6. GỬI DỮ LIỆU KHÁC SANG VIEW
            ViewBag.ActiveClasses = await _context.LopHoc.CountAsync(l => l.TrangThai == 1);

            // 7. DOANH THU (🔥 BẢN SỬA: Lọc phiếu thu theo NhanVienId nếu là Sale)
            var revQuery = _context.PhieuThu.Include(p => p.HocVien).AsQueryable();
            if (!isAdmin)
            {
                revQuery = revQuery.Where(p => p.HocVien != null && p.HocVien.NhanVienId == currentUserId);
            }

            ViewBag.RevenueToday = await revQuery.Where(p => p.NgayThu >= today && p.NgayThu < today.AddDays(1)).SumAsync(p => (decimal?)p.SoTien) ?? 0;
            ViewBag.RevenueMonth = await revQuery.Where(p => p.NgayThu >= startOfMonth && p.NgayThu < endOfMonth).SumAsync(p => (decimal?)p.SoTien) ?? 0;

            // 8. DỮ LIỆU BIỂU ĐỒ (7 ngày gần nhất)
            var labels7 = new List<string>();
            var data7 = new List<decimal>();
            for (int i = 6; i >= 0; i--)
            {
                var date = today.AddDays(-i);
                labels7.Add(date.ToString("dd/MM"));
                data7.Add(await revQuery.Where(p => p.NgayThu.Date == date).SumAsync(p => (decimal?)p.SoTien) ?? 0);
            }
            ViewBag.ChartLabels7 = labels7;
            ViewBag.ChartData7 = data7;

            // 9. LỊCH HẸN VÀ DANH SÁCH MỚI
            var dsLichHen = await _context.HocVien
                .Where(h => h.NgayHen != null && h.NgayHen.Value.Date <= today && h.TrangThai == 1)
                .Where(h => isAdmin || h.NhanVienId == currentUserId)
                .OrderBy(h => h.NgayHen).ToListAsync();
            ViewBag.LichHenHomNay = dsLichHen;

            var newStudentsList = await _context.HocVien
                .Include(h => h.NhanVien) // Lấy thêm tên nhân viên để hiển thị ra bảng
                .Where(h => isAdmin || h.NhanVienId == currentUserId)
                .OrderByDescending(h => h.NgayTao).ToListAsync();

            var next7Days = DateTime.Today.AddDays(7);

            // 10. DANH SÁCH CẦN THANH TOÁN (🔥 BẢN SỬA: Lọc nợ theo nhân viên nếu là Sale)
            var dsCanThanhToanQuery = _context.DotThanhToan
                .Include(d => d.HocVien)
                .ThenInclude(h => h.LopHoc)
                .Where(d => d.TrangThai != 2 && d.HanThanhToan <= next7Days);

            if (!isAdmin)
            {
                dsCanThanhToanQuery = dsCanThanhToanQuery.Where(d => d.HocVien != null && d.HocVien.NhanVienId == currentUserId);
            }

            ViewBag.DsCanThanhToan = await dsCanThanhToanQuery.OrderBy(d => d.HanThanhToan).Take(10).ToListAsync();

            return View(newStudentsList);
        }

        [HttpGet]
        public async Task<JsonResult> GetRevenueData(string period)
        {
            // 🔥 BẢN SỬA: Phải kẹp bộ lọc nhân viên vào cả hàm gọi AJAX vẽ biểu đồ
            var userName = HttpContext.Session.GetString("UserName");
            var currentEmployee = await _context.NhanVien.FirstOrDefaultAsync(n => n.HoTen == userName || n.Email == userName);
            int currentUserId = currentEmployee?.Id ?? 0;
            bool isAdmin = (currentEmployee?.VaiTro == 0 || userName == "admin@devmaster.edu.vn");

            var revQ = _context.PhieuThu.Include(p => p.HocVien).AsQueryable();
            if (!isAdmin)
            {
                revQ = revQ.Where(p => p.HocVien != null && p.HocVien.NhanVienId == currentUserId);
            }

            var labels = new List<string>();
            var data = new List<decimal>();
            var now = DateTime.Now;

            if (period == "month")
            {
                for (int i = 29; i >= 0; i--)
                {
                    var date = now.Date.AddDays(-i);
                    labels.Add(date.ToString("dd/MM"));
                    data.Add(await revQ.Where(p => p.NgayThu.Date == date).SumAsync(p => (decimal?)p.SoTien) ?? 0);
                }
            }
            else if (period == "year")
            {
                for (int i = 11; i >= 0; i--)
                {
                    var date = now.AddMonths(-i);
                    labels.Add("T" + date.Month);
                    data.Add(await revQ.Where(p => p.NgayThu.Month == date.Month && p.NgayThu.Year == date.Year).SumAsync(p => (decimal?)p.SoTien) ?? 0);
                }
            }

            return Json(new { labels, data });
        }

        [HttpPost]
        public async Task<IActionResult> AssignKPI(int staffId, int newKpi)
        {
            var userName = HttpContext.Session.GetString("UserName");
            var currentAdmin = await _context.NhanVien.FirstOrDefaultAsync(n => n.HoTen == userName || n.Email == userName);

            if (currentAdmin == null || currentAdmin.VaiTro != 0)
            {
                TempData["Error"] = "❌ Bạn không có quyền thực hiện thao tác này!";
                return RedirectToAction("Index");
            }

            var staff = await _context.NhanVien.FindAsync(staffId);
            if (staff != null && newKpi >= 0)
            {
                staff.KpiThang = newKpi;
                _context.Update(staff);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"🎯 Đã cập nhật chỉ tiêu KPI thành công cho nhân viên {staff.HoTen}!";
            }
            else
            {
                TempData["Error"] = "❌ Không tìm thấy thông tin nhân viên hoặc số liệu không hợp lệ!";
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<JsonResult> GetKpiHistory(int month, int year)
        {
            var userName = HttpContext.Session.GetString("UserName");
            var nv = await _context.NhanVien.FirstOrDefaultAsync(n => n.HoTen == userName || n.Email == userName);

            var start = new DateTime(year, month, 1);
            var end = start.AddMonths(1);

            var achieved = await _context.HocVien
                .Where(h => h.TrangThai == 2 && h.NgayTao >= start && h.NgayTao < end)
                .Where(h => nv.VaiTro == 0 || h.NhanVienId == nv.Id)
                .CountAsync();

            return Json(new { achieved = achieved, target = (nv.VaiTro == 0 ? 0 : nv.KpiThang) });
        }
    }
}