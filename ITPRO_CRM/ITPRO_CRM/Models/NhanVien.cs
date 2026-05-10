using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITPRO_CRM.Models
{
    // 1. ĐỊNH NGHĨA CÁC VAI TRÒ (Enum)
    public enum LoaiVaiTro
    {
        [Display(Name = "Giám đốc / Admin")]
        Admin = 0,

        [Display(Name = "Nhân viên Sale")]
        Sale = 1,

        [Display(Name = "Kế toán")]
        KeToan = 2,

        [Display(Name = "Giảng viên")]
        GiangVien = 3
    }

    [Table("NhanVien")]
    public class NhanVien
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [Display(Name = "Họ và Tên")]
        public string HoTen { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [Display(Name = "Mật khẩu")]
        public string MatKhau { get; set; }

        [Display(Name = "Số điện thoại")]
        public string? SoDienThoai { get; set; }

        [Display(Name = "Ảnh đại diện")]
        public string? AnhDaiDien { get; set; }

        // 2. NÂNG CẤP TỪ int SANG Enum LoaiVaiTro
        [Required(ErrorMessage = "Vai trò là bắt buộc")]
        [Display(Name = "Vai trò")]
        public LoaiVaiTro VaiTro { get; set; } = LoaiVaiTro.Sale; // Mặc định tạo tài khoản mới sẽ là Sale

        [Display(Name = "Trạng thái")]
        public bool TrangThai { get; set; } = true;

        [Display(Name = "KPI Tháng")]
        public int? KpiThang { get; set; }
    }
}