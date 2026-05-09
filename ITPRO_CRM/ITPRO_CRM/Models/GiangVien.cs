using System.ComponentModel.DataAnnotations;

namespace ITPRO_CRM.Models
{
    public class GiangVien
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Họ và tên")]
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        public string HoTen { get; set; } = string.Empty;

        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Display(Name = "Số điện thoại")]
        public string? SoDienThoai { get; set; }

        [Display(Name = "Chuyên môn chính")]
        public string? ChuyenMon { get; set; }

        [Display(Name = "Giới thiệu/Tiểu sử")]
        public string? TieuSu { get; set; }

        [Display(Name = "Link Ảnh")]
        public string? AnhDaiDien { get; set; }

        // --- CÁC CỘT BỔ SUNG ĐỂ KHỚP VỚI GIAO DIỆN ---

        [Display(Name = "Kinh nghiệm")]
        public string? KinhNghiem { get; set; }

        [Display(Name = "Thành tích")]
        public string? ThanhTich { get; set; }

        // 👇 Dòng này để sửa lỗi mới nhất của bạn:
        [Display(Name = "Bằng cấp")]
        public string? BangCap { get; set; }

        [Display(Name = "Trạng thái")]
        public int TrangThai { get; set; } = 1;
    }
}