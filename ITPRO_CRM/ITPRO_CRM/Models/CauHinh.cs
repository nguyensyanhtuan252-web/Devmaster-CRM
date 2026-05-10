using System.ComponentModel.DataAnnotations;

namespace ITPRO_CRM.Models
{
    public class CauHinh
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Tên trung tâm")]
        [MaxLength(200)]
        public string? TenTrungTam { get; set; } = "ITPRO ACADEMY";

        [Display(Name = "Địa chỉ")]
        [MaxLength(500)]
        public string? DiaChi { get; set; } = "Hà Nội";

        [Display(Name = "Logo (đường dẫn)")]
        [MaxLength(500)]
        public string? LogoUrl { get; set; }

        [Display(Name = "Số điện thoại chính")]
        [MaxLength(20)]
        public string? SoDienThoai { get; set; }

        [Display(Name = "Hotline dự phòng")]
        [MaxLength(20)]
        public string? HotlineDuPhong { get; set; }

        [Display(Name = "Website")]
        [MaxLength(200)]
        public string? Website { get; set; }

        [Display(Name = "Facebook Page")]
        [MaxLength(200)]
        public string? Facebook { get; set; }

        [Display(Name = "Kênh YouTube")]
        [MaxLength(200)]
        public string? Youtube { get; set; }

        [Display(Name = "TikTok")]
        [MaxLength(200)]
        public string? TikTok { get; set; }

        [Display(Name = "Ngôn ngữ hệ thống")]
        [MaxLength(20)]
        public string? NgonNgu { get; set; } = "vi";

        [Display(Name = "Múi giờ")]
        [MaxLength(50)]
        public string? MuiGio { get; set; } = "GMT+7";

        [Display(Name = "Đơn vị tiền tệ")]
        [MaxLength(10)]
        public string? DonViTienTe { get; set; } = "VND";

        [Display(Name = "Định dạng ngày")]
        [MaxLength(20)]
        public string? DinhDangNgay { get; set; } = "DD/MM/YYYY";

        [Display(Name = "Thời gian hết phiên (phút)")]
        [Range(15, 480)]
        public int ThoiGianHetPhien { get; set; } = 60;

        [Display(Name = "Số phiên tối đa / tài khoản")]
        [Range(1, 10)]
        public int SoPhienToiDa { get; set; } = 3;

        [Display(Name = "Tần suất sao lưu")]
        [MaxLength(20)]
        public string? TanSuatSaoLuu { get; set; } = "weekly";

        [Display(Name = "Lưu trữ bản sao lưu (ngày)")]
        [Range(7, 365)]
        public int LuuTruSaoLuu { get; set; } = 30;
    }
}