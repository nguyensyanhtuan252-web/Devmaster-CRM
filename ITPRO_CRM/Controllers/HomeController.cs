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
            bool isAdmin = (currentEmployee?.VaiTro == 0 || userName == "admin@devmaster.edu.vn");

            // Truyền quyền isAdmin sang View để ẩn/hiện nút sửa
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

                // 2. TÍNH TOÁN TIẾN ĐỘ TỪNG NGƯỜI: Đếm số học viên đã chốt (Trạng thái 2) của mỗi nhân viên trong tháng
                var staffProgress = await _context.HocVien
                    .Where(h => h.TrangThai == 2 && h.NgayTao >= startOfMonth && h.NgayTao < endOfMonth)
                    .GroupBy(h => h.NhanVienId)
                    .Select(g => new { StaffId = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.StaffId ?? 0, x => x.Count);

                ViewBag.StaffProgress = staffProgress; // Gửi cái này sang View để hiện con số thực tế

                // 3. TÍNH TỔNG KPI TRUNG TÂM: Tổng chỉ tiêu của tất cả nhân viên
                myKpiTarget = allSales.Sum(n => n.KpiThang ?? 0);

                // 4. TỔNG THỰC TẾ TRUNG TÂM: Tổng số học viên chốt được của cả trung tâm
                myNewStudents = await kpiQuery.CountAsync();
            }
            else
            {
                // Nếu là Sale: Chỉ lấy chỉ tiêu và kết quả của cá nhân
                myKpiTarget = currentEmployee?.KpiThang ?? 0;
                myNewStudents = await kpiQuery.Where(h => h.NhanVienId == currentUserId).CountAsync();
            }

            ViewBag.MyNewStudents = myNewStudents;
            ViewBag.MyKpiTarget = myKpiTarget;
            ViewBag.KpiPercentage = myKpiTarget > 0 ? (myNewStudents * 100) / myKpiTarget : 0;

            // 5. TÍNH TOÁN LEAD TIỀM NĂNG (ĐỒNG BỘ VỚI TRANG DANH SÁCH)
            var leadQuery = _context.HocVien.Where(h => h.TrangThai == 0);
            if (!isAdmin) leadQuery = leadQuery.Where(h => h.NhanVienId == currentUserId);
            ViewBag.TotalLeads = await leadQuery.CountAsync();

            // 6. GỬI DỮ LIỆU KHÁC SANG VIEW
            ViewBag.ActiveClasses = await _context.LopHoc.CountAsync(l => l.TrangThai == 1);

            // 7. DOANH THU
            var revQuery = _context.PhieuThu.AsQueryable();
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
                .OrderBy(h => h.NgayHen).Take(5).ToListAsync();
            ViewBag.LichHenHomNay = dsLichHen;

            var newStudentsList = await _context.HocVien
                .Where(h => isAdmin || h.NhanVienId == currentUserId)
                .OrderByDescending(h => h.NgayTao).Take(5).ToListAsync();

            return View(newStudentsList);
        }

        // CHỨC NĂNG QUAN TRỌNG: Admin gán KPI cho nhân viên
        [HttpPost]
        public async Task<IActionResult> AssignKPI(int staffId, int newKpi)
        {
            // Bảo mật: Kiểm tra xem người thực hiện có phải Admin không
            var userName = HttpContext.Session.GetString("UserName");
            var currentAdmin = await _context.NhanVien.FirstOrDefaultAsync(n => n.HoTen == userName || n.Email == userName);

            if (currentAdmin == null || currentAdmin.VaiTro != 0)
            {
                return Forbid(); // Không có quyền thì cấm thực hiện
            }

            // Tìm nhân viên cần gán và cập nhật chỉ tiêu
            var staff = await _context.NhanVien.FindAsync(staffId);
            if (staff != null && newKpi >= 0)
            {
                staff.KpiThang = newKpi;
                _context.Update(staff);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        // Lấy lịch sử KPI tháng bất kỳ (Ajax gọi)
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