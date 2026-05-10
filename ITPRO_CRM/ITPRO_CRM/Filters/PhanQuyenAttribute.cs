using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ITPRO_CRM.Models; // Gọi đến thư mục Models để lấy Enum LoaiVaiTro

namespace ITPRO_CRM.Filters
{
    public class PhanQuyenAttribute : ActionFilterAttribute
    {
        private readonly LoaiVaiTro[] _roles;

        // Cho phép truyền vào 1 hoặc nhiều quyền
        public PhanQuyenAttribute(params LoaiVaiTro[] roles)
        {
            _roles = roles;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // 1. Lấy quyền của người dùng đang đăng nhập từ Session
            var roleSession = context.HttpContext.Session.GetInt32("VaiTro");

            // Nếu chưa đăng nhập -> Đá về trang Login
            if (roleSession == null)
            {
                context.Result = new RedirectToActionResult("Login", "Access", null);
                return;
            }

            LoaiVaiTro currentRole = (LoaiVaiTro)roleSession;

            // 2. GIÁM ĐỐC / ADMIN thì có "Kim bài miễn tử", luôn được qua cổng
            if (currentRole == LoaiVaiTro.Admin)
            {
                base.OnActionExecuting(context);
                return;
            }

            // 3. Nếu không phải Admin, kiểm tra xem quyền có nằm trong danh sách cho phép không
            if (!_roles.Contains(currentRole))
            {
                // Nếu không có quyền -> Đá về trang báo lỗi (Hoặc trang chủ)
                context.Result = new RedirectToActionResult("Index", "Home", new { error = "unauthorized" });
            }

            base.OnActionExecuting(context);
        }
    }
}