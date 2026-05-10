using ITPRO_CRM.Data;
using ITPRO_CRM.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Mail;

namespace ITPRO_CRM.Controllers
{
    public class AccessController : Controller
    {
        private readonly ITPRO_CRMContext _context;

        public AccessController(ITPRO_CRMContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("UserName") != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string user, string pass)
        {
            var nhanVien = await _context.NhanVien
                                .FirstOrDefaultAsync(n => n.Email == user && n.MatKhau == pass);

            if (nhanVien != null)
            {
                if (nhanVien.TrangThai == false)
                {
                    ViewBag.Error = "Tài khoản này đã bị khóa!";
                    return View();
                }

                HttpContext.Session.SetString("UserName", nhanVien.Email);
                HttpContext.Session.SetInt32("UserId", nhanVien.Id);
                HttpContext.Session.SetInt32("VaiTro", (int)nhanVien.VaiTro);
                HttpContext.Session.SetString("HoTen", nhanVien.HoTen);

                string roleName = nhanVien.VaiTro switch
                {
                    LoaiVaiTro.Admin => "Giám đốc / Admin",
                    LoaiVaiTro.Sale => "Nhân viên Sale",
                    LoaiVaiTro.KeToan => "Kế toán",
                    LoaiVaiTro.GiangVien => "Giảng viên",
                    _ => "Nhân viên"
                };
                HttpContext.Session.SetString("UserRole", roleName);

                return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewBag.Error = "Email hoặc mật khẩu không chính xác!";
                ViewBag.User = user;
                return View();
            }
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var user = await _context.NhanVien.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                ViewBag.Message = "Nếu email hợp lệ, một đường dẫn khôi phục sẽ được gửi đến hộp thư của bạn.";
                return View();
            }

            // 1. Tạo Token ngẫu nhiên và cho sống 15 phút
            user.ResetToken = Guid.NewGuid().ToString() + "-" + DateTime.Now.Ticks;
            user.ResetTokenExpiry = DateTime.Now.AddMinutes(15);
            _context.Update(user);
            await _context.SaveChangesAsync();

            // 2. Tạo link khôi phục
            var resetLink = Url.Action("ResetPassword", "Access", new { token = user.ResetToken }, Request.Scheme);

            // 3. THỰC HIỆN GỬI EMAIL THỰC TẾ
            try
            {
                // --- 🚨 CẤU HÌNH QUAN TRỌNG 🚨 ---
                // Tuấn thay 2 dòng này bằng thông tin của Tuấn thì mail mới gửi đi được nhé
                string fromEmail = "Nguyensyanhtuan252@gmail.com";
                string appPassword = "owkfvxoxnesrrasu"; // Mật khẩu ứng dụng 16 ký tự của Gmail
                // ---------------------------------

                var fromAddress = new MailAddress(fromEmail, "DEVMASTER CRM System");
                var toAddress = new MailAddress(user.Email);

                string subject = "🔑 Khôi phục mật khẩu tài khoản DEVMASTER CRM";
                string body = $@"
                    <div style='font-family: Inter, Arial, sans-serif; line-height: 1.6; color: #1e293b; max-width: 500px; margin: auto; padding: 20px; border: 1px solid #e2e8f0; border-radius: 12px;'>
                        <h2 style='color: #2d5be3; margin-top: 0;'>Khôi phục mật khẩu</h2>
                        <p>Chào <b>{user.HoTen}</b>,</p>
                        <p>Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản liên kết với email này.</p>
                        <p>Vui lòng click vào nút bên dưới để tạo mật khẩu mới. Lưu ý: Link này sẽ hết hạn sau <b>15 phút</b>.</p>
                        <div style='text-align: center; margin: 30px 0;'>
                            <a href='{resetLink}' style='background-color: #2d5be3; color: white; padding: 12px 24px; text-decoration: none; border-radius: 8px; font-weight: 600; display: inline-block;'>ĐẶT LẠI MẬT KHẨU</a>
                        </div>
                        <p style='font-size: 13px; color: #64748b;'>Nếu bạn không thực hiện yêu cầu này, hãy bỏ qua email này. Tài khoản của bạn vẫn được bảo mật.</p>
                        <hr style='border: none; border-top: 1px solid #f1f5f9; margin: 20px 0;'>
                        <p style='font-size: 12px; color: #94a3b8; text-align: center;'>Đây là email tự động từ hệ thống DEVMASTER CRM CRM. Vui lòng không phản hồi email này.</p>
                    </div>";

                var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromAddress.Address, appPassword)
                };

                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                })
                {
                    await smtp.SendMailAsync(message);
                }

                ViewBag.Message = "Đã gửi đường dẫn khôi phục mật khẩu! Vui lòng kiểm tra hộp thư đến (hoặc thư rác) của bạn.";
            }
            catch (Exception ex)
            {
                // Nếu cấu hình sai Email hoặc Pass ứng dụng, nó sẽ báo lỗi ở đây
                ViewBag.Error = "Lỗi hệ thống khi gửi Email: " + ex.Message;
            }

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login");

            var user = await _context.NhanVien.FirstOrDefaultAsync(u => u.ResetToken == token && u.ResetTokenExpiry > DateTime.Now);
            if (user == null)
            {
                ViewBag.Error = "Đường dẫn khôi phục không hợp lệ hoặc đã hết hạn. Vui lòng yêu cầu lại.";
                return View("ForgotPassword");
            }

            ViewBag.Token = token;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(string token, string newPassword, string confirmPassword)
        {
            var user = await _context.NhanVien.FirstOrDefaultAsync(u => u.ResetToken == token && u.ResetTokenExpiry > DateTime.Now);
            if (user == null)
            {
                ViewBag.Error = "Đường dẫn khôi phục đã hết hạn.";
                return View("ForgotPassword");
            }

            if (newPassword != confirmPassword)
            {
                ViewBag.Error = "Mật khẩu xác nhận không khớp!";
                ViewBag.Token = token;
                return View();
            }

            user.MatKhau = newPassword;
            user.ResetToken = null;
            user.ResetTokenExpiry = null;

            _context.Update(user);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đặt lại mật khẩu thành công! Vui lòng đăng nhập.";
            return RedirectToAction("Login");
        }
    }
}