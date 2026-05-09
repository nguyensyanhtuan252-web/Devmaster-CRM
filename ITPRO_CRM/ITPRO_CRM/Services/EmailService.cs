using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;

namespace ITPRO_CRM.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var emailSettings = _config.GetSection("EmailSettings");

            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(emailSettings["DisplayName"], emailSettings["Email"]));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = body };
            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            try
            {
                // Kết nối đến server Gmail
                await smtp.ConnectAsync(emailSettings["Host"], int.Parse(emailSettings["Port"]), SecureSocketOptions.StartTls);

                // Đăng nhập bằng chìa khóa 16 chữ cái
                await smtp.AuthenticateAsync(emailSettings["Email"], emailSettings["Password"]);

                // Thực hiện gửi
                await smtp.SendAsync(email);
            }
            catch (Exception ex)
            {
                // Log lỗi nếu có (Tuấn có thể xem lỗi ở đây nếu gửi thất bại)
                Console.WriteLine("Lỗi gửi mail: " + ex.Message);
                throw;
            }
            finally
            {
                await smtp.DisconnectAsync(true);
            }
        }
    }
}