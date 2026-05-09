using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ITPRO_CRM.Data;
using ITPRO_CRM.Models;
using OfficeOpenXml;
using System.IO;


namespace ITPRO_CRM.Controllers
{
    public class PhieuThusController : Controller
    {
        private readonly ITPRO_CRMContext _context;

        public PhieuThusController(ITPRO_CRMContext context)
        {
            _context = context;
        }

        // GET: PhieuThus
        public async Task<IActionResult> Index()
        {
            // 1. Lấy danh sách phiếu thu để hiển thị ở bảng
            var iTPRO_CRMContext = _context.PhieuThu.Include(p => p.HocVien).Include(p => p.LopHoc);
            var listPhieuThu = await iTPRO_CRMContext.ToListAsync();

            // 2. Tính Tổng học phí dự kiến thu của toàn bộ học viên chính thức (Trạng thái 2)
            // Lưu ý: Sum học phí từ bảng Lớp học mà học viên đó đang tham gia
            var tongHocPhiDuKien = await _context.HocVien
                .Where(h => h.TrangThai == 2 && h.LopHocId != null)
                .SumAsync(h => h.LopHoc.HocPhi);

            // 3. Tính Tổng số tiền thực tế đã thu được (từ các phiếu thu)
            var tongThucThu = listPhieuThu.Sum(x => x.SoTien);

            // 4. Công nợ = Dự kiến - Thực thu
            ViewBag.CongNo = tongHocPhiDuKien - tongThucThu;

            return View(listPhieuThu);
        }

        // GET: PhieuThus/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var phieuThu = await _context.PhieuThu
                .Include(p => p.HocVien)
                .Include(p => p.LopHoc)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (phieuThu == null)
            {
                return NotFound();
            }

            return View(phieuThu);
        }

        // GET: PhieuThus/Create
        // GET: PhieuThus/Create
        // GET: PhieuThus/Create
        public IActionResult Create()
        {
            // Lấy dữ liệu học viên
            var danhSachHocVien = _context.HocVien
                .Select(h => new
                {
                    Id = h.Id,
                    MaHocVien = string.IsNullOrEmpty(h.MaHocVien) ? "" : h.MaHocVien,
                    HoTen = h.HoTen + (string.IsNullOrEmpty(h.SoDienThoai) ? "" : $" ({h.SoDienThoai})")
                }).ToList();

            // 👉 GỬI 2 DANH SÁCH SANG VIEW
            ViewBag.ListMaHV = new SelectList(danhSachHocVien, "Id", "MaHocVien");
            ViewBag.HocVienId = new SelectList(danhSachHocVien, "Id", "HoTen");
            ViewBag.LopHocId = new SelectList(_context.LopHoc, "Id", "TenLop");

            return View();
        }

        // POST: PhieuThus/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,HocVienId,LopHocId,SoTien,NgayThu,NoiDung,NguoiThu,HinhThuc")] PhieuThu phieuThu)
        {
            // Tự sinh mã phiếu: PT + yyyyMMddHHmm (Ví dụ: PT202601280930)
            phieuThu.MaPhieu = "PT" + DateTime.Now.ToString("yyyyMMddHHmm");

            if (ModelState.IsValid)
            {
                _context.Add(phieuThu);
                await _context.SaveChangesAsync();
                // Sau khi thu tiền xong, quay về trang danh sách phiếu thu
                return RedirectToAction(nameof(Index));
            }
            ViewData["HocVienId"] = new SelectList(_context.HocVien, "Id", "HoTen", phieuThu.HocVienId);
            ViewData["LopHocId"] = new SelectList(_context.LopHoc, "Id", "TenLop", phieuThu.LopHocId);
            return View(phieuThu);
        }

        // GET: PhieuThus/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var phieuThu = await _context.PhieuThu.FindAsync(id);
            if (phieuThu == null)
            {
                return NotFound();
            }
            ViewData["HocVienId"] = new SelectList(_context.HocVien, "Id", "HoTen", phieuThu.HocVienId);
            ViewData["LopHocId"] = new SelectList(_context.LopHoc, "Id", "TenLop", phieuThu.LopHocId);
            return View(phieuThu);
        }

        // POST: PhieuThus/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,MaPhieu,HocVienId,LopHocId,SoTien,NgayThu,NoiDung,NguoiThu,HinhThuc")] PhieuThu phieuThu)
        {
            if (id != phieuThu.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(phieuThu);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PhieuThuExists(phieuThu.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["HocVienId"] = new SelectList(_context.HocVien, "Id", "HoTen", phieuThu.HocVienId);
            ViewData["LopHocId"] = new SelectList(_context.LopHoc, "Id", "TenLop", phieuThu.LopHocId);
            return View(phieuThu);
        }

        // GET: PhieuThus/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var phieuThu = await _context.PhieuThu
                .Include(p => p.HocVien)
                .Include(p => p.LopHoc)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (phieuThu == null)
            {
                return NotFound();
            }

            return View(phieuThu);
        }

        // POST: PhieuThus/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var phieuThu = await _context.PhieuThu.FindAsync(id);
            if (phieuThu != null)
            {
                _context.PhieuThu.Remove(phieuThu);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PhieuThuExists(int id)
        {
            return _context.PhieuThu.Any(e => e.Id == id);
        }
        [HttpGet]
        public async Task<IActionResult> ExportExcel(string search, int? hinhThuc)
        {
            try
            {
                var query = _context.PhieuThu.Include(p => p.HocVien).AsQueryable();

                // Lọc theo hình thức (0: Tiền mặt, 1: Chuyển khoản)
                if (hinhThuc.HasValue)
                {
                    query = query.Where(p => p.HinhThuc == hinhThuc.Value);
                }

                // Lọc theo từ khóa (Mã phiếu, Tên HV, Nội dung)
                if (!string.IsNullOrEmpty(search))
                {
                    search = search.ToLower();
                    query = query.Where(p => p.MaPhieu.ToLower().Contains(search) ||
                                             p.NoiDung.ToLower().Contains(search) ||
                                             (p.HocVien != null && p.HocVien.HoTen.ToLower().Contains(search)));
                }

                var danhSach = await query.OrderByDescending(p => p.NgayThu).ToListAsync();

                // Thiết lập bản quyền EPPlus
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (var package = new ExcelPackage())
                {
                    var ws = package.Workbook.Worksheets.Add("Bao_Cao_Doanh_Thu");

                    // Tiêu đề
                    string[] heads = { "STT", "Mã Phiếu", "Ngày Thu", "Học Viên", "Nội Dung", "Số Tiền", "Hình Thức", "Người Thu" };
                    for (int i = 0; i < heads.Length; i++)
                    {
                        ws.Cells[1, i + 1].Value = heads[i];
                        ws.Cells[1, i + 1].Style.Font.Bold = true;
                        ws.Cells[1, i + 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        ws.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Teal);
                        ws.Cells[1, i + 1].Style.Font.Color.SetColor(System.Drawing.Color.White);
                    }

                    int row = 2;
                    int stt = 1;
                    foreach (var item in danhSach)
                    {
                        ws.Cells[row, 1].Value = stt++;
                        ws.Cells[row, 2].Value = item.MaPhieu;
                        ws.Cells[row, 3].Value = item.NgayThu.ToString("dd/MM/yyyy HH:mm");
                        ws.Cells[row, 4].Value = item.HocVien?.HoTen ?? "Khách vãng lai";
                        ws.Cells[row, 5].Value = item.NoiDung;

                        // Định dạng tiền tệ cho Excel để Kế toán sum() được
                        ws.Cells[row, 6].Value = item.SoTien;
                        ws.Cells[row, 6].Style.Numberformat.Format = "#,##0";

                        ws.Cells[row, 7].Value = item.HinhThuc == 1 ? "Chuyển khoản" : "Tiền mặt";
                        ws.Cells[row, 8].Value = item.NguoiThu;
                        row++;
                    }

                    ws.Columns.AutoFit(); // Tự động căn chỉnh độ rộng

                    using (var stream = new MemoryStream())
                    {
                        package.SaveAs(stream);
                        var content = stream.ToArray();
                        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"PhieuThu_{DateTime.Now:ddMMyyyy}.xlsx");
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "❌ Lỗi xuất Excel: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
