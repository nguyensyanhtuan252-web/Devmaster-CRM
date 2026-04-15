using ITPRO_CRM.Data;
using ITPRO_CRM.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Mail;

namespace ITPRO_CRM.Controllers
{
    public class EmailMarketingController : Controller
    {
        private readonly ITPRO_CRMContext _context;

        public EmailMarketingController(ITPRO_CRMContext context)
        {
            _context = context;
        }

        // 1. Danh sách lịch sử gửi
        public async Task<IActionResult> Index()
        {
            return View(await _context.EmailMarketing.OrderByDescending(e => e.NgayGui).ToListAsync());
        }

        // 2. Màn hình soạn thảo
        public IActionResult Create()
        {
            return View();
        }

        // 3. Xử lý gửi Mail
        [HttpPost]
        public async Task<IActionResult> Create(EmailMarketing model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // A. Lấy cấu hình SMTP từ Database
                    var cauHinh = await _context.CauHinh.FirstOrDefaultAsync();
                    if (cauHinh == null || string.IsNullOrEmpty(cauHinh.EmailGui) || string.IsNullOrEmpty(cauHinh.MatKhauEmail))
                    {
                        TempData["Error"] = "Chưa cấu hình Email hệ thống! Vui lòng vào mục Thiết lập.";
                        return View(model);
                    }

                    // B. Lấy danh sách Email khách hàng (Demo lấy tất cả học viên)
                    var dsEmail = await _context.HocVien
                                    .Where(h => !string.IsNullOrEmpty(h.Email))
                                    .Select(h => h.Email)
                                    .ToListAsync();

                    if (dsEmail.Count == 0)
                    {
                        TempData["Error"] = "Không tìm thấy học viên nào có Email!";
                        return View(model);
                    }

                    // C. Cấu hình gửi
                    var smtpClient = new SmtpClient("smtp.gmail.com")
                    {
                        Port = 587,
                        Credentials = new NetworkCredential(cauHinh.EmailGui, cauHinh.MatKhauEmail),
                        EnableSsl = true,
                    };

                    // D. Gửi từng người (Hoặc dùng BCC để gửi nhanh)
                    foreach (var emailNhan in dsEmail)
                    {
                        var mailMessage = new MailMessage
                        {
                            From = new MailAddress(cauHinh.EmailGui, cauHinh.TenTrungTam),
                            Subject = model.TieuDe,
                            Body = model.NoiDung,
                            IsBodyHtml = true, // Cho phép gửi HTML
                        };
                        mailMessage.To.Add(emailNhan);

                        try { smtpClient.Send(mailMessage); }
                        catch { /* Bỏ qua lỗi nếu 1 người bị lỗi để gửi tiếp người sau */ }
                    }

                    // E. Lưu lịch sử
                    model.NgayGui = DateTime.Now;
                    model.DaGui = true;
                    model.DoiTuongGui = $"Toàn bộ danh sách ({dsEmail.Count} người)";

                    _context.Add(model);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Đã gửi chiến dịch thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Lỗi gửi mail: " + ex.Message;
                }
            }
            return View(model);
        }
    }
}