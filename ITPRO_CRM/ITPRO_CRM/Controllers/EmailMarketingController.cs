using ITPRO_CRM.Data;
using ITPRO_CRM.Models;
using ITPRO_CRM.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http; // 👈 Dùng Session
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ITPRO_CRM.Filters;

namespace ITPRO_CRM.Controllers
{
    [PhanQuyen(LoaiVaiTro.Admin, LoaiVaiTro.KeToan, LoaiVaiTro.Sale)]
    public class EmailMarketingController : Controller
    {
        private readonly ITPRO_CRMContext _context;
        private readonly IEmailService _emailService;

        public EmailMarketingController(ITPRO_CRMContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // INDEX: Xem lịch sử (Lưu ý: Bạn có thể lọc thêm để Sale chỉ thấy lịch sử gửi mail của mình)
        public async Task<IActionResult> Index()
        {
            var lichSu = await _context.EmailMarketing.OrderByDescending(e => e.NgayGui).ToListAsync();
            return View(lichSu);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var email = await _context.EmailMarketing.FindAsync(id);
            if (email == null) return NotFound();
            return View(email);
        }

        // 3. Màn hình soạn thảo (GET) - Đã cá nhân hóa danh sách học viên
        public IActionResult Create()
        {
            var roleId = HttpContext.Session.GetInt32("VaiTro");
            var userId = HttpContext.Session.GetInt32("UserId");

            ViewBag.LopHocId = new SelectList(_context.LopHoc.Where(l => l.TrangThai == 1), "Id", "TenLop");

            // 🔐 LỌC HỌC VIÊN: Sale chỉ thấy khách của mình trong ô chọn đích danh
            var hvQuery = _context.HocVien.Where(h => !string.IsNullOrEmpty(h.Email));
            if (roleId == (int)LoaiVaiTro.Sale)
            {
                hvQuery = hvQuery.Where(h => h.NhanVienId == userId);
            }

            var hocViens = hvQuery.Select(h => new { Id = h.Id, Display = h.HoTen + " (" + h.Email + ")" }).ToList();
            ViewBag.HocVienId = new SelectList(hocViens, "Id", "Display");

            var danhSachMau = _context.MauEmail.ToList();
            ViewBag.MauEmailId = new SelectList(danhSachMau, "Id", "TenMau");
            ViewBag.TemplatesJson = JsonConvert.SerializeObject(danhSachMau);

            return View();
        }

        // 4. Xử lý gửi Mail (POST) - Đã áp dụng bộ lọc cá nhân hóa cho mọi đối tượng
        [HttpPost]
        public async Task<IActionResult> Create(EmailMarketing model, string LoaiDoiTuong, int? LopHocId, int? TrangThaiHocVien, List<int> SelectedHocVienIds)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var roleId = HttpContext.Session.GetInt32("VaiTro");
                    var userId = HttpContext.Session.GetInt32("UserId");

                    // Query gốc: Chỉ lấy người có email
                    var query = _context.HocVien.Where(h => !string.IsNullOrEmpty(h.Email));

                    // 🔐 QUY TẮC CÁ NHÂN HÓA: Sale luôn bị kẹp bộ lọc NhanVienId
                    if (roleId == (int)LoaiVaiTro.Sale)
                    {
                        query = query.Where(h => h.NhanVienId == userId);
                    }

                    string moTaDoiTuong = "Tệp quản lý cá nhân";

                    switch (LoaiDoiTuong)
                    {
                        case "CLASS":
                            if (LopHocId.HasValue)
                            {
                                query = query.Where(h => h.LopHocId == LopHocId.Value);
                                var lop = await _context.LopHoc.FindAsync(LopHocId.Value);
                                moTaDoiTuong = "Lớp: " + (lop?.TenLop ?? "N/A");
                            }
                            break;
                        case "STATUS":
                            if (TrangThaiHocVien.HasValue)
                            {
                                query = query.Where(h => h.TrangThai == TrangThaiHocVien.Value);
                                string[] labels = { "Tiềm năng", "Đang học", "Tốt nghiệp", "Bảo lưu/Nghỉ" };
                                moTaDoiTuong = "Trạng thái: " + (TrangThaiHocVien.Value < labels.Length ? labels[TrangThaiHocVien.Value] : "Khác");
                            }
                            break;
                        case "CUSTOM":
                            if (SelectedHocVienIds != null && SelectedHocVienIds.Any())
                            {
                                query = query.Where(h => SelectedHocVienIds.Contains(h.Id));
                                moTaDoiTuong = $"Chọn thủ công ({SelectedHocVienIds.Count} người)";
                            }
                            break;
                    }

                    var danhSachNhan = await query.Select(h => new { h.HoTen, h.Email }).ToListAsync();

                    if (!danhSachNhan.Any())
                    {
                        TempData["Error"] = "Không tìm thấy học viên thuộc quyền quản lý của bạn khớp với bộ lọc.";
                        return RedirectToAction(nameof(Create));
                    }

                    int success = 0;
                    foreach (var hv in danhSachNhan)
                    {
                        try
                        {
                            string content = model.NoiDung.Replace("{{TenHocVien}}", hv.HoTen);
                            await _emailService.SendEmailAsync(hv.Email, model.TieuDe, content);
                            success++;
                        }
                        catch { }
                    }

                    model.NgayGui = DateTime.Now;
                    model.DaGui = true;
                    model.DoiTuongGui = $"{moTaDoiTuong} ({success}/{danhSachNhan.Count})";

                    _context.Add(model);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = $"Đã gửi thành công tới {success} học viên.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex) { TempData["Error"] = "Lỗi: " + ex.Message; }
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SendQuickEmail(int hocVienId, string tieuDe, string noiDung)
        {
            try
            {
                var roleId = HttpContext.Session.GetInt32("VaiTro");
                var userId = HttpContext.Session.GetInt32("UserId");

                var hocVien = await _context.HocVien.FindAsync(hocVienId);
                if (hocVien == null || string.IsNullOrEmpty(hocVien.Email))
                {
                    return Json(new { success = false, message = "Không tìm thấy Email học viên!" });
                }

                // 🔐 BẢO MẬT: Chặn Sale gửi mail cho học sinh người khác bằng AJAX
                if (roleId == (int)LoaiVaiTro.Sale && hocVien.NhanVienId != userId)
                {
                    return Json(new { success = false, message = "Bạn không có quyền tương tác với học viên này!" });
                }

                string body = noiDung.Replace("{{TenHocVien}}", hocVien.HoTen);
                await _emailService.SendEmailAsync(hocVien.Email, tieuDe, body);

                var history = new EmailMarketing
                {
                    TieuDe = tieuDe,
                    NoiDung = noiDung,
                    NgayGui = DateTime.Now,
                    DaGui = true,
                    DoiTuongGui = $"Cá nhân: {hocVien.HoTen}"
                };
                _context.Add(history);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Đã gửi email thành công!" });
            }
            catch (Exception ex) { return Json(new { success = false, message = "Lỗi: " + ex.Message }); }
        }
    }
}