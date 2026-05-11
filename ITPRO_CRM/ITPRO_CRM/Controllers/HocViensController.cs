using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ITPRO_CRM.Data;
using ITPRO_CRM.Models;
// Thư viện Excel
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;

using System.IO;
using ITPRO_CRM.Filters; // BẢN SỬA: Thêm thư viện Ổ khóa phân quyền

namespace ITPRO_CRM.Controllers
{
    [PhanQuyen(LoaiVaiTro.Admin, LoaiVaiTro.Sale, LoaiVaiTro.KeToan, LoaiVaiTro.GiangVien)] // Cấp quyền vào Controller
    public class HocViensController : Controller
    {
        private readonly ITPRO_CRMContext _context;

        public HocViensController(ITPRO_CRMContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. CÁC TRANG DANH SÁCH (LOGIC MỚI)
        // ==========================================

        // --- DANH SÁCH TIỀM NĂNG (Trạng thái 0) ---
        public async Task<IActionResult> Leads()
        {
            ViewData["TitlePage"] = "Danh sách Tiềm năng";
            ViewData["CurrentStatus"] = 0;

            // BẢN SỬA: Lấy thẳng quyền và ID từ Session cho nhanh, không cần chọc Database tìm nhân viên nữa
            var roleId = HttpContext.Session.GetInt32("VaiTro");
            var userIdSession = HttpContext.Session.GetInt32("UserId");
            if (userIdSession == null) return RedirectToAction("Login", "Access");

            int currentUserId = userIdSession.Value;
            bool isAdmin = (roleId == (int)LoaiVaiTro.Admin || roleId == (int)LoaiVaiTro.KeToan);

            // Chỉ lấy Trạng thái 0
            var query = _context.HocVien
                .Include(h => h.LopHoc)
                .Include(h => h.NhanVien)
                .Where(h => h.TrangThai == 0);

            // 🔐 Phân quyền Row-level: Không phải sếp/kế toán thì chỉ lấy khách của mình
            if (!isAdmin)
            {
                query = query.Where(h => h.NhanVienId == currentUserId);
            }

            var leads = await query.OrderByDescending(h => h.NgayTao).ToListAsync();
            return View("Index", leads);
        }

        // --- DANH SÁCH ĐANG TƯ VẤN (Trạng thái 1) ---
        public async Task<IActionResult> Pipeline()
        {
            ViewData["TitlePage"] = "Danh sách Đang tư vấn";
            ViewData["CurrentStatus"] = 1;

            // BẢN SỬA: Áp dụng Session
            var roleId = HttpContext.Session.GetInt32("VaiTro");
            var userIdSession = HttpContext.Session.GetInt32("UserId");
            if (userIdSession == null) return RedirectToAction("Login", "Access");

            int currentUserId = userIdSession.Value;
            bool isAdmin = (roleId == (int)LoaiVaiTro.Admin || roleId == (int)LoaiVaiTro.KeToan);

            // Chỉ lấy Trạng thái 1
            var query = _context.HocVien
                .Include(h => h.LopHoc)
                .Include(h => h.NhanVien)
                .Where(h => h.TrangThai == 1);

            if (!isAdmin)
            {
                query = query.Where(h => h.NhanVienId == currentUserId);
            }

            var data = await query.OrderByDescending(h => h.NgayTao).ToListAsync();
            return View("Index", data);
        }

        // 🔵 HÀM INDEX TỔNG HỢP: Xử lý cả Tiềm năng (0), Tư vấn (1) và Học viên (2)
        // 🔵 HÀM INDEX TỔNG HỢP: Đã tích hợp bộ lọc Thẻ (Tabs) và tính Nợ thông minh
        public async Task<IActionResult> Index(int? trangThai, string filter)
        {
            // BẢN SỬA: Áp dụng Session
            var roleId = HttpContext.Session.GetInt32("VaiTro");
            var userIdSession = HttpContext.Session.GetInt32("UserId");
            if (userIdSession == null) return RedirectToAction("Login", "Access");

            int currentUserId = userIdSession.Value;
            bool isAdmin = (roleId == (int)LoaiVaiTro.Admin || roleId == (int)LoaiVaiTro.KeToan);

            int status = trangThai ?? 2;
            ViewData["CurrentStatus"] = status;

            // Lưu lại bộ lọc hiện tại để View biết đang đứng ở Tab nào mà ẩn/hiện cột
            ViewBag.CurrentFilter = string.IsNullOrEmpty(filter) ? "all" : filter;

            var query = _context.HocVien
                .Include(h => h.LopHoc)
                .Include(h => h.NhanVien)
                .Include(h => h.ChienDich)
                .Include(h => h.PhieuThus)
                .AsQueryable();

            if (!isAdmin) { query = query.Where(h => h.NhanVienId == currentUserId); }

            if (status == 2)
            {
                query = query.Where(h => h.TrangThai >= 2);
                ViewData["TitlePage"] = "Danh sách Học viên chính thức";
            }
            else
            {
                query = query.Where(h => h.TrangThai == status);
                ViewData["TitlePage"] = status == 0 ? "Danh sách Tiềm năng" : "Danh sách Đang tư vấn";
            }

            // ================= LOGIC LỌC THEO TAB TRÊN GIAO DIỆN =================
            if (filter == "co-lop")
            {
                query = query.Where(h => h.LopHocId != null);
            }
            else if (filter == "chua-lop")
            {
                query = query.Where(h => h.LopHocId == null);
            }
            else if (filter == "no-phi")
            {
                // CHỈ LẤY: Những đợt CHƯA ĐÓNG XONG (Trạng thái != 2) VÀ HẠN NỘP <= 7 NGÀY TỚI
                var hanChot = DateTime.Today.AddDays(7);
                var idNhungNguoiNo = await _context.DotThanhToan
                    .Where(d => d.TrangThai != 2 && d.HanThanhToan <= hanChot)
                    .Select(d => d.HocVienId)
                    .Distinct()
                    .ToListAsync();

                query = query.Where(h => idNhungNguoiNo.Contains(h.Id));
            }

            var customers = await query.OrderByDescending(h => h.NgayTao).ToListAsync();

            // ================= ĐẾM SỐ LƯỢNG CHO 4 THẺ (CARDS) TRÊN CÙNG =================
            ViewBag.CountTong = await _context.HocVien.CountAsync(h => h.TrangThai >= 2 && (isAdmin || h.NhanVienId == currentUserId));
            ViewBag.CountCoLop = await _context.HocVien.CountAsync(h => h.TrangThai >= 2 && h.LopHocId != null && (isAdmin || h.NhanVienId == currentUserId));
            ViewBag.CountChuaLop = await _context.HocVien.CountAsync(h => h.TrangThai >= 2 && h.LopHocId == null && (isAdmin || h.NhanVienId == currentUserId));

            var hanChotDem = DateTime.Today.AddDays(7);
            ViewBag.CountNoPhi = await _context.DotThanhToan
                .Include(d => d.HocVien)
                .Where(d => d.TrangThai != 2 && d.HanThanhToan <= hanChotDem && d.HocVien.TrangThai >= 2 && (isAdmin || d.HocVien.NhanVienId == currentUserId))
                .Select(d => d.HocVienId)
                .Distinct()
                .CountAsync();

            return View(customers);
        }

        // ==========================================
        // 2. CHI TIẾT & GHI CHÚ
        // ==========================================
        public async Task<IActionResult> Details(int? id)
        {
            var roleId = HttpContext.Session.GetInt32("VaiTro");
            var userIdSession = HttpContext.Session.GetInt32("UserId");

            if (id == null) return NotFound();

            var hocVien = await _context.HocVien
                .Include(h => h.LopHoc)
                .Include(h => h.NhanVien)
                .Include(h => h.ChienDich)
                .Include(h => h.PhieuThus)
                .Include(h => h.DiemDanhs)
                .Include(h => h.LichSuTuVans)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (hocVien == null) return NotFound();

            // 🔐 BẢO MẬT: CHẶN SALE XEM HỒ SƠ KHÁCH HÀNG CỦA NGƯỜI KHÁC (Chống gõ URL lén)
            if (roleId == (int)LoaiVaiTro.Sale && hocVien.NhanVienId != userIdSession)
            {
                return RedirectToAction("Index", "Home", new { error = "unauthorized" });
            }

            // Lấy danh sách các đợt đóng phí của học viên này
            ViewBag.CacDotThanhToan = await _context.DotThanhToan
                .Where(d => d.HocVienId == id)
                .OrderBy(d => d.HanThanhToan)
                .ToListAsync();

            if (hocVien.TrangThai >= 2)
                return View("DetailsStudent", hocVien);

            ViewBag.DanhSachLop = await _context.LopHoc.Where(l => l.TrangThai == 1).ToListAsync();
            return View(hocVien);
        }

        [HttpPost]
        public async Task<IActionResult> AddNote(int HocVienId, string HinhThuc, string NoiDung, string KetQua, DateTime? NgayHen)
        {
            var currentUser = HttpContext.Session.GetString("HoTen") ?? HttpContext.Session.GetString("UserName") ?? "Admin";

            // 1. Lưu nội dung lịch sử tư vấn
            var history = new LichSuTuVan
            {
                HocVienId = HocVienId,
                HinhThuc = HinhThuc,
                NoiDung = NoiDung,
                KetQua = KetQua ?? "Đang tư vấn",
                NgayTuVan = DateTime.Now,
                NguoiTuVan = currentUser
            };
            _context.Add(history);

            // 2. Tự động cập nhật Khách hàng (Đổi trạng thái & Lưu lịch hẹn)
            var hv = await _context.HocVien.FindAsync(HocVienId);
            if (hv != null)
            {
                // Chuyển Tiềm năng (0) thành Cơ hội (1) vì đã được tư vấn
                if (hv.TrangThai == 0) hv.TrangThai = 1;

                // NẾU CÓ CHỌN NGÀY HẸN LẠI THÌ LƯU VÀO HỒ SƠ KHÁCH HÀNG
                if (NgayHen.HasValue)
                {
                    hv.NgayHen = NgayHen.Value;
                    // Tóm tắt nội dung hẹn dựa trên nội dung tư vấn
                    hv.NoiDungHen = "Cần liên lạc lại: " + KetQua;
                }

                _context.Update(hv);
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "💾 Đã lưu lịch sử tư vấn và cập nhật lịch hẹn!";

            // (Tùy chọn) Truyền biến để view biết là mở tab lịch sử
            TempData["ActiveTab"] = "history";
            return RedirectToAction("Details", new { id = HocVienId });
        }

        [HttpPost]
        public async Task<IActionResult> DatLichHen(int HocVienId, DateTime NgayHen, string NoiDungHen)
        {
            var hv = await _context.HocVien.FindAsync(HocVienId);
            if (hv != null)
            {
                hv.NgayHen = NgayHen;
                hv.NoiDungHen = NoiDungHen;

                // Tạo thêm 1 dòng lịch sử tư vấn để lưu lại vết "Đã đặt lịch hẹn"
                var currentUser = HttpContext.Session.GetString("HoTen") ?? HttpContext.Session.GetString("UserName") ?? "Admin";
                _context.Add(new LichSuTuVan
                {
                    HocVienId = HocVienId,
                    HinhThuc = "Hẹn lịch",
                    NoiDung = $"Hẹn gọi lại vào {NgayHen:dd/MM/yyyy HH:mm} - Nội dung: {NoiDungHen}",
                    KetQua = "Chờ tới lịch",
                    NgayTuVan = DateTime.Now,
                    NguoiTuVan = currentUser
                });

                await _context.SaveChangesAsync();
                TempData["Success"] = "⏰ Đã lên lịch hẹn thành công!";
            }
            return RedirectToAction("Details", new { id = HocVienId });
        }

        // ==========================================
        // 3. TẠO MỚI (CREATE)
        // ==========================================
        public IActionResult Create()
        {
            if (HttpContext.Session.GetString("UserName") == null) return RedirectToAction("Login", "Access");
            ViewData["LopHocId"] = new SelectList(_context.LopHoc, "Id", "TenLop");
            ViewData["ChienDichId"] = new SelectList(_context.ChienDich.Where(c => c.DangHoatDong == true), "Id", "TenChienDich");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(HocVien hocVien)
        {
            if (HttpContext.Session.GetString("UserName") == null) return RedirectToAction("Login", "Access");

            if (!string.IsNullOrWhiteSpace(hocVien.Email) && _context.HocVien.Any(x => x.Email == hocVien.Email))
            {
                ModelState.AddModelError("Email", "Email này đã tồn tại!");
            }

            if (ModelState.IsValid)
            {
                if (hocVien.NgayTao == DateTime.MinValue) hocVien.NgayTao = DateTime.Now;

                if (hocVien.LopHocId != null)
                {
                    hocVien.TrangThai = 2; // Có lớp -> Học viên chính thức
                }
                else
                {
                    if (hocVien.TrangThai == 0 || hocVien.TrangThai == null) hocVien.TrangThai = 0;
                }

                var currentUserId = HttpContext.Session.GetInt32("UserId");
                if (currentUserId != null)
                {
                    hocVien.NhanVienId = currentUserId;
                }

                _context.Add(hocVien);
                await _context.SaveChangesAsync();

                // ===============================================================
                // TẠO LỘ TRÌNH ĐÓNG HỌC PHÍ (CHỈ KHI ĐĂNG KÝ VÀO LỚP)
                // ===============================================================
                if (hocVien.LopHocId != null)
                {
                    var lopHoc = await _context.LopHoc.FindAsync(hocVien.LopHocId);
                    if (lopHoc != null)
                    {
                        decimal tongHocPhi = lopHoc.HocPhi;

                        // Đợt 1: 60%
                        var dot1 = new DotThanhToan
                        {
                            HocVienId = hocVien.Id,
                            TenDot = "Đợt 1 (Đóng 60%)",
                            SoTienPhaiThu = tongHocPhi * 0.6m,
                            SoTienDaThu = 0,
                            HanThanhToan = DateTime.Now,
                            TrangThai = 0
                        };

                        // Đợt 2: 40% - Sau 30 ngày
                        var dot2 = new DotThanhToan
                        {
                            HocVienId = hocVien.Id,
                            TenDot = "Đợt 2 (Đóng 40%)",
                            SoTienPhaiThu = tongHocPhi * 0.4m,
                            SoTienDaThu = 0,
                            HanThanhToan = DateTime.Now.AddDays(30),
                            TrangThai = 0
                        };

                        _context.DotThanhToan.AddRange(dot1, dot2);
                        await _context.SaveChangesAsync();
                    }
                }
                // ===============================================================

                TempData["Success"] = "🎉 Đã thêm thành công!";

                if (hocVien.TrangThai == 0) return RedirectToAction(nameof(Leads));
                if (hocVien.TrangThai == 1) return RedirectToAction(nameof(Pipeline));
                return RedirectToAction(nameof(Index));
            }

            ViewData["LopHocId"] = new SelectList(_context.LopHoc, "Id", "TenLop", hocVien.LopHocId);
            ViewData["ChienDichId"] = new SelectList(_context.ChienDich, "Id", "TenChienDich", hocVien.ChienDichId);
            return View(hocVien);
        }

        // ==========================================
        // 4. CHỈNH SỬA & CÁC HÀM KHÁC
        // ==========================================
        public async Task<IActionResult> Edit(int? id)
        {
            var roleId = HttpContext.Session.GetInt32("VaiTro");
            var userIdSession = HttpContext.Session.GetInt32("UserId");
            if (userIdSession == null) return RedirectToAction("Login", "Access");
            if (id == null) return NotFound();

            var hocVien = await _context.HocVien.FindAsync(id);
            if (hocVien == null) return NotFound();

            // 🔐 BẢO MẬT: CHẶN SALE SỬA HỒ SƠ KHÁCH HÀNG CỦA NGƯỜI KHÁC
            if (roleId == (int)LoaiVaiTro.Sale && hocVien.NhanVienId != userIdSession)
            {
                return RedirectToAction("Index", "Home", new { error = "unauthorized" });
            }

            ViewData["LopHocId"] = new SelectList(_context.LopHoc, "Id", "TenLop", hocVien.LopHocId);
            ViewData["ChienDichId"] = new SelectList(_context.ChienDich, "Id", "TenChienDich", hocVien.ChienDichId);
            return View(hocVien);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, HocVien hocVien)
        {
            if (id != hocVien.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(hocVien);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!HocVienExists(hocVien.Id)) return NotFound();
                    else throw;
                }
                TempData["Success"] = "💾 Cập nhật thành công!";

                if (hocVien.TrangThai == 0) return RedirectToAction(nameof(Leads));
                if (hocVien.TrangThai == 1) return RedirectToAction(nameof(Pipeline));
                return RedirectToAction(nameof(Index));
            }
            ViewData["LopHocId"] = new SelectList(_context.LopHoc, "Id", "TenLop", hocVien.LopHocId);
            ViewData["ChienDichId"] = new SelectList(_context.ChienDich, "Id", "TenChienDich", hocVien.ChienDichId);
            return View(hocVien);
        }

        // 🔐 CHỈ ADMIN MỚI ĐƯỢC XÓA KHÁCH HÀNG
        [PhanQuyen(LoaiVaiTro.Admin)]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var hocVien = await _context.HocVien.Include(h => h.LopHoc).FirstOrDefaultAsync(m => m.Id == id);
            return hocVien == null ? NotFound() : View(hocVien);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [PhanQuyen(LoaiVaiTro.Admin)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var hocVien = await _context.HocVien.FindAsync(id);
            if (hocVien != null)
            {
                _context.HocVien.Remove(hocVien);
                await _context.SaveChangesAsync();
            }
            TempData["Success"] = "🗑️ Đã xóa thành công!";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ChangeStatus(int id, int status)
        {
            var hv = await _context.HocVien.FindAsync(id);
            if (hv != null)
            {
                hv.TrangThai = status;
                await _context.SaveChangesAsync();
            }
            if (status == 0) return RedirectToAction(nameof(Leads));
            if (status == 1) return RedirectToAction(nameof(Pipeline));
            return RedirectToAction(nameof(Index));
        }

        public IActionResult ExportToExcel()
        {
            var danhSach = _context.HocVien.Include(h => h.LopHoc).ToList();
            ExcelPackage.License.SetNonCommercialPersonal("Student");
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("DanhSach");
                worksheet.Cells[1, 1].Value = "ID"; worksheet.Cells[1, 2].Value = "Họ Tên";
                worksheet.Cells[1, 3].Value = "SĐT"; worksheet.Cells[1, 4].Value = "Trạng Thái";

                int row = 2;
                foreach (var item in danhSach)
                {
                    worksheet.Cells[row, 1].Value = item.Id;
                    worksheet.Cells[row, 2].Value = item.HoTen;
                    worksheet.Cells[row, 3].Value = item.SoDienThoai;
                    string stt = "Tiềm năng";
                    if (item.TrangThai == 1) stt = "Cơ hội";
                    if (item.TrangThai == 2) stt = "Học viên";
                    worksheet.Cells[row, 4].Value = stt;
                    row++;
                }
                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "DanhSach.xlsx");
            }
        }

        private bool HocVienExists(int id) => _context.HocVien.Any(e => e.Id == id);

        [HttpPost]
        public async Task<IActionResult> ThemPhieuThu(int HocVienId, decimal SoTien, int HinhThuc, string NoiDung)
        {
            var currentUser = HttpContext.Session.GetString("HoTen") ?? HttpContext.Session.GetString("UserName") ?? "Admin";

            // Lưu Phiếu thu vào lịch sử
            var phieuThu = new PhieuThu
            {
                MaPhieu = "PT" + DateTime.Now.ToString("yyMMddHHmm"),
                HocVienId = HocVienId,
                SoTien = SoTien,
                HinhThuc = HinhThuc,
                NoiDung = NoiDung,
                NgayThu = DateTime.Now,
                NguoiThu = currentUser
            };
            _context.Add(phieuThu);

            // LOGIC GẠCH NỢ: Rót tiền vào các đợt còn thiếu
            var cacDotNo = await _context.DotThanhToan
                .Where(d => d.HocVienId == HocVienId && d.TrangThai != 2)
                .OrderBy(d => d.HanThanhToan).ToListAsync();

            decimal tienConLai = SoTien;
            foreach (var dot in cacDotNo)
            {
                if (tienConLai <= 0) break;
                decimal canThu = dot.SoTienPhaiThu - dot.SoTienDaThu;
                decimal thucThu = Math.Min(tienConLai, canThu);
                dot.SoTienDaThu += thucThu;
                tienConLai -= thucThu;
                dot.TrangThai = (dot.SoTienDaThu >= dot.SoTienPhaiThu) ? 2 : 1;
                _context.Update(dot);
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "💰 Đã thu tiền và khấu trừ công nợ thành công!";
            TempData["ActiveTab"] = "finance";
            return RedirectToAction("Details", new { id = HocVienId });
        }

        [HttpPost]
        public async Task<IActionResult> ImportExcel(IFormFile fileExcel)
        {
            if (fileExcel == null || fileExcel.Length == 0)
            {
                TempData["Error"] = "❌ Vui lòng chọn file Excel để tải lên!";
                return RedirectToAction("Leads");
            }

            var extension = Path.GetExtension(fileExcel.FileName).ToLower();
            if (extension != ".xlsx")
            {
                TempData["Error"] = "❌ Chỉ chấp nhận file định dạng chuẩn .xlsx!";
                return RedirectToAction("Leads");
            }

            try
            {
                var listLeads = new List<HocVien>();

                // Lấy danh sách Sale để chia vòng tròn (KHÔNG DÙNG SESSION NỮA)
                var danhSachSale = await _context.NhanVien.Where(nv => (int)nv.VaiTro == 1).OrderBy(nv => nv.Id).ToListAsync();
                if (!danhSachSale.Any())
                {
                    TempData["Error"] = "❌ Hệ thống chưa có nhân viên Sale nào để chia khách!";
                    return RedirectToAction("Leads");
                }

                int indexSale = 0;
                int tongSoSale = danhSachSale.Count;

                // Cấp phép sử dụng thư viện EPPlus
                // Khai báo bản quyền phi thương mại theo chuẩn EPPlus 8+
                ExcelPackage.License.SetNonCommercialOrganization("ITPRO_CRM");

                using (var stream = new MemoryStream())
                {
                    await fileExcel.CopyToAsync(stream);
                    stream.Position = 0;

                    using (var package = new ExcelPackage(stream))
                    {
                        var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                        if (worksheet == null)
                        {
                            TempData["Error"] = "❌ File Excel bị hỏng hoặc không có Sheet nào.";
                            return RedirectToAction("Leads");
                        }

                        int rowCount = worksheet.Dimension?.Rows ?? 0;
                        int soDongTrongLienTiep = 0;

                        for (int row = 2; row <= rowCount; row++)
                        {
                            // Đọc dữ liệu cực kỳ an toàn với .Text của EPPlus
                            var hoTen = worksheet.Cells[row, 1].Text.Trim();
                            var sdt = worksheet.Cells[row, 2].Text.Trim();

                            if (string.IsNullOrEmpty(hoTen) && string.IsNullOrEmpty(sdt))
                            {
                                soDongTrongLienTiep++;
                                if (soDongTrongLienTiep >= 5) break; // Nếu gặp 5 dòng trống liên tiếp -> Đáy danh sách -> Đập vỡ vòng lặp ngay lập tức!
                                continue;
                            }

                            soDongTrongLienTiep = 0;

                            listLeads.Add(new HocVien
                            {
                                HoTen = string.IsNullOrEmpty(hoTen) ? "Khách hàng mới" : hoTen,
                                SoDienThoai = sdt,
                                Email = worksheet.Cells[row, 3].Text.Trim(),
                                GioiTinh = string.IsNullOrEmpty(worksheet.Cells[row, 4].Text.Trim()) ? "Chưa rõ" : worksheet.Cells[row, 4].Text.Trim(),
                                DiaChi = worksheet.Cells[row, 5].Text.Trim(),
                                FacebookLink = worksheet.Cells[row, 6].Text.Trim(),
                                ZaloLink = worksheet.Cells[row, 7].Text.Trim(),
                                MucTieuHocTap = worksheet.Cells[row, 8].Text.Trim(),
                                TrangThai = 0,
                                NgayTao = DateTime.Now,
                                NgaySinh = new DateTime(2000, 1, 1),
                                NhanVienId = danhSachSale[indexSale % tongSoSale].Id, // Chia đều khách cho từng Sale
                                NguonGoc = "Import từ Excel"
                            });

                            indexSale++;
                        }
                    }
                }

                if (listLeads.Any())
                {
                    // 1. Lưu khách hàng trước để lấy ID (dùng cho LinkUrl trong thông báo)
                    await _context.HocVien.AddRangeAsync(listLeads);
                    await _context.SaveChangesAsync();

                    // 2. TẠO DANH SÁCH THÔNG BÁO (NOTIFICATION)
                    var listThongBao = new List<ThongBao>();

                    // Thông báo cho từng Sale được chia khách
                    foreach (var lead in listLeads)
                    {
                        if (lead.NhanVienId.HasValue)
                        {
                            listThongBao.Add(new ThongBao
                            {
                                NhanVienId = lead.NhanVienId.Value,
                                TieuDe = "📥 Khách mới từ Excel",
                                NoiDung = $"Bạn được chia khách: {lead.HoTen}",
                                LinkUrl = $"/HocViens/Details/{lead.Id}", // Bấm vào chuông nhảy thẳng đến khách đó
                                NgayTao = DateTime.Now,
                                DaDoc = false
                            });
                        }
                    }

                    // Gửi 1 thông báo tổng hợp cho các Admin (VaiTro = 0)
                    var adminIds = await _context.NhanVien
                                                 .Where(nv => (int)nv.VaiTro == 0)
                                                 .Select(nv => nv.Id)
                                                 .ToListAsync();

                    foreach (var adminId in adminIds)
                    {
                        listThongBao.Add(new ThongBao
                        {
                            NhanVienId = adminId,
                            TieuDe = "📊 Hệ thống vừa nạp Excel",
                            NoiDung = $"Vừa nhập thành công {listLeads.Count} khách hàng mới.",
                            LinkUrl = "/HocViens/Leads",
                            NgayTao = DateTime.Now,
                            DaDoc = false
                        });
                    }

                    // 3. Lưu tất cả thông báo vào DB
                    if (listThongBao.Any())
                    {
                        await _context.ThongBao.AddRangeAsync(listThongBao);
                        await _context.SaveChangesAsync();
                    }

                    TempData["Success"] = $"✅ Đã nhập và chia đều {listLeads.Count} khách hàng thành công!";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "❌ Lỗi hệ thống: " + ex.Message;
            }

            return RedirectToAction("Leads");
        }

        [HttpGet]
        public async Task<IActionResult> ExportExcel(int? status)
        {
            var roleId = HttpContext.Session.GetInt32("VaiTro");
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            bool isAdmin = (roleId == (int)LoaiVaiTro.Admin || roleId == (int)LoaiVaiTro.KeToan);

            // 1. Lọc danh sách theo đúng tab đang đứng
            var query = _context.HocVien
                .Include(h => h.LopHoc)
                .Include(h => h.NhanVien)
                .Include(h => h.ChienDich)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(h => h.TrangThai == status.Value);
            }

            // Chặn Sale xuất Excel dữ liệu của toàn trung tâm
            if (!isAdmin && currentUserId.HasValue)
            {
                query = query.Where(h => h.NhanVienId == currentUserId.Value);
            }

            var danhSach = await query.OrderByDescending(h => h.NgayTao).ToListAsync();

            // 2. Dùng EPPlus tạo file Excel
            // Khai báo bản quyền phi thương mại theo chuẩn EPPlus 8+
            ExcelPackage.License.SetNonCommercialOrganization("ITPRO_CRM");
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Danh_Sach_Data");

                // Đổ tiêu đề cột
                worksheet.Cells[1, 1].Value = "STT";
                worksheet.Cells[1, 2].Value = "Họ và Tên";
                worksheet.Cells[1, 3].Value = "Số điện thoại";
                worksheet.Cells[1, 4].Value = "Email";
                worksheet.Cells[1, 5].Value = "Giới tính";
                worksheet.Cells[1, 6].Value = "Ngày sinh";
                worksheet.Cells[1, 7].Value = "Lớp học";
                worksheet.Cells[1, 8].Value = "Nguồn (Chiến dịch)";
                worksheet.Cells[1, 9].Value = "Sale phụ trách";
                worksheet.Cells[1, 10].Value = "Ngày tạo";
                worksheet.Cells[1, 11].Value = "Địa chỉ";
                worksheet.Cells[1, 12].Value = "Facebook";

                // Trang trí cho dòng tiêu đề nổi bật
                using (var range = worksheet.Cells[1, 1, 1, 12])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Teal);
                    range.Style.Font.Color.SetColor(System.Drawing.Color.White);
                    range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                }

                // 3. Chạy vòng lặp để đắp dữ liệu
                int row = 2;
                int stt = 1;
                foreach (var item in danhSach)
                {
                    worksheet.Cells[row, 1].Value = stt++;
                    worksheet.Cells[row, 2].Value = item.HoTen;
                    worksheet.Cells[row, 3].Value = "'" + item.SoDienThoai; // Thêm nháy để không mất số 0
                    worksheet.Cells[row, 4].Value = item.Email;
                    worksheet.Cells[row, 5].Value = item.GioiTinh;
                    worksheet.Cells[row, 6].Value = item.NgaySinh.ToString("dd/MM/yyyy");
                    worksheet.Cells[row, 7].Value = item.LopHoc?.TenLop ?? "Chưa có";
                    worksheet.Cells[row, 8].Value = item.ChienDich?.TenChienDich ?? "Tự nhiên";
                    worksheet.Cells[row, 9].Value = item.NhanVien?.HoTen ?? "Admin";
                    worksheet.Cells[row, 10].Value = item.NgayTao?.ToString("dd/MM/yyyy") ?? "";
                    worksheet.Cells[row, 11].Value = item.DiaChi;
                    worksheet.Cells[row, 12].Value = item.FacebookLink;
                    row++;
                }

                // Tự động căn chỉnh độ rộng cột
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                // 4. Trả file về trình duyệt an toàn (Chống sập web)
                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0; // Tua lại đầu file

                string fileName = status == 0 ? "DanhSach_TiemNang.xlsx" :
                                  status == 1 ? "DanhSach_CoHoi.xlsx" :
                                  status == 2 ? "DanhSach_HocVien.xlsx" : "DanhSach_TongHop.xlsx";

                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }

        [HttpGet]
        public IActionResult DownloadTemplate()
        {
            // Khai báo bản quyền phi thương mại theo chuẩn EPPlus 8+
            ExcelPackage.License.SetNonCommercialOrganization("ITPRO_CRM");
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Mau_Nhap_Tiem_Nang");

                // 1. TẠO DÒNG TIÊU ĐỀ
                worksheet.Cells[1, 1].Value = "Họ và Tên (*)";
                worksheet.Cells[1, 2].Value = "Số điện thoại (*)";
                worksheet.Cells[1, 3].Value = "Email";
                worksheet.Cells[1, 4].Value = "Giới tính (Nam/Nữ)";
                worksheet.Cells[1, 5].Value = "Địa chỉ";
                worksheet.Cells[1, 6].Value = "Link Facebook";
                worksheet.Cells[1, 7].Value = "Số Zalo";
                worksheet.Cells[1, 8].Value = "Mục tiêu/Ghi chú";

                // Trang trí dòng tiêu đề
                using (var range = worksheet.Cells[1, 1, 1, 8])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Font.Color.SetColor(System.Drawing.Color.White);
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.DarkCyan);
                    range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                }

                // 2. TẠO DỮ LIỆU MẪU ĐỂ SALE BIẾT ĐƯỜNG NHẬP
                worksheet.Cells[2, 1].Value = "Nguyễn Văn A";
                worksheet.Cells[2, 2].Value = "0987654321";
                worksheet.Cells[2, 3].Value = "nguyenvana@gmail.com";
                worksheet.Cells[2, 4].Value = "Nam";
                worksheet.Cells[2, 5].Value = "Hà Nội";
                worksheet.Cells[2, 6].Value = "facebook.com/nva";
                worksheet.Cells[2, 7].Value = "0987654321";
                worksheet.Cells[2, 8].Value = "Tư vấn khóa .NET";

                // Ghi chú thêm cho Sale
                worksheet.Cells[5, 1].Value = "LƯU Ý:";
                worksheet.Cells[5, 1].Style.Font.Bold = true;
                worksheet.Cells[5, 1].Style.Font.Color.SetColor(System.Drawing.Color.Red);
                worksheet.Cells[6, 1].Value = "- Không được xóa dòng tiêu đề đầu tiên.";
                worksheet.Cells[7, 1].Value = "- Họ tên và Số điện thoại là bắt buộc phải nhập.";

                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                // 3. XUẤT THÀNH FILE .XLSX
                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "FileMau_NhapTiemNang.xlsx");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnrollStudent(int id, int lopHocId, decimal hocPhi, decimal giamTru, string lyDoGiamGia, decimal daNop, string hinhThuc, string ghiChu)
        {
            var hocVien = await _context.HocVien.FindAsync(id);
            if (hocVien == null) return NotFound();

            try
            {
                hocVien.TrangThai = 2;
                hocVien.LopHocId = lopHocId;
                hocVien.MaHocVien = "HV" + DateTime.Now.ToString("yyMMddHHmm");
                _context.Update(hocVien);

                decimal tongDaNopVaGiam = 0; // Biến này chứa tổng (Tiền mặt + Voucher)

                // 1. TẠO PHIẾU GHI NHẬN MÃ KHUYẾN MÃI (Nếu có)
                if (giamTru > 0)
                {
                    _context.PhieuThu.Add(new PhieuThu
                    {
                        MaPhieu = "KM" + DateTime.Now.ToString("yyMMddHHmm"),
                        HocVienId = hocVien.Id,
                        SoTien = giamTru,
                        NgayThu = DateTime.Now,
                        NoiDung = "Áp dụng giảm trừ / Khuyến mãi. Lý do: " + lyDoGiamGia,
                        NguoiThu = HttpContext.Session.GetString("HoTen") ?? HttpContext.Session.GetString("UserName") ?? "Hệ thống",
                        HinhThuc = 3 // Quy ước 3 là Mã khuyến mãi
                    });
                    tongDaNopVaGiam += giamTru;
                }

                // 2. TẠO PHIẾU THU TIỀN MẶT/CHUYỂN KHOẢN (Nếu có)
                if (daNop > 0)
                {
                    _context.PhieuThu.Add(new PhieuThu
                    {
                        MaPhieu = "PT" + DateTime.Now.ToString("yyMMddHHmm"),
                        HocVienId = hocVien.Id,
                        SoTien = daNop,
                        NgayThu = DateTime.Now,
                        NoiDung = "Thu học phí nhập học." + (string.IsNullOrEmpty(ghiChu) ? "" : " Ghi chú: " + ghiChu),
                        NguoiThu = HttpContext.Session.GetString("HoTen") ?? HttpContext.Session.GetString("UserName") ?? "Admin",
                        HinhThuc = hinhThuc == "Tiền mặt" ? 1 : 2
                    });
                    tongDaNopVaGiam += daNop;
                }

                // 3. CHIA ĐỢT THANH TOÁN (Luôn dựa trên TỔNG HỌC PHÍ GỐC)
                decimal tienDeGachNo = tongDaNopVaGiam;

                // Đợt 1 (60%)
                decimal phaiThuD1 = hocPhi * 0.6m; // Tính 60% của 6 triệu = 3.6tr
                decimal thucThuD1 = Math.Min(tienDeGachNo, phaiThuD1);
                tienDeGachNo -= thucThuD1;
                _context.DotThanhToan.Add(new DotThanhToan
                {
                    HocVienId = hocVien.Id,
                    TenDot = "Đợt 1 (Đóng 60%)",
                    SoTienPhaiThu = phaiThuD1,
                    SoTienDaThu = thucThuD1,
                    HanThanhToan = DateTime.Now,
                    TrangThai = (thucThuD1 >= phaiThuD1) ? 2 : (thucThuD1 > 0 ? 1 : 0)
                });

                // Đợt 2 (40%)
                decimal phaiThuD2 = hocPhi * 0.4m; // Tính 40% của 6 triệu = 2.4tr
                decimal thucThuD2 = Math.Min(tienDeGachNo, phaiThuD2);
                _context.DotThanhToan.Add(new DotThanhToan
                {
                    HocVienId = hocVien.Id,
                    TenDot = "Đợt 2 (Đóng 40%)",
                    SoTienPhaiThu = phaiThuD2,
                    SoTienDaThu = thucThuD2,
                    HanThanhToan = DateTime.Now.AddDays(30),
                    TrangThai = (thucThuD2 >= phaiThuD2) ? 2 : (thucThuD2 > 0 ? 1 : 0)
                });

                await _context.SaveChangesAsync();
                TempData["Success"] = "🎉 Nhập học thành công!";
            }
            catch (Exception ex) { TempData["Error"] = "Lỗi: " + ex.Message; }
            return RedirectToAction("Details", new { id = id });
        }

        [HttpPost]
        [HttpPost]
        public async Task<IActionResult> AddTeacherNote(int HocVienId, string GhiChuGiaoVien)
        {
            // 1. Tìm thông tin học viên để lấy tên và ID của Sale phụ trách
            var hv = await _context.HocVien.FindAsync(HocVienId);
            if (hv == null) return NotFound();

            // 2. Lấy buổi điểm danh gần nhất của học viên để gắn ghi chú vào
            var diemDanh = await _context.DiemDanh
                .Where(d => d.HocVienId == HocVienId)
                .OrderByDescending(d => d.NgayDiemDanh)
                .FirstOrDefaultAsync();

            if (diemDanh != null)
            {
                // Nếu đã có buổi điểm danh, cập nhật ghi chú vào buổi đó
                diemDanh.GhiChuGiaoVien = GhiChuGiaoVien;
                diemDanh.NgayGhiChu = DateTime.Now; // Lưu chính xác phút giây giáo viên viết
                _context.Update(diemDanh);
            }
            else
            {
                // Nếu học viên mới tinh chưa điểm danh buổi nào, tạo 1 bản ghi ảo để lưu lời nhắn
                var newDiemDanh = new DiemDanh
                {
                    HocVienId = HocVienId,
                    NgayDiemDanh = DateTime.Now,
                    TrangThai = 1, // Mặc định là có mặt
                    GhiChuGiaoVien = GhiChuGiaoVien,
                    NgayGhiChu = DateTime.Now
                };
                _context.Add(newDiemDanh);
            }

            // 3. 🔥 GẮN NGÒI NỔ: BẮN THÔNG BÁO CHO SALE PHỤ TRÁCH
            if (hv.NhanVienId != null)
            {
                var noti = new ThongBao
                {
                    NhanVienId = hv.NhanVienId.Value, // Bắn cho ông Sale đang quản lý học viên này
                    TieuDe = "⚠️ Phản ánh từ Giảng viên",
                    NoiDung = $"Học viên {hv.HoTen} bị phản ánh: {GhiChuGiaoVien}",
                    LinkUrl = $"/HocViens/Details/{hv.Id}", // Bấm vào chuông nhảy thẳng đến trang chi tiết
                    NgayTao = DateTime.Now,
                    DaDoc = false
                };
                _context.ThongBao.Add(noti);
            }

            // 4. Lưu tất cả thay đổi (Ghi chú điểm danh + Cảnh báo Sale) vào Database cùng 1 lúc
            await _context.SaveChangesAsync();

            TempData["Success"] = "📢 Đã lưu phản ánh và gửi thông báo nhắc nhở cho bộ phận Sale!";
            TempData["ActiveTab"] = "academic"; // Load lại giữ nguyên Tab điểm danh

            return RedirectToAction("Details", new { id = HocVienId });
        }
    }
}