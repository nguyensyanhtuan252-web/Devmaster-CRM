using System.ComponentModel.DataAnnotations;

namespace ITPRO_CRM.Models
{
    public class CauHinh
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Tên trung tâm")]
        public string? TenTrungTam { get; set; } = "ITPRO ACADEMY";

        [Display(Name = "Địa chỉ")]
        public string? DiaChi { get; set; } = "Hà Nội";

        [Display(Name = "Số điện thoại")]
        public string? SoDienThoai { get; set; }

        [Display(Name = "Email hệ thống")]
        public string? Email { get; set; }
        [Display(Name = "Email hiển thị")]
        public string? EmailHeThong { get; set; }

        [Display(Name = "Email gửi (SMTP User)")]
        public string? EmailGui { get; set; }

        [Display(Name = "Mật khẩu Email (App Password)")]
        public string? MatKhauEmail { get; set; }
    }
}