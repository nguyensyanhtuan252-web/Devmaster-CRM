using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ITPRO_CRM.Data;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace ITPRO_CRM.Controllers
{
    public class ThongBaosController : Controller
    {
        private readonly ITPRO_CRMContext _context;

        public ThongBaosController(ITPRO_CRMContext context)
        {
            _context = context;
        }

        // Hàm này chạy khi User bấm vào 1 thông báo trên quả chuông
        public async Task<IActionResult> Read(int id)
        {
            var tb = await _context.ThongBao.FindAsync(id);
            if (tb != null)
            {
                // 1. Đánh dấu đã đọc
                tb.DaDoc = true;
                _context.Update(tb);
                await _context.SaveChangesAsync();

                // 2. Nhảy đến trang chi tiết
                if (!string.IsNullOrEmpty(tb.LinkUrl))
                {
                    return Redirect(tb.LinkUrl);
                }
            }
            return RedirectToAction("Index", "Home");
        }

        // API dùng cho AJAX để lấy danh sách thông báo thả xuống quả chuông
        [HttpGet]
        public async Task<IActionResult> GetMyNotifications()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return BadRequest();

            var thongBaos = await _context.ThongBao
                .Where(t => t.NhanVienId == userId)
                .OrderByDescending(t => t.NgayTao)
                .Take(10) // Lấy 10 thông báo mới nhất
                .Select(t => new {
                    t.Id,
                    t.TieuDe,
                    t.NoiDung,
                    t.DaDoc,
                    NgayTao = t.NgayTao.ToString("dd/MM HH:mm")
                })
                .ToListAsync();

            var unreadCount = await _context.ThongBao.CountAsync(t => t.NhanVienId == userId && !t.DaDoc);

            return Json(new { unreadCount, notifications = thongBaos });
        }
    }
}