using ITPRO_CRM.Data;
using ITPRO_CRM.Models;
using ITPRO_CRM.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ITPRO_CRM.Controllers
{
    public class EmailMarketingController : Controller
    {
        private readonly ITPRO_CRMContext _context;
        private readonly IEmailService _emailService;

        public EmailMarketingController(ITPRO_CRMContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // 1. Danh sách lịch sử chiến dịch
        public async Task<IActionResult> Index()
        {
            var lichSu = await _context.EmailMarketing.OrderByDescending(e => e.NgayGui).ToListAsync();
            return View(lichSu);
        }

        // 2. Xem chi tiết nội dung thư đã gửi
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var email = await _context.EmailMarketing.FindAsync(id);
            if (email == null) return NotFound();
            return View(email);
        }

        // 3. Màn hình soạn thảo (GET)
        public IActionResult Create()
        {
            // Load danh sách lớp
            ViewBag.LopHocId = new SelectList(_context.LopHoc.Where(l => l.TrangThai == 1), "Id", "TenLop");

            // Load danh sách học viên cho Select2
            var hocViens = _context.HocVien
                .Where(h => !string.IsNullOrEmpty(h.Email))
                .Select(h => new { Id = h.Id, Display = h.HoTen + " (" + h.Email + ")" }).ToList();
            ViewBag.HocVienId = new SelectList(hocViens, "Id", "Display");

            // Load kho mẫu Email và chuyển sang JSON để JS xử lý
            var danhSachMau = _context.MauEmail.ToList();
            ViewBag.MauEmailId = new SelectList(danhSachMau, "Id", "TenMau");
            ViewBag.TemplatesJson = JsonConvert.SerializeObject(danhSachMau);

            return View();
        }

        // 4. Xử lý gửi Mail (POST)
        [HttpPost]
        public async Task<IActionResult> Create(EmailMarketing model, string LoaiDoiTuong, int? LopHocId, int? TrangThaiHocVien, List<int> SelectedHocVienIds)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Truy vấn cơ bản: chỉ lấy người có email
                    var query = _context.HocVien.AsQueryable();
                    query = query.Where(h => !string.IsNullOrEmpty(h.Email));

                    string moTaDoiTuong = "Toàn bộ học viên";

                    // Xử lý các bộ lọc
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
                                moTaDoiTuong = "Tệp: " + (TrangThaiHocVien.Value < labels.Length ? labels[TrangThaiHocVien.Value] : "Khác");
                            }
                            break;

                        case "DEBT":
                            // VÌ BẢNG HOCVIEN CHƯA CÓ TONGNOP NÊN TẠM THỜI CHƯA LỌC NỢ
                            // Bạn có thể xử lý logic nợ ở đây sau khi đã có cột tiền nong
                            moTaDoiTuong = "Học viên nợ phí (Chưa lọc)";
                            break;

                        case "CUSTOM":
                            if (SelectedHocVienIds != null && SelectedHocVienIds.Any())
                            {
                                query = query.Where(h => SelectedHocVienIds.Contains(h.Id));
                                moTaDoiTuong = $"Đích danh ({SelectedHocVienIds.Count} người)";
                            }
                            break;
                    }

                    var danhSachNhan = await query.Select(h => new { h.HoTen, h.Email }).ToListAsync();

                    if (!danhSachNhan.Any())
                    {
                        TempData["Error"] = "Không tìm thấy học viên nào phù hợp với bộ lọc này.";
                        return RedirectToAction(nameof(Create));
                    }

                    // Gửi Mail & Cá nhân hóa
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

                    // Lưu lịch sử
                    model.NgayGui = DateTime.Now;
                    model.DaGui = true;
                    model.DoiTuongGui = $"{moTaDoiTuong} ({success}/{danhSachNhan.Count} thành công)";

                    _context.Add(model);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = $"Đã thực hiện gửi xong tới {success} học viên.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Lỗi: " + ex.Message;
                }
            }
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> SendQuickEmail(int hocVienId, string tieuDe, string noiDung)
        {
            try
            {
                // 1. Tìm thông tin học viên
                var hocVien = await _context.HocVien.FindAsync(hocVienId);
                if (hocVien == null || string.IsNullOrEmpty(hocVien.Email))
                {
                    return Json(new { success = false, message = "Học viên này chưa cập nhật địa chỉ Email!" });
                }

                // 2. Thay thế tag cá nhân hóa và Gửi mail
                string body = noiDung.Replace("{{TenHocVien}}", hocVien.HoTen);
                await _emailService.SendEmailAsync(hocVien.Email, tieuDe, body);

                // 3. Lưu vào lịch sử chiến dịch để quản lý
                var history = new EmailMarketing
                {
                    TieuDe = tieuDe,
                    NoiDung = noiDung,
                    NgayGui = DateTime.Now,
                    DaGui = true,
                    DoiTuongGui = $"Cá nhân: {hocVien.HoTen} ({hocVien.Email})"
                };
                _context.Add(history);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Đã gửi email thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }
    }
}