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
using ClosedXML.Excel;
using System.IO;

namespace ITPRO_CRM.Controllers
{
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

        // 🟢 TIỀM NĂNG (LEADS) -> Trạng thái 0 (Chưa liên hệ)
        // 🟢 TIỀM NĂNG (LEADS) -> Trạng thái 0
        // --- DANH SÁCH TIỀM NĂNG (Trạng thái 0) ---
        public async Task<IActionResult> Leads()
        {
            ViewData["TitlePage"] = "Danh sách Tiềm năng";
            ViewData["CurrentStatus"] = 0;

            var userName = HttpContext.Session.GetString("UserName");
            if (userName == null) return RedirectToAction("Login", "Access");

            var currentEmployee = await _context.NhanVien.FirstOrDefaultAsync(n => n.HoTen == userName || n.Email == userName);
            int currentUserId = currentEmployee?.Id ?? 0;
            bool isAdmin = (currentEmployee?.VaiTro == 0 || userName == "admin@devmaster.edu.vn");

            // Chỉ lấy Trạng thái 0
            var query = _context.HocVien
                .Include(h => h.LopHoc)
                .Include(h => h.NhanVien)
                .Where(h => h.TrangThai == 0);

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

            var userName = HttpContext.Session.GetString("UserName");
            if (userName == null) return RedirectToAction("Login", "Access");

            var currentEmployee = await _context.NhanVien.FirstOrDefaultAsync(n => n.HoTen == userName || n.Email == userName);
            int currentUserId = currentEmployee?.Id ?? 0;
            bool isAdmin = (currentEmployee?.VaiTro == 0 || userName == "admin@devmaster.edu.vn");

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

        // 🔵 HỌC VIÊN CHÍNH THỨC -> Trạng thái 2
        public async Task<IActionResult> Index()
        {
            ViewData["TitlePage"] = "Danh sách Học viên chính thức";
            ViewData["CurrentStatus"] = 2; // Đánh dấu menu active

            var customers = await _context.HocVien
                .Where(h => h.TrangThai == 2) // LỌC CHUẨN: Chỉ lấy số 2
                .Include(h => h.LopHoc)
                .Include(h => h.NhanVien)
                .Include(h => h.ChienDich)
                .Include(h => h.PhieuThus) // 👉 BỔ SUNG QUAN TRỌNG: Kéo theo Phiếu thu để tính Nợ học phí
                .OrderByDescending(h => h.NgayTao)
                .ToListAsync();

            return View(customers);
        }

        // ==========================================
        // 2. CHI TIẾT & GHI CHÚ
        // ==========================================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var hocVien = await _context.HocVien
                .Include(h => h.LopHoc)
                .Include(h => h.NhanVien)
                .Include(h => h.ChienDich)
                .Include(h => h.PhieuThus) // Load thêm dữ liệu kế toán
                .Include(h => h.DiemDanhs)
                .Include(h => h.LichSuTuVans)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (hocVien == null) return NotFound();
            if (hocVien.TrangThai == 2)
            {
                // Nếu là HỌC VIÊN CHÍNH THỨC -> Mở file giao diện "xịn sò" (DetailsStudent)
                return View("DetailsStudent", hocVien);
            }
            else
            {
                // Nếu là TIỀM NĂNG / CƠ HỘI -> Mở file giao diện tập trung chốt sale (Details)
                ViewBag.DanhSachLop = await _context.LopHoc.Where(l => l.TrangThai == 1).ToListAsync();
                return View(hocVien);
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddNote(int HocVienId, string HinhThuc, string NoiDung, string KetQua, DateTime? NgayHen)
        {
            var currentUser = HttpContext.Session.GetString("UserName") ?? "Admin";

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
                var currentUser = HttpContext.Session.GetString("UserName") ?? "Admin";
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

            if (_context.HocVien.Any(x => x.Email == hocVien.Email))
            {
                ModelState.AddModelError("Email", "Email này đã tồn tại!");
            }

            if (ModelState.IsValid)
            {
                if (hocVien.NgayTao == DateTime.MinValue) hocVien.NgayTao = DateTime.Now;

                // --- LOGIC PHÂN LOẠI TỰ ĐỘNG ---
                if (hocVien.LopHocId != null)
                {
                    hocVien.TrangThai = 2; // Có lớp -> Học viên
                }
                else
                {
                    if (hocVien.TrangThai == 0 || hocVien.TrangThai == null) hocVien.TrangThai = 0;
                }

                // Lấy ID của nhân viên đang đăng nhập từ Session
                // Lưu ý: Đảm bảo khi Login bạn đã lưu Session["UserId"]
                var currentUserId = HttpContext.Session.GetInt32("UserId");
                if (currentUserId != null)
                {
                    hocVien.NhanVienId = currentUserId; // Gán học viên cho nhân viên này
                }

                _context.Add(hocVien);
                await _context.SaveChangesAsync();

                // --- MỚI: LOGIC CẬP NHẬT KPI TRÊN DASHBOARD ---
                // Sau khi lưu học viên, ta tính toán lại số lượng đạt được của nhân viên
                // Nếu bạn muốn KPI tăng lên mỗi khi thêm 1 HocVien bất kỳ:
                if (currentUserId != null)
                {
                    // Đếm tổng số học viên mà nhân viên này đã thêm trong tháng hiện tại
                    var totalLeadsThisMonth = _context.HocVien
                        .Count(h => h.NhanVienId == currentUserId && h.NgayTao.Month == DateTime.Now.Month);

                    // Bạn có thể lưu giá trị này vào ViewBag hoặc một bảng tạm 
                    // Nhưng cách chuẩn nhất là để Dashboard tự đếm (Count) mỗi khi Load trang.
                }

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
            if (HttpContext.Session.GetString("UserName") == null) return RedirectToAction("Login", "Access");
            if (id == null) return NotFound();

            var hocVien = await _context.HocVien.FindAsync(id);
            if (hocVien == null) return NotFound();

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

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var hocVien = await _context.HocVien.Include(h => h.LopHoc).FirstOrDefaultAsync(m => m.Id == id);
            return hocVien == null ? NotFound() : View(hocVien);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
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
            // 1. Lấy tên người đang thao tác (Nếu chưa đăng nhập thì để Admin)
            var currentUser = HttpContext.Session.GetString("UserName") ?? "Admin";

            // 2. Tạo đối tượng Phiếu Thu mới
            var phieuThu = new PhieuThu
            {
                HocVienId = HocVienId,
                SoTien = SoTien,
                HinhThuc = HinhThuc,
                NoiDung = NoiDung,
                NgayThu = DateTime.Now,
                NguoiThu = currentUser
            };

            // 3. Lưu vào Database
            _context.Add(phieuThu);
            await _context.SaveChangesAsync();

            // 4. Thông báo thành công và Quay lại đúng tab Tài chính
            TempData["Success"] = "💰 Đã thu tiền thành công!";
            TempData["ActiveTab"] = "finance";

            return RedirectToAction("Details", new { id = HocVienId });
        }
        [HttpPost]
        public async Task<IActionResult> ImportExcel(IFormFile fileExcel)
        {
            // 1. Kiểm tra file đầu vào
            if (fileExcel == null || fileExcel.Length == 0)
            {
                TempData["Error"] = "❌ Vui lòng chọn file Excel để tải lên!";
                return RedirectToAction("Leads");
            }

            var extension = Path.GetExtension(fileExcel.FileName).ToLower();
            if (extension != ".xlsx" && extension != ".xls")
            {
                TempData["Error"] = "❌ Chỉ chấp nhận file định dạng .xlsx hoặc .xls!";
                return RedirectToAction("Leads");
            }

            try
            {
                var listLeads = new List<HocVien>();
                var currentUser = HttpContext.Session.GetString("UserName") ?? "Admin";

                // 2. Mở file excel trực tiếp từ bộ nhớ đệm (MemoryStream)
                using (var stream = new MemoryStream())
                {
                    await fileExcel.CopyToAsync(stream);

                    using (var workbook = new XLWorkbook(stream))
                    {
                        // Lấy Sheet đầu tiên trong file Excel
                        var worksheet = workbook.Worksheet(1);
                        var rowCount = worksheet.RowsUsed().Count();

                        // 3. Vòng lặp đọc dữ liệu (Bắt đầu từ dòng 2 để chừa dòng 1 làm Tiêu đề cột)
                        for (int row = 2; row <= rowCount; row++)
                        {
                            // Lấy dữ liệu từng cột (Cột 1: Tên, Cột 2: SĐT, Cột 3: Email, Cột 4: Giới tính)
                            var hoTen = worksheet.Cell(row, 1).Value.ToString().Trim();
                            var sdt = worksheet.Cell(row, 2).Value.ToString().Trim();
                            var email = worksheet.Cell(row, 3).Value.ToString().Trim();
                            var gioiTinh = worksheet.Cell(row, 4).Value.ToString().Trim();

                            // Nếu dòng nào mà Tên và SĐT đều trống thì bỏ qua luôn (tránh đọc nhầm dòng trắng)
                            if (string.IsNullOrEmpty(hoTen) && string.IsNullOrEmpty(sdt))
                                continue;

                            // 4. Tạo đối tượng Tiềm năng (Lead) mới
                            var newLead = new HocVien
                            {
                                HoTen = string.IsNullOrEmpty(hoTen) ? "Khách hàng mới" : hoTen,
                                SoDienThoai = sdt,
                                Email = email,
                                GioiTinh = string.IsNullOrEmpty(gioiTinh) ? "Chưa rõ" : gioiTinh,
                                TrangThai = 0, // 👉 0 = Tiềm năng (Chưa gọi)
                                NgayTao = DateTime.Now,
                                NgaySinh = new DateTime(2000, 1, 1) // Ngày sinh ảo mặc định tránh lỗi DB
                            };

                            // Add vào danh sách tạm
                            listLeads.Add(newLead);
                        }
                    }
                }

                // 5. Lưu toàn bộ danh sách vào Database 1 lần duy nhất (Cực nhanh)
                if (listLeads.Any())
                {
                    await _context.HocVien.AddRangeAsync(listLeads);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = $"✅ Đã nhập thành công {listLeads.Count} Tiềm năng mới vào hệ thống!";
                }
                else
                {
                    TempData["Warning"] = "⚠️ File Excel không có dữ liệu hợp lệ (hoặc trống)!";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "❌ Lỗi khi đọc file: Chắc chắn rằng bạn đang dùng đúng file mẫu. Chi tiết: " + ex.Message;
            }

            return RedirectToAction("Leads");
        }
        [HttpGet]
        public async Task<IActionResult> ExportExcel(int? status)
        {
            // 1. Lọc danh sách theo đúng tab đang đứng (Tiềm năng/Cơ hội/Học viên)
            var query = _context.HocVien
                .Include(h => h.LopHoc)
                .Include(h => h.NhanVien)
                .Include(h => h.ChienDich)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(h => h.TrangThai == status.Value);
            }

            var danhSach = await query.OrderByDescending(h => h.NgayTao).ToListAsync();

            // 2. Dùng ClosedXML tạo file Excel
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Danh_Sach_Data");

                // Đổ tiêu đề cột
                worksheet.Cell(1, 1).Value = "STT";
                worksheet.Cell(1, 2).Value = "Họ và Tên";
                worksheet.Cell(1, 3).Value = "Số điện thoại";
                worksheet.Cell(1, 4).Value = "Email";
                worksheet.Cell(1, 5).Value = "Giới tính";
                worksheet.Cell(1, 6).Value = "Ngày sinh";
                worksheet.Cell(1, 7).Value = "Lớp học";
                worksheet.Cell(1, 8).Value = "Nguồn (Chiến dịch)";
                worksheet.Cell(1, 9).Value = "Sale phụ trách";
                worksheet.Cell(1, 10).Value = "Ngày tạo";

                // Trang trí cho dòng tiêu đề nổi bật
                var headerRow = worksheet.Row(1);
                headerRow.Style.Font.Bold = true;
                headerRow.Style.Fill.BackgroundColor = XLColor.Teal;
                headerRow.Style.Font.FontColor = XLColor.White;
                headerRow.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // 3. Chạy vòng lặp để đắp dữ liệu vào các dòng bên dưới
                int row = 2;
                int stt = 1;
                foreach (var item in danhSach)
                {
                    worksheet.Cell(row, 1).Value = stt++;
                    worksheet.Cell(row, 2).Value = item.HoTen;
                    worksheet.Cell(row, 3).Value = "'" + item.SoDienThoai; // Thêm dấu nháy để Excel không mất số 0 ở đầu
                    worksheet.Cell(row, 4).Value = item.Email;
                    worksheet.Cell(row, 5).Value = item.GioiTinh;
                    worksheet.Cell(row, 6).Value = item.NgaySinh.ToString("dd/MM/yyyy");
                    worksheet.Cell(row, 7).Value = item.LopHoc?.TenLop ?? "Chưa có";
                    worksheet.Cell(row, 8).Value = item.ChienDich?.TenChienDich ?? "Tự nhiên";
                    worksheet.Cell(row, 9).Value = item.NhanVien?.HoTen ?? "Admin";
                    worksheet.Cell(row, 10).Value = item.NgayTao.ToString("dd/MM/yyyy");
                    row++;
                }

                // Tự động căn chỉnh độ rộng cột cho đẹp
                worksheet.Columns().AdjustToContents();

                // 4. Trả file về cho trình duyệt tải xuống
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();

                    // Đặt tên file linh hoạt theo trạng thái
                    string fileName = status == 0 ? "DanhSach_TiemNang.xlsx" :
                                      status == 1 ? "DanhSach_CoHoi.xlsx" :
                                      status == 2 ? "DanhSach_HocVien.xlsx" : "DanhSach_TongHop.xlsx";

                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
        }
        [HttpGet]
        public IActionResult DownloadTemplate()
        {
            // Tạo một file Excel ảo trong bộ nhớ
            using (var workbook = new XLWorkbook())
            {
                // Đặt tên cho Sheet
                var worksheet = workbook.Worksheets.Add("Mau_Nhap_Tiem_Nang");

                // 1. TẠO DÒNG TIÊU ĐỀ (Cột 1 đến 4 tương ứng A, B, C, D)
                worksheet.Cell(1, 1).Value = "Họ và Tên (*)";
                worksheet.Cell(1, 2).Value = "Số điện thoại (*)";
                worksheet.Cell(1, 3).Value = "Email";
                worksheet.Cell(1, 4).Value = "Giới tính (Nam/Nữ)";

                // Trang trí dòng tiêu đề cho đẹp (In đậm, nền xanh lá, chữ trắng)
                var headerRow = worksheet.Row(1);
                headerRow.Style.Font.Bold = true;
                headerRow.Style.Font.FontColor = XLColor.White;
                headerRow.Style.Fill.BackgroundColor = XLColor.DarkCyan;
                headerRow.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // 2. TẠO 2 DÒNG DỮ LIỆU MẪU ĐỂ SALE BIẾT ĐƯỜNG NHẬP
                worksheet.Cell(2, 1).Value = "Nguyễn Văn A";
                worksheet.Cell(2, 2).Value = "0987654321";
                worksheet.Cell(2, 3).Value = "nguyenvana@gmail.com";
                worksheet.Cell(2, 4).Value = "Nam";

                worksheet.Cell(3, 1).Value = "Trần Thị B";
                worksheet.Cell(3, 2).Value = "0912345678";
                worksheet.Cell(3, 3).Value = "tranthib@gmail.com";
                worksheet.Cell(3, 4).Value = "Nữ";

                // Ghi chú thêm cho Sale
                worksheet.Cell(5, 1).Value = "LƯU Ý:";
                worksheet.Cell(5, 1).Style.Font.Bold = true;
                worksheet.Cell(5, 1).Style.Font.FontColor = XLColor.Red;
                worksheet.Cell(6, 1).Value = "- Không được xóa dòng tiêu đề đầu tiên.";
                worksheet.Cell(7, 1).Value = "- Họ tên và Số điện thoại là bắt buộc phải nhập.";

                // Tự động kéo giãn độ rộng các cột cho vừa chữ
                worksheet.Columns().AdjustToContents();

                // 3. XUẤT THÀNH FILE .XLSX TRẢ VỀ CHO TRÌNH DUYỆT TẢI XUỐNG
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                    // Tên file khi tải về máy
                    return File(content, contentType, "FileMau_NhapTiemNang.xlsx");
                }
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        // 👉 ĐÃ THÊM: tham số string lyDoGiamGia
        public async Task<IActionResult> EnrollStudent(int id, int lopHocId, decimal hocPhi, decimal giamTru, string lyDoGiamGia, decimal daNop, string hinhThuc, string ghiChu)
        {
            var hocVien = await _context.HocVien.FindAsync(id);
            if (hocVien == null) return NotFound();

            try
            {
                hocVien.TrangThai = 2;
                hocVien.LopHocId = lopHocId;
                hocVien.MaHocVien = "HV" + DateTime.Now.ToString("yyMMddHHmm");
                hocVien.NgayTao = DateTime.Now;
                _context.Update(hocVien);

                if (daNop > 0)
                {
                    // Xử lý câu chữ cho Kế toán dễ đọc
                    string textGiamGia = giamTru > 0 ? $" | Đã giảm: {giamTru:N0}đ (Lý do: {lyDoGiamGia})" : "";

                    var phieuThu = new PhieuThu
                    {
                        MaPhieu = "PT" + DateTime.Now.ToString("yyMMddHHmm"),
                        HocVienId = hocVien.Id,
                        LopHocId = lopHocId,
                        SoTien = daNop,
                        NgayThu = DateTime.Now,
                        // Nối lý do giảm giá vào đây
                        NoiDung = $"Thu học phí. Ghi chú GV: {ghiChu}{textGiamGia}",
                        NguoiThu = HttpContext.Session.GetString("UserName") ?? "Admin",
                        HinhThuc = hinhThuc == "Tiền mặt" ? 0 : 1
                    };
                    _context.PhieuThu.Add(phieuThu);
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "🎉 CHÚC MỪNG! Đã chốt thành công học viên mới!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "❌ Lỗi khi làm thủ tục: " + ex.Message;
            }

            return RedirectToAction(nameof(Details), new { id = id });
        }
    }
}