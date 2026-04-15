using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ITPRO_CRM.Data;
using ITPRO_CRM.Models;
using System.Diagnostics;

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

            // 2. LẤY THÔNG TIN NHÂN VIÊN VÀ XÁC ĐỊNH QUYỀN
            var currentEmployee = await _context.NhanVien.FirstOrDefaultAsync(n => n.HoTen == userName || n.Email == userName);
            int currentUserId = currentEmployee?.Id ?? 0;

            // Xác định xem có phải Admin không (Dựa vào VaiTro trong DB hoặc Email đặc biệt)
            bool isAdmin = (currentEmployee?.VaiTro == 0 || userName == "admin@devmaster.edu.vn");

            // 3. THIẾT LẬP THỜI GIAN
            var today = DateTime.Today;
            var startOfMonth = new DateTime(today.Year, today.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1);

            // 4. TÍNH TOÁN KPI (SỐ NHẢY CHUẨN Ở ĐÂY)
            var kpiQuery = _context.HocVien
                .Where(h => h.TrangThai == 2) // Chỉ đếm "Đã học"
                .Where(h => h.NgayTao >= startOfMonth && h.NgayTao < endOfMonth);

            if (!isAdmin)
            {
                // Nếu là Sale: Chỉ đếm khách của mình
                kpiQuery = kpiQuery.Where(h => h.NhanVienId == currentUserId);
            }
            var myNewStudents = await kpiQuery.CountAsync();

            // 5. TÍNH TOÁN LEAD TIỀM NĂNG (ĐỒNG BỘ VỚI TRANG DANH SÁCH)
            var leadQuery = _context.HocVien.Where(h => h.TrangThai == 0 || h.TrangThai == 1);
            if (!isAdmin)
            {
                leadQuery = leadQuery.Where(h => h.NhanVienId == currentUserId);
            }
            var totalLeads = await leadQuery.CountAsync();

            // 6. GỬI DỮ LIỆU SANG VIEW
            int kpiAdminFallback = HttpContext.Session.GetInt32("AdminKPI") ?? 40;
            int myKpiTarget = currentEmployee?.KpiThang ?? kpiAdminFallback;

            ViewBag.MyNewStudents = myNewStudents;
            ViewBag.MyKpiTarget = myKpiTarget;
            ViewBag.KpiPercentage = myKpiTarget > 0 ? (myNewStudents * 100) / myKpiTarget : 0;
            ViewBag.TotalLeads = totalLeads;
            ViewBag.ActiveClasses = await _context.LopHoc.CountAsync(l => l.TrangThai == 1);

            // --- CÁC CHỈ SỐ DOANH THU (Admin thấy hết, Sale thấy cá nhân nếu cần) ---
            var revQuery = _context.PhieuThu.AsQueryable();
            // Nếu muốn Sale chỉ thấy doanh thu mình chốt, hãy bỏ comment dòng dưới:
            // if (!isAdmin) revQuery = revQuery.Where(p => p.HocVien.NhanVienId == currentUserId);

            ViewBag.RevenueToday = await revQuery.Where(p => p.NgayThu >= today && p.NgayThu < today.AddDays(1)).SumAsync(p => (decimal?)p.SoTien) ?? 0;
            ViewBag.RevenueMonth = await revQuery.Where(p => p.NgayThu >= startOfMonth && p.NgayThu < endOfMonth).SumAsync(p => (decimal?)p.SoTien) ?? 0;

            // --- DỮ LIỆU BIỂU ĐỒ & LỊCH HẸN ---
            // (Giữ nguyên logic cũ của bạn vì nó đã chạy ổn)
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

            var dsLichHen = await _context.HocVien
                .Where(h => h.NgayHen != null && h.NgayHen.Value.Date <= today && h.TrangThai == 1)
                .Where(h => isAdmin || h.NhanVienId == currentUserId)
                .OrderBy(h => h.NgayHen).Take(5).ToListAsync();
            ViewBag.LichHenHomNay = dsLichHen;

            var newStudentsList = await _context.HocVien
                .Where(h => isAdmin || h.NhanVienId == currentUserId)
                .OrderByDescending(h => h.NgayTao).Take(5).ToListAsync();

            return View(newStudentsList);
        }

        // Các hàm UpdateKPI và GetKpiHistory giữ nguyên logic isAdmin tương tự
        [HttpPost]
        public async Task<IActionResult> UpdateKPI(int newTarget)
        {
            var userName = HttpContext.Session.GetString("UserName");
            var nv = await _context.NhanVien.FirstOrDefaultAsync(n => n.HoTen == userName || n.Email == userName);
            if (nv != null && newTarget > 0)
            {
                nv.KpiThang = newTarget;
                _context.Update(nv);
                await _context.SaveChangesAsync();
            }
            else
            {
                HttpContext.Session.SetInt32("AdminKPI", newTarget);
            }
            return RedirectToAction("Index");
        }
    }
}